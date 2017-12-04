using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvLoader3.Models
{
    public abstract class IUnitOfWork
    {
        private readonly System.Data.Entity.DbContext _db = new DbContext();
        private IRepository<FilesModel> _fileRepository;
        private IRepository<LoginModel> _loginRepository;
        public abstract IRepository<FilesModel> Files { get; }
        public abstract IRepository<LoginModel> LoginPasswords { get; }
        public abstract void Save();
        private bool _disposed = false;
        public abstract void Dispose(bool disposing);
        public abstract void Dispose();
    }
}

