using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.UI.WebControls;

namespace PhotosManager.Models
{
    public class DB
    {
        #region singleton setup
        private static readonly DB instance = new DB();
        public static DB Instance
        {
            get { return instance; }
        }
        #endregion
        #region Repositories
        public static UsersRepository Users { get; set; }
        public static PhotosRepository Photos { get; set; }
        public static LikesRepository Likes { get; set; }


        #endregion
        #region initialization
        public DB()
        {
            Users = new UsersRepository();
            Photos = new PhotosRepository();
            Likes = new LikesRepository();
            InitRepositories(this);
        }
        private static void InitRepositories(DB db)
        {
            string serverPath = HostingEnvironment.MapPath(@"~/App_Data/");
            PropertyInfo[] myPropertyInfo = db.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in myPropertyInfo)
            {
                if (propertyInfo.PropertyType.Name.Contains("Repository"))
                {
                    MethodInfo method = propertyInfo.PropertyType.GetMethod("Init");
                    method.Invoke(propertyInfo.GetValue(db), new object[] { serverPath + propertyInfo.Name + ".json" });
                }
            }
        }
        #endregion
    }
}