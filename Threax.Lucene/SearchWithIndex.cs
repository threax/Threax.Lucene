using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Threax.Lucene
{
    /// <summary>
    /// Provides a lucene search index that will delete files that are not in the update set based on id.
    /// When using this be sure to write a field to your document called the value in IdField. Also be sure
    /// to use that as the term when you add the doc.
    /// <code>
    /// doc.Add(new StringField(IdField, "YourData", Field.Store.YES));
    /// writer.UpdateDocument(new Lucene.Net.Index.Term(IdField, "YourData"), doc); //Use the page links as ids, see if we can avoid the string
    /// </code>
    /// </summary>
    /// <typeparam name="SearchResult">The type of the search result.</typeparam>
    /// <typeparam name="TISearchService">The type of the search service, used to discover other dependencies.</typeparam>
    public abstract class SearchWithIndex<SearchResult, TISearchService, Id> : SearchBase<SearchResult, TISearchService>
    {
        private String idField;
        private List<Id> currentIndexDocs;

        public SearchWithIndex(String idField, ILuceneDirectoryProvider<TISearchService> directoryProvider, LuceneServiceOptions<TISearchService> options) 
            : base(directoryProvider, options)
        {
            this.idField = idField;
        }

        protected abstract Id GetId(String strId);

        protected String IdField { get => idField; }

        protected async Task Index(Func<IndexWriter, List<Id>, Task> loadData)
        {
            //Get curent documents so extra ones can be erased
            currentIndexDocs = new List<Id>(0);
            if (EnsureSearchManager())
            {
                SearchManager.MaybeRefreshBlocking();
                var searcher = SearchManager.Acquire();
                try
                {
                    var lQuery = new MatchAllDocsQuery();

                    var hits = searcher.Search(lQuery, int.MaxValue); //Please note that if you have more than int.maxvalue users this will start to fail, this is unlikely for this dataset
                    currentIndexDocs.Capacity = hits.TotalHits;
                    foreach (var scoreDoc in hits.ScoreDocs)
                    {
                        var doc = searcher.Doc(scoreDoc.Doc);
                        currentIndexDocs.Add(GetId(doc.Get(idField)));
                    }
                }
                finally
                {
                    SearchManager.Release(searcher);
                }
            }

            await base.Index(async writer =>
            {
                await loadData(writer, currentIndexDocs);

                //Any remaining index docs are no longer in the authority employee data, so erase them.
                foreach (var toDelete in currentIndexDocs)
                {
                    writer.DeleteDocuments(new Term(idField, toDelete.ToString()));
                }
            });
        }
    }

    public abstract class SearchWithIndex<SearchResult, Id> : SearchWithIndex<SearchResult, GenericSearchPlaceholder, Id>
    {
        public SearchWithIndex(string idField, ILuceneDirectoryProvider directoryProvider, LuceneServiceOptions options) : base(idField, directoryProvider, options)
        {
        }
    }

    /// <summary>
    /// A search index with a guid for its key.
    /// </summary>
    /// <typeparam name="SearchResult">The type of the search result.</typeparam>
    /// <typeparam name="TISearchService">The type of the search service, used to discover other dependencies.</typeparam>
    public abstract class SearchWithIndexGuid<SearchResult, TISearchService> : SearchWithIndex<SearchResult, TISearchService, Guid>
    {
        public SearchWithIndexGuid(string idField, ILuceneDirectoryProvider<TISearchService> directoryProvider, LuceneServiceOptions<TISearchService> options) : base(idField, directoryProvider, options)
        {
        }

        protected sealed override Guid GetId(string strId)
        {
            return Guid.Parse(strId);
        }
    }

    public abstract class SearchWithIndexGuid<SearchResult> : SearchWithIndexGuid<SearchResult, GenericSearchPlaceholder>
    {
        public SearchWithIndexGuid(string idField, ILuceneDirectoryProvider directoryProvider, LuceneServiceOptions options) : base(idField, directoryProvider, options)
        {
        }
    }

    /// <summary>
    /// A search index with an int for its key.
    /// </summary>
    /// <typeparam name="SearchResult">The type of the search result.</typeparam>
    /// <typeparam name="TISearchService">The type of the search service, used to discover other dependencies.</typeparam>
    public abstract class SearchWithIndexInt32<SearchResult, TISearchService> : SearchWithIndex<SearchResult, TISearchService, Int32>
    {
        public SearchWithIndexInt32(string idField, ILuceneDirectoryProvider<TISearchService> directoryProvider, LuceneServiceOptions<TISearchService> options) : base(idField, directoryProvider, options)
        {
        }

        protected sealed override Int32 GetId(string strId)
        {
            return Int32.Parse(strId);
        }
    }

    public abstract class SearchWithIndexInt32<SearchResult> : SearchWithIndexInt32<SearchResult, GenericSearchPlaceholder>
    {
        public SearchWithIndexInt32(string idField, ILuceneDirectoryProvider directoryProvider, LuceneServiceOptions options) : base(idField, directoryProvider, options)
        {
        }
    }

    /// <summary>
    /// A search index with a long for its key.
    /// </summary>
    /// <typeparam name="SearchResult">The type of the search result.</typeparam>
    /// <typeparam name="TISearchService">The type of the search service, used to discover other dependencies.</typeparam>
    public abstract class SearchWithIndexInt64<SearchResult, TISearchService> : SearchWithIndex<SearchResult, TISearchService, Int64>
    {
        public SearchWithIndexInt64(string idField, ILuceneDirectoryProvider<TISearchService> directoryProvider, LuceneServiceOptions<TISearchService> options) : base(idField, directoryProvider, options)
        {
        }

        protected sealed override Int64 GetId(string strId)
        {
            return Int64.Parse(strId);
        }
    }

    public abstract class SearchWithIndexInt64<SearchResult> : SearchWithIndexInt64<SearchResult, GenericSearchPlaceholder>
    {
        public SearchWithIndexInt64(string idField, ILuceneDirectoryProvider directoryProvider, LuceneServiceOptions options) : base(idField, directoryProvider, options)
        {
        }
    }

    /// <summary>
    /// A search index with a string for its key.
    /// </summary>
    /// <typeparam name="SearchResult">The type of the search result.</typeparam>
    /// <typeparam name="TISearchService">The type of the search service, used to discover other dependencies.</typeparam>
    public abstract class SearchWithIndexString<SearchResult, TISearchService> : SearchWithIndex<SearchResult, TISearchService, String>
    {
        public SearchWithIndexString(string idField, ILuceneDirectoryProvider<TISearchService> directoryProvider, LuceneServiceOptions<TISearchService> options) : base(idField, directoryProvider, options)
        {
        }

        protected sealed override String GetId(string strId)
        {
            return strId;
        }
    }

    public abstract class SearchWithIndexString<SearchResult> : SearchWithIndexString<SearchResult, GenericSearchPlaceholder>
    {
        public SearchWithIndexString(string idField, ILuceneDirectoryProvider directoryProvider, LuceneServiceOptions options) : base(idField, directoryProvider, options)
        {
        }
    }
}
