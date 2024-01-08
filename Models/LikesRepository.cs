using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PhotosManager.Models
{
    public class LikesRepository : Repository<Like>
    {

        public void ToogleLike(int photoId, int userId)
        {
            Like like = ToList().Where(l => (l.PhotoId == photoId && l.UserId == userId)).FirstOrDefault();
            Photo photo = DB.Photos.Get(photoId);
            if (like != null)
            {
                BeginTransaction();
                photo.Likes--;
                DB.Photos.Update(photo);
                Delete(like.Id);
                EndTransaction();
            }
            else
            {
                BeginTransaction();
                photo.Likes++;
                DB.Photos.Update(photo);
                like = new Like { PhotoId = photoId, UserId = userId };
                Add(like);
                EndTransaction();
            }
        }
        public void DeleteByPhotoId(int photoId)
        {
            List<Like> list = ToList().Where(l=>l.PhotoId == photoId).ToList();
            list.ForEach(l => { Delete(l.Id); });
        }
        public void DeleteByUserId(int userId)
        {
            List<Like> list = ToList().Where(l => l.UserId == userId).ToList();
            list.ForEach(l => {
                Photo photo = DB.Photos.Get(l.PhotoId);
                photo.Likes--;
                DB.Photos.Update(photo);
                Delete(l.Id); 
            });
        }
    }
}