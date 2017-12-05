using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CsvLoader3.Models;

namespace CsvLoader3.Controllers
{
    public class Helper
    {
        public static Dictionary<string, string> LoadDbLoginPasswordObjects(IEntityRepository<LoginModel> repository,EncrDecrHelper encrDecrHelper, byte[] key, byte[] iv)
        {
            var loginModels = repository.GetAllObjectsList();
            var loginPassword = new Dictionary<string, string>();
            foreach (var document in loginModels)
            {
                loginPassword.Add(encrDecrHelper.DecryptString(document.Email, key, iv),
                    encrDecrHelper.DecryptString(document.Password, key, iv));
            }
            return loginPassword;
        }
    }
}