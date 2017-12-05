using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CsvLoader3.Models;
using Google.Authenticator;

namespace CsvLoader3.Controllers
{
    public class _2FAController : Controller
    {
        // GET: _2FA
        public ActionResult _2FA(string qrCodeSetupImageUrl, string manualEntryKey)
        {
            _2FAModel model = new _2FAModel {QrCodeSetupImageUrl = qrCodeSetupImageUrl, ManualEntryKey = manualEntryKey };
            
            return View(model);
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
            return RedirectToAction("Upload", "Files");
        }


    }
}