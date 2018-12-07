using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Store;

namespace Threax.Lucene
{
    public class FileDirectoryProvider : ILuceneDirectoryProvider
    {
        String indexPath;

        public FileDirectoryProvider(LuceneServiceOptions options)
        {
            this.indexPath = options.IndexPath;
        }

        public Directory CreateDirectory()
        {
            if (!System.IO.Directory.Exists(indexPath))
            {
                System.IO.Directory.CreateDirectory(indexPath);
            }

            return FSDirectory.Open(indexPath);
        }
    }
}
