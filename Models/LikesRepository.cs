using System.Collections.Generic;
using System.Linq;

namespace PhotosManager.Models
{
    public class LikesRepository : Repository<Like>
    {
        public void DeleteByPhotoId(int photoId)
        {
            List<Like> list = ToList().Where(l=>l.PhotoId == photoId).ToList();
            list.ForEach(l => { base.Delete(l.Id); });
        }
    }
}