using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PhotoId { get; set; }
        public DateTime CreationDate { get; set; }

        public Like()
        {
            Id = 0;
            CreationDate = DateTime.Now;
        }
    }
}