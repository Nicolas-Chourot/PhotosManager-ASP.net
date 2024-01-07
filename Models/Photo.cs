using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class Photo
    {
        const string PhotosFolder = @"/Images_Data/Photos/";
        const string DefaultPhoto = @"No_Image.png";

        public int Id { get; set; }
        public int OwnerId { get; set; }            // Id du propriétaire de la photo
        public string Title { get; set; }           // Titre de la photo
        public string Description { get; set; }     // Description de la photo
        public DateTime CreationDate { get; set; }  // Date de création
        public bool Shared { get; set; }            // Indicateur de partage ("true" ou "false")
        public int Likes { get; set; }              // compte des likes
        [Asset(PhotosFolder)]
        public string Image { get; set; }           // Url relatif de l'image

        public Photo()
        {
            Id = 0;
            CreationDate = DateTime.Now;
            Shared = false;
            Image = PhotosFolder + DefaultPhoto;
        }

        public User Owner
        {
            get
            {
                return DB.Users.Get(OwnerId);
            }
        }
    }
}