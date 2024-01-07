using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class UsersRepository : Repository<User>
    {
        public bool EmailExist(string email)
        {
            return ToList().Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault() != null;
        }
        public User GetUser(LoginCredential loginCredential)
        {
            User user = ToList().Where(u => (u.Email.ToLower() == loginCredential.Email.ToLower()) &&
                                            (u.Password == loginCredential.Password))
                                .FirstOrDefault();
            return user;
        }
        public override bool Update(User user)
        {
            if (string.IsNullOrEmpty(user.Password))
            {
                User storedUser = Get(user.Id); 
                if (storedUser != null)
                {
                    user.Password = storedUser.Password;
                }
            }
            return base.Update(user);
        }
        public override bool Delete(int userId)
        {
            try
            {
                User userToDelete = DB.Users.Get(userId);
                if (userToDelete != null)
                {
                    BeginTransaction();
                    DB.Photos.DeleteByOwnerId(userId);
                    base.Delete(userId);
                    EndTransaction();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Remove user failed : Message - {ex.Message}");
                EndTransaction();
                return false;
            }
        }

    }
}