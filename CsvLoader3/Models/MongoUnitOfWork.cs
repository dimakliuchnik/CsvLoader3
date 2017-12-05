using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CsvLoader3.Models
{
    public class MongoUnitOfWork : IUnitOfWork
    {
        private readonly MongoContext _db = new MongoContext();
        private static readonly Dictionary<Type, IRepository> Repositories = new Dictionary<Type, IRepository>();
        private bool _disposed = false;

        public IEntityRepository<T> GetRepository<T>()
        {
            var t = typeof(T);
            if (!Repositories.ContainsKey(t))
            {
                var repository = new MongoEntityRepository<T>(_db);
                Repositories.Add(t, repository);
            }

            return Repositories[t] as IEntityRepository<T>;
  
        }

        ~MongoUnitOfWork()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Save()
        {
            _db.SaveChanges();
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
                this._disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}