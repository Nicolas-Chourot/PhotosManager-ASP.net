using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotosManager.Controllers
{
    [AllowCrossSite]
    public class AccountsController : Controller
    {
        public JsonResult EmailExist(string Email)
        {
            bool exist = DB.Users.EmailExist(Email);
            return Json(exist, JsonRequestBehavior.AllowGet);
        }
        public JsonResult PasswordValid(string Password)
        {
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExpiredSession()
        {
            Session["LoginMessage"] = "Session expirée, veuillez vous reconnecter.";
            return RedirectToAction("Login", "Accounts");
        }
        public ActionResult Logout()
        {
            return RedirectToAction("Login", "Accounts");
        }
        public ActionResult Login()
        {
            Session["currentLoginEmail"] = "";
            Session["connectedUser"] = null;
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginCredential credential)
        {
            credential.Email = credential.Email.Trim();
            credential.Password = credential.Password.Trim();
            Session["currentLoginEmail"] = credential.Email;
            Session["LoginMessage"] = "";
            Session["connectedUser"] = DB.Users.GetUser(credential);
            if (Session["connectedUser"] == null)
            {
                Session["LoginMessage"] = "Erreur de connexion"; 
                return View();
            }
            return RedirectToAction("List", "Photos");
        }
        public ActionResult Subscribe()
        {
            Session["connectedUser"] = null;
            Session["currentLoginEmail"] = "";
            return View(new User());
        }
        [HttpPost]
        public ActionResult Subscribe(User user)
        {
            DB.Users.Add(user);
            Session["LoginMessage"] = "Création de compte effectué avec succès";
            return RedirectToAction("Login");
        }
        public ActionResult EditProfil()
        {
            User connectedUser = (User)Session["connectedUser"];
            if (connectedUser != null)
            {
                return View(connectedUser);
            }
            return RedirectToAction("Login", "Accounts");
        }
        [HttpPost]
        public ActionResult EditProfil(User user)
        {
            if (DB.Users.Update(user))
            {
                Session["connectedUser"] = DB.Users.Get(user.Id);
            }
            return RedirectToAction("List", "Photos");
        }
    }
}