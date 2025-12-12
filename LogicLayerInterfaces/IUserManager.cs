using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataDomain;

namespace LogicLayerInterfaces
{
    public interface IUserManager
    {
        public UserVM loginUser(string username, string password);
        public bool AuthenticateUser(string username, string password);
        public List<string> GetAccessForUser(string email);
        public User GetUserByEmail(string email);
        string HashSha256(string password);
        bool ResetPassword(string email, string oldPassword, string newPassword);
        public List<User> GetUsersByActive(string accountStatusID);
        public bool EditUser(User user, User oldUser);
        public int AddUser(User user);
        public bool SetAccountLocked(string email, bool locked);
    }
}
