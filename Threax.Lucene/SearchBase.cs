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
    /// <typeparam name="SearchResult"></typeparam>
    public abstract class SearchBase<SearchResult> : IDisposable
    {
        protected const LuceneVersion version = LuceneVersion.LUCENE_48;

        private Directory directory;
        private Analyzer analyzer;
        private SearcherManager searchManager;
        private String[] queryFields;

        //Get strange errors with the query parser as if it is not thread safe, use a pool and separate them out per request.
        private ConcurrentBag<QueryParser> parserPool = new ConcurrentBag<QueryParser>();

        private int maxResults;

        public SearchBase(ILuceneDirectoryProvider directoryProvider, LuceneServiceOptions options)
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

        protected SearcherManager SearchManager { get => searchManager;}

        public IEnumerable<SearchResult> Search(String query)
        {
            EnsureSearchManager(true);

            QueryParser queryParser = null;
            searchManager.MaybeRefreshBlocking();
            IndexSearcher searcher = searchManager.Acquire();
            try
            {
                if (!parserPool.TryTake(out queryParser))
                {
                    queryParser = new MultiFieldQueryParser(version, queryFields, analyzer);
                }

                var lQuery = queryParser.Parse(query);

                var hits = searcher.Search(lQuery, maxResults);

                foreach (var scoreDoc in hits.ScoreDocs)
                {
                    var doc = searcher.Doc(scoreDoc.Doc);
                    yield return CreateResult(scoreDoc, doc);
                }
            }
            finally
            {
                searchManager.Release(searcher);
                if (queryParser != null)
                {
                    parserPool.Add(queryParser);
                }
            }
        }

        protected abstract SearchResult CreateResult(ScoreDoc scoreDoc, Document doc);

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
}
