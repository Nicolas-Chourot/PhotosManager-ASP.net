using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotosManager.Controllers
{
    public class UserAccess : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            User connectedUser = (User)HttpContext.Current.Session["connectedUser"];
            if (connectedUser == null)
            {
                httpContext.Response.Redirect("/Accounts/Login?message=Accès non autorisé!");
                return false;
            }
            return true;
        }
    }
    public class AdminAccess : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            HttpContext.Current.Session["CRUD_Access"] = false;
            User connectedUser = (User)HttpContext.Current.Session["connectedUser"];
            if (connectedUser == null)
            {
                httpContext.Response.Redirect("/Accounts/Login?message=Accès non autorisé!");
                return false;
            }
            else
            {
                if (!connectedUser.IsAdmin)
                {
                    httpContext.Response.Redirect("/Accounts/Login?message=Accès administrateur non autorisé!");
                    return false;
                }
                return true;
            }
        }
    }
}