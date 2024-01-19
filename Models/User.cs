using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class User
    {
        const string AvatarsFolder = @"/Images_Data/Users_Avatars/";
        const string DefaultAvatar = @"no_avatar.png";

        public User()
        {
            Id = 0;
            Blocked = false;
            Avatar = AvatarsFolder + DefaultAvatar;
            Admin = false;
        }
        public User Clone()
        {
            return JsonConvert.DeserializeObject<User>(JsonConvert.SerializeObject(this));
        }
        #region Data Members
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Admin { get; set; }
        public bool Blocked { get; set; }

        [Asset(AvatarsFolder)]
        public string Avatar { get; set; }
        #endregion

        #region View members
        [JsonIgnore]
        public bool IsAdmin { get { return Admin; } }
        [JsonIgnore]
        public bool IsBlocked { get { return Blocked; } }
        #endregion
    }
}