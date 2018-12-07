using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Lucene
{
    public class NGramAnalyzer : Analyzer
    {
        private LuceneVersion Version;
        private int minGram;
        private int maxGram;

        public NGramAnalyzer(LuceneVersion version, int minGram, int maxGram)
        {
            this.Version = version;
            this.minGram = minGram;
            this.maxGram = maxGram;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, System.IO.TextReader reader)
        {
            var nGramTokenizer = new NGramTokenizer(Version, reader, minGram, maxGram);
            var nGramTokenFilter = new NGramTokenFilter(Version, new LowerCaseFilter(Version, nGramTokenizer), minGram, maxGram);
            return new TokenStreamComponents(nGramTokenizer, nGramTokenFilter);
        }
    }
}
