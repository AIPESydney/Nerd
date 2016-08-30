using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerd.Api.Interfaces
{
    public interface ICacheProvider
    {
        T Get<T>();
        void Save<T>(T item, int cacheInMins = 0);
        void Clear<T>();
    }
}
