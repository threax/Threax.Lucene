using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Lucene
{
    public class EdgeNGramAnalyzer : Analyzer
    {
        private LuceneVersion Version;
        private int minGram;
        private int maxGram;

        public EdgeNGramAnalyzer(LuceneVersion version, int minGram, int maxGram)
        {
            this.Version = version;
            this.minGram = minGram;
            this.maxGram = maxGram;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, System.IO.TextReader reader)
        {
            var nGramTokenizer = new EdgeNGramTokenizer(Version, reader, minGram, maxGram);
            var nGramTokenFilter = new EdgeNGramTokenFilter(Version, new LowerCaseFilter(Version, nGramTokenizer), minGram, maxGram);
            return new TokenStreamComponents(nGramTokenizer, nGramTokenFilter);
        }
    }
}
