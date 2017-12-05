using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvLoader3.Models
{
    public interface IUnitOfWork : IDisposable
    {
        IEntityRepository<T> GetRepository<T>();
        void Save();
    }
}

