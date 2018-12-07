using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Store;

namespace Threax.Lucene
{
    public class RamDirectoryProvider : ILuceneDirectoryProvider
    {
        public Directory CreateDirectory()
        {
            return new RAMDirectory();
        }
    }
}
