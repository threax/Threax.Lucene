using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Lucene
{
    public class IndexSearcherManager : IDisposable
    {
        private SearcherManager manager;

        internal IndexSearcherManager(SearcherManager manager)
        {
            this.manager = manager;
            manager.MaybeRefreshBlocking();
            this.Searcher = manager.Acquire();
        }

        public void Dispose()
        {
            this.manager.Release(Searcher);
        }

        public IndexSearcher Searcher { get; private set; }
    }
}
