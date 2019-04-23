using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Lucene
{
    /// <summary>
    /// A lucene search base class for a search over multiple fields. The exact analyzer, data load and search results
    /// are up to the subclasses.
    /// </summary>
    /// <typeparam name="SearchResult">The type of the search result.</typeparam>
    /// <typeparam name="TISearchService">The type of the search service, used to discover other dependencies.</typeparam>
    public abstract class SearchBase<SearchResult, TISearchService> : IDisposable
    {
        protected const LuceneVersion version = LuceneVersion.LUCENE_48;

        private Directory directory;
        private Analyzer analyzer;
        private SearcherManager searchManager;
        private String[] queryFields;

        //Get strange errors with the query parser as if it is not thread safe, use a pool and separate them out per request.
        private ConcurrentBag<QueryParser> parserPool = new ConcurrentBag<QueryParser>();

        private int maxResults;

        public SearchBase(ILuceneDirectoryProvider<TISearchService> directoryProvider, LuceneServiceOptions<TISearchService> options)
        {
            maxResults = options.MaxResults;
            this.directory = directoryProvider.CreateDirectory();
            EnsureSearchManager();
        }

        public virtual void Dispose()
        {
            searchManager?.Dispose();
            analyzer.Dispose();
            directory.Dispose();
        }

        protected void SetConfig(Analyzer analyzer, IEnumerable<String> queryFields)
        {
            this.analyzer = analyzer;
            this.queryFields = queryFields.ToArray();
        }

        protected async Task Index(Func<IndexWriter, Task> loadData)
        {
            //Updated index from authority
            using (var writer = new IndexWriter(directory, new IndexWriterConfig(version, analyzer)))
            {
                await loadData(writer);

                writer.Flush(true, true);
                writer.Commit();
            }
        }

        protected internal SearcherManager SearchManager { get => this.searchManager; }

        protected int MaxResults { get => this.maxResults; }

        /// <summary>
        /// Get the search results for the given query. This is not async since you can do any async
        /// calls on the results of calling this function.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<SearchResult> Search(String query)
        {
            using(var queryParserManager = AcquireQueryParser())
            {
                var lQuery = queryParserManager.Parser.Parse(query);

                return Search(lQuery);
            }
        }

        /// <summary>
        /// Run a search with an optional sort and max results. This provides an easy way to search with a bit more control.
        /// If you need more control than this call AcquireSearcher (be sure to dispose it) and run the search yourself calling
        /// CreateResults with the hits.
        /// </summary>
        /// <param name="query">The lucene query to run.</param>
        /// <param name="maxResults">The max number of results. This must have a value or the default max will be used.</param>
        /// <param name="sort">The sort object to sort by.</param>
        /// <returns></returns>
        public IEnumerable<SearchResult> Search(Query query, int? maxResults = null, Sort sort = null)
        {
            EnsureSearchManager(true);

            using (var searcherManager = AcquireSearcher())
            {
                TopDocs hits;
                if (sort == null)
                {
                    hits = searcherManager.Searcher.Search(query, maxResults ?? this.maxResults);
                }
                else
                {
                    hits = searcherManager.Searcher.Search(query, maxResults ?? this.maxResults, sort);
                }

                return CreateResults(searcherManager.Searcher, hits);
            }
        }

        /// <summary>
        /// Get a IndexSearcher. Be sure to dispose the result of this function.
        /// </summary>
        /// <returns></returns>
        protected IndexSearcherManager AcquireSearcher()
        {
            EnsureSearchManager(true);
            return new IndexSearcherManager(searchManager);
        }

        /// <summary>
        /// Get a query parser that will query all fields. Be sure to dispose the returned QueryParserManager or the parsers
        /// will never return to the pool.
        /// </summary>
        /// <returns></returns>
        public QueryParserManager AcquireQueryParser()
        {
            EnsureSearchManager(true);
            return new QueryParserManager(parserPool, version, queryFields, analyzer);
        }

        protected abstract IEnumerable<SearchResult> CreateResults(IndexSearcher searcher, TopDocs hits);

        /// <summary>
        /// Ensure that a search manager is created, will return true if a searcher is created and usable, false if it is not.
        /// You can optionally throw an exception if the searcher cannot be created instead.
        /// Because of how this is implemetned it will keep trying to create SearcherMangaers until it succeeds.
        /// Failure is most often caused by there not being an index yet.
        /// </summary>
        /// <param name="throwOnError">Set this to true to throw an exception on an error instead of returning false.</param>
        /// <returns></returns>
        protected bool EnsureSearchManager(bool throwOnError = false)
        {
            if (searchManager == null)
            {
                try
                {
                    searchManager = new SearcherManager(directory, null);
                }
                catch (IndexNotFoundException)
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                    return false;
                }
            }
            return true;
        }
    }

    public abstract class SearchBase<SearchResult> : SearchBase<SearchResult, GenericSearchPlaceholder>
    {
        public SearchBase(ILuceneDirectoryProvider directoryProvider, LuceneServiceOptions options) : base(directoryProvider, options)
        {
        }
    }
}
