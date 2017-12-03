using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CsvLoader3.Models;
using Google.Authenticator;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CsvLoader3.Controllers
{
    public class LoginController : Controller
    {

        private readonly MongoDbHelper _mongoDbHelper = new MongoDbHelper();
        private readonly EncrDecrHelper _encrDecrHelper = new EncrDecrHelper();
        private const string LoginPassword = "LoginPassword";
        private const string PasswordKey = "Alexandra_2219256"; // any 10-12 char string for use as private key in google authenticator
        
        // Create secret IV
        public byte[] Iv { get; } = new byte[16]
            {0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0};


        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(LoginModel login)
        {
            var message = "";
            var status = false;
            SHA256 mySha256 = SHA256Managed.Create();
            byte[] key = mySha256.ComputeHash(Encoding.ASCII.GetBytes(PasswordKey));

            var database = _mongoDbHelper.CreateConnection();
            var loginPassword = LoadDbLoginPasswordObjects(database, _encrDecrHelper, key, Iv);
            //check username and password form our database here
            //for demo I am going to use Admin as Username and Password1 as Password static value
            if (loginPassword.ContainsKey(login.Email) && loginPassword[login.Email]==login.Password/*login.Email == "Admin@a.com" && login.Password == "Password1"!*/)
            {
                //Dictionary<string, object> dict = new Dictionary<string, object>();
                //dict.Add("Email", _encrDecrHelper.EncryptString(login.Email, key, Iv));
                //dict.Add("Password", _encrDecrHelper.EncryptString(login.Password,key,Iv));
                //await _mongoDbHelper.SaveObjectToCollection(database, dict, LoginPassword);

                status = true; // show 2FA form
                message = "2FA Verification";
                Session["Email"] = login.Email;

                //2FA Setup
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                var userUniqueKey = (login.Email + PasswordKey); //as Its a demo, I have done this way. But you should use any encrypted value here which will be unique value per user.
                Session["UserUniqueKey"] = userUniqueKey;
                var setupInfo = tfa.GenerateSetupCode("Dotnet Awesome", login.Email, userUniqueKey, 300, 300);
                ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                ViewBag.SetupCode = setupInfo.ManualEntryKey;
            }
            else
            {
                message = "Invalid credential";
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View();
        }

        private static Dictionary<string, string> LoadDbLoginPasswordObjects(IMongoDatabase database, EncrDecrHelper encrDecrHelper, byte[] key, byte[] iv)
        {
            var collection = database.GetCollection<BsonDocument>(LoginPassword);
            var cursor = collection.Find(new BsonDocument()).ToCursor();
            var loginPassword = new Dictionary<string, string>();
            foreach (var document in cursor.ToEnumerable())
            {
                var documentElements = document.Elements.ToDictionary(x => x.Name, y => y.Value);
                loginPassword.Add(encrDecrHelper.DecryptString(documentElements["Email"].AsString,key,iv), 
                    encrDecrHelper.DecryptString(documentElements["Password"].AsString, key, iv));
                
            }
            return loginPassword;
        }



        public ActionResult Verify2FA()
        {
            var token = Request["passcode"];
            var tfa = new TwoFactorAuthenticator();
            var userUniqueKey = Session["UserUniqueKey"].ToString();
            var isValid = tfa.ValidateTwoFactorPIN(userUniqueKey, token);
            if (!isValid)
                return RedirectToAction("Index", "Login");
            Session["IsValid2FA"] = true;
            return RedirectToAction("Upload", "Loader");
        }


    }
}