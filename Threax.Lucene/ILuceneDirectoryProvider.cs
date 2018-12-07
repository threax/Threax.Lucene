using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Threax.AspNetCore.Lucene
{
    public interface ILuceneDirectoryProvider
    {
        Directory CreateDirectory();
    }
}
