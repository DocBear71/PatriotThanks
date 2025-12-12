using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataDomain;

namespace DataAccessInterfaces
{
    public interface IUserAccessor
    {
        int AuthenticateUserWithEmailAndPasswordHash(string email, string passwordHash);
        User SelectUserByEmail(string email);
        List<string> SelectAccessByEmail(string email);
        int UpdatePasswordHash(string email, string oldPasswordHash, string newPasswordHash);

        List<User> SelectUserByActiveStatus(string accountStatusID);
        int UpdateUser(User user, User oldUser);
        int InsertUser(User user);
        int UpdateAccountLocked(string email, bool locked);
    }
}
