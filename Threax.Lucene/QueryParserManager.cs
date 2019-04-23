using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Util;

namespace Threax.Lucene
{
    public class QueryParserManager : IDisposable
    {
        private ConcurrentBag<QueryParser> parserPool;
        private QueryParser queryParser = null;

        internal QueryParserManager(ConcurrentBag<QueryParser> parserPool, LuceneVersion version, String[] queryFields, Analyzer analyzer)
        {
            this.parserPool = parserPool;
            if (!parserPool.TryTake(out queryParser))
            {
                queryParser = new MultiFieldQueryParser(version, queryFields, analyzer);
            }
        }

        public QueryParser Parser { get => queryParser; }

        public void Dispose()
        {
            if (queryParser != null)
            {
                parserPool.Add(queryParser);
            }
        }
    }
}
