using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Threax.Lucene
{
    public interface ILuceneDirectoryProvider<T>
    {
        Directory CreateDirectory();
    }

    public interface ILuceneDirectoryProvider : ILuceneDirectoryProvider<GenericSearchPlaceholder>
    {
        
    }
}
