using CsvLoader3.Models;
using Google.Authenticator;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace CsvLoader3.Controllers
{
    public class LoginController : Controller
    {
        private readonly IEntityRepository<LoginModel> _loginRepository;
        private readonly EncrDecrHelper _encrDecrHelper = new EncrDecrHelper();

        public LoginController(IUnitOfWork unitOfWork)
        {
            _loginRepository = unitOfWork.GetRepository<LoginModel>();
        }

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginModel login)
        {
            //var model = new LoginViewModel(login.Model, _loginRepository, Session, ViewBag);
            //model.Login();
            //return View(model);

            var message = "";
            var status = false;
            SHA256 mySha256 = SHA256Managed.Create();
            byte[] key = mySha256.ComputeHash(Encoding.ASCII.GetBytes(Utils.PasswordKey));

            var loginPassword = Utils.LoadDbLoginPasswordObjects(_loginRepository, _encrDecrHelper, key, Utils.Iv);
            if (loginPassword.ContainsKey(login.Email) && loginPassword[login.Email] == login.Password)
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
                var userUniqueKey = (login.Email + Utils.PasswordKey);
                Session["UserUniqueKey"] = userUniqueKey;
                var setupInfo = tfa.GenerateSetupCode("Dotnet Awesome", login.Email, userUniqueKey, 300, 300);
                ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                ViewBag.SetupCode = setupInfo.ManualEntryKey;
                return RedirectToAction("_2FA", "_2FA", new { @qrCodeSetupImageUrl = ViewBag.BarcodeImageUrl, @manualEntryKey = ViewBag.SetupCode });
            }
            else
            {
                message = "Invalid credential";
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return RedirectToAction("Index", "Login");
        }
    }
}