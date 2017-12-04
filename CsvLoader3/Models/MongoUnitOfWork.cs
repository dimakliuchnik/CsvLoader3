using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CsvLoader3.Models
{
    public class MongoUnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly System.Data.Entity.DbContext _db = new DbContext();
        private IRepository<FilesModel> _fileRepository;
        private IRepository<LoginModel> _loginRepository;
        private bool _disposed = false;

        public override IRepository<FilesModel> Files
        {
            get
            {
                if (_fileRepository == null)
                    _fileRepository = new MongoLoaderRepository();
                return _fileRepository;
            }
        }

        public override IRepository<LoginModel> LoginPasswords
        {
            get
            {
                if (_loginRepository == null)
                    _loginRepository = new MongoLoginRepository();
                return _loginRepository;
            }
        }

        public override void Save()
        {
            _db.SaveChanges();
        }


        public override void Dispose(bool disposing)
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

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}