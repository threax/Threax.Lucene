using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Threax.AspNetCore.Lucene
{
    public class LuceneServiceOptions
    {
        public LuceneServiceOptions()
        {
            this.IndexPath = Path.GetFullPath("../LuceneIndex");
        }

        /// <summary>
        /// Set this to true to use a directory index stored at IndexPath. False will use a ram index, which
        /// is good for testing, but not as good for production.
        /// </summary>
        public bool UseDirectoryIndex { get; set; } = true;

        /// <summary>
        /// The path where the lucene index will be written. This defaults to ../LuceneIndex.
        /// </summary>
        public String IndexPath { get; set; }

        /// <summary>
        /// The maximum number of search results to return in one search. No matter how many results there are no
        /// more than this value will be returned. Defaults to 100.
        /// </summary>
        public int MaxResults { get; set; } = 100;
    }
}
