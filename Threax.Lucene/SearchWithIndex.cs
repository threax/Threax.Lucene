using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Threax.Lucene
{
    /// <summary>
    /// Provides a lucene search index that will delete files that are not in the update set based on id.
    /// </summary>
    /// <typeparam name="SearchResult"></typeparam>
    public abstract class SearchWithIndex<SearchResult> : SearchBase<SearchResult>
    {
        private String idField;
        private List<Guid> currentIndexDocs;

        public SearchWithIndex(String idField, ILuceneDirectoryProvider directoryProvider, LuceneServiceOptions options) 
            : base(directoryProvider, options)
        {
            this.idField = idField;
        }

        protected async Task Index(Func<IndexWriter, List<Guid>, Task> loadData)
        {
            //Get curent documents so extra ones can be erased
            currentIndexDocs = new List<Guid>(0);
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
                        currentIndexDocs.Add(Guid.Parse(doc.Get(idField)));
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
}
