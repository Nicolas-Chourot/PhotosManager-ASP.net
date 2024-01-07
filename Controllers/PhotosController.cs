using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;

namespace PhotosManager.Controllers
{
    public class PhotosController : Controller
    {
        public ActionResult SetPhotoOwnerSearchId(int id)
        {
            Session["photoOwnerSearchId"] = id;
            return RedirectToAction("List");
        }
        public ActionResult SetSearchKeywords(string keywords)
        {
            Session["searchKeywords"] = keywords;
            return RedirectToAction("List");
        }
        public ActionResult List(string sortType)
        {
            if (Session["photoOwnerSearchId"] == null) Session["photoOwnerSearchId"] = 0;
            if (Session["searchKeywords"] == null) Session["searchKeywords"] = "";
            if (Session["PhotosSortType"] == null) Session["PhotosSortType"] = "date";
            if (!string.IsNullOrEmpty(sortType)) Session["PhotosSortType"] = sortType;
            List<Photo> list;
            switch ((string)Session["PhotosSortType"])
            {
                case "likes":
                    list = DB.Photos.ToList().OrderByDescending(p => p.Likes).ToList();
                    break;
                case "owner":
                    list = DB.Photos.ToList().Where(p => p.OwnerId == ((User)Session["ConnectedUser"]).Id).OrderByDescending(p => p.CreationDate).ToList();
                    break;
                case "user":
                    if ((int)Session["photoOwnerSearchId"] != 0)
                        list = DB.Photos.ToList().Where(p => p.OwnerId == (int)Session["photoOwnerSearchId"]).OrderByDescending(p => p.CreationDate).ToList();
                    else
                        list = DB.Photos.ToList().OrderBy(p => p.Owner.Name).ThenByDescending(p => p.CreationDate).ToList();
                    break;
                case "keywords":
                    if (!string.IsNullOrEmpty((string)Session["searchKeywords"]))
                    {
                        list = new List<Photo>();
                        string[] keywords = ((string)Session["searchKeywords"]).Split(' ');
                        foreach (var photo in DB.Photos.ToList())
                        {
                            bool keep = true;
                            foreach (string keyword in keywords)
                            {
                                string kw = keyword.Trim().ToLower();
                                if (!photo.Title.ToLower().Contains(kw) && !photo.Description.ToLower().Contains(kw))
                                {
                                    keep = false;
                                    break;
                                }
                            }
                            if (keep)
                                list.Add(photo);
                        }
                        list = list.OrderByDescending(p => p.CreationDate).ToList();
                    }
                    else
                        list = DB.Photos.ToList().OrderByDescending(p => p.CreationDate).ToList();
                    break;
                default:
                    list = DB.Photos.ToList().OrderByDescending(p => p.CreationDate).ToList();
                    break;
            }
            return View(list);
        }
        public ActionResult Create()
        {
            return View(new Photo());
        }
        [HttpPost]
        public ActionResult Create(Photo photo)
        {
            DB.Photos.Add(photo);
            return RedirectToAction("List");
        }
        public ActionResult Update()
        {
            return View(new Photo());
        }
        [HttpPost]
        public ActionResult Update(Photo photo)
        {
            DB.Photos.Add(photo);
            return RedirectToAction("List");
        }
        public ActionResult Delete(int id)
        {
            DB.Photos.Delete(id);
            return RedirectToAction("List");
        }
    }
}