using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotosManager.Controllers
{
    public class AccountsController : Controller
    {
        public JsonResult EmailExist(string Email)
        {
            bool exist = DB.Users.EmailExist(Email);
            return Json(exist, JsonRequestBehavior.AllowGet);
        }
        [UserAccess]
        public JsonResult EmailConflict(string Email)
        {
            User connectedUser = (User)Session["ConnectedUser"];
            User foundUser = DB.Users.ToList().Where(u => u.Email == Email).FirstOrDefault();
            bool conflict = false;
            if (foundUser != null) conflict = foundUser.Id != connectedUser.Id;
            return Json(conflict, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExpiredSession()
        {
            return RedirectToAction("Login?message=Session expirée, veuillez vous reconnecter.");
        }
        public ActionResult Logout()
        {
            return RedirectToAction("Login", "Accounts");
        }
        public ActionResult Login(string message = "")
        {
            Session["LoginMessage"] = message;
            if (Session["currentLoginEmail"] == null) Session["currentLoginEmail"] = "";
            Session["connectedUser"] = null;
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginCredential credential)
        {
            credential.Email = credential.Email.Trim();
            credential.Password = credential.Password.Trim();
            Session["currentLoginEmail"] = credential.Email;
            Session["connectedUser"] = DB.Users.GetUser(credential);
            if (Session["connectedUser"] == null)
            {
                Session["LoginMessage"] = "Erreur de connexion!";
                return View();
            }
            else
            {
                User user = (User)Session["connectedUser"];
                if (user.Blocked)
                {
                    return Redirect("/Accounts/Login?message=Votre compte a été bloqué!");
                }
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
            return RedirectToAction("Login?message=Création de compte effectué avec succès!");
        }
        [UserAccess]
        public ActionResult EditProfil()
        {
            User connectedUser = (User)Session["connectedUser"];
            if (connectedUser != null)
            {
                return View(connectedUser);
            }
            return RedirectToAction("Login", "Accounts");
        }
        [UserAccess]
        [HttpPost]
        public ActionResult EditProfil(User user)
        {
            if (DB.Users.Update(user))
            {
                Session["connectedUser"] = DB.Users.Get(user.Id);
            }
            return RedirectToAction("List", "Photos");
        }
        [UserAccess]
        public ActionResult Delete()
        {
            User connectedUser = (User)Session["ConnectedUser"];
            DB.Users.Delete(connectedUser.Id);
            return RedirectToAction("Login?message=Votre compte a été effacé avec succès!");
        }
        [AdminAccess]
        public ActionResult ManageUsers()
        {
            return View(DB.Users.ToList().OrderBy(u => u.Name).ToList());
        }
        [AdminAccess]
        public ActionResult TooglePromoteUser(int id)
        {
            User user = DB.Users.Get(id);
            if (user != null)
            {
                user.AccessType = user.AccessType == 1 ? 0 : 1;
                DB.Users.Update(user); 
            }
            return RedirectToAction("ManageUsers");
        }
        [AdminAccess]
        public ActionResult ToogleBlockUser(int id)
        {
            User user = DB.Users.Get(id);
            if (user != null)
            {
                user.Blocked = !user.Blocked;
                DB.Users.Update(user);
            }
            return RedirectToAction("ManageUsers");
        }
        [AdminAccess]
        public ActionResult DeleteUser(int id)
        {
            DB.Users.Delete(id);
            return RedirectToAction("ManageUsers");
        }
    }
}