using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Store;

namespace Threax.Lucene
{
    public class FileDirectoryProvider<T> : ILuceneDirectoryProvider<T>
    {
        String indexPath;

        public FileDirectoryProvider(LuceneServiceOptions<T> options)
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

    public class FileDirectoryProvider : FileDirectoryProvider<GenericSearchPlaceholder>, ILuceneDirectoryProvider
    {
        public FileDirectoryProvider(LuceneServiceOptions options) : base(options)
        {
        }
    }
}
