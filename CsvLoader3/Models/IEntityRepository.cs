using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace CsvLoader3.Models
{
    public interface IEntityRepository<T> : IRepository
    {
        List<T> GetAllObjectsList(); // get all objects // ienumerable
        void Create(T item); // create object
    }
}
