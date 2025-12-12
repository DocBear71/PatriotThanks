using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicLayerInterfaces;
using DataDomain;
using System.Security.Cryptography;
using DataAccessInterfaces;
using DataAccessLayer;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Metadata.Ecma335;

namespace LogicLayer
{
    public class UserManager : IUserManager
    {
        private IUserAccessor _userAccessor;

        // dependency inversion/injection follows
        public UserManager()
        {
            _userAccessor = new UserAccessor();
        }

        public UserManager(IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
        }

        public bool AuthenticateUser(string username, string password)
        {
            bool isAuthenticated = false;

            try
            {
                password = HashSha256(password);
                isAuthenticated = (1 == _userAccessor.AuthenticateUserWithEmailAndPasswordHash(username, password));
            }
            catch (Exception ex)
            {

                throw new ApplicationException("Authentication Failed", ex);
            }

            return isAuthenticated;
        }

        public User GetUserByEmail(string email)
        {
            User user = null;

            try
            {
                user = _userAccessor.SelectUserByEmail(email);
            }
            catch (Exception ex)
            {

                throw new ApplicationException("user not found", ex);
            }


            return user;
        }

        public List<string> GetAccessForUser(string email)
        {
            List<string> access = null;

            try
            {
                access = _userAccessor.SelectAccessByEmail(email);
            }
            catch (Exception ex)
            {

                throw new ApplicationException("user not found", ex);
            }

            return access;
        }

        public string HashSha256(string password)
        {
            string result = null;

            if (password == "")
            {
                throw new ArgumentOutOfRangeException("password");
            }

            // create a byte array
            byte[] data;

            // create a .NET hash provider object
            using (SHA256 sha256hasher = SHA256.Create())
            {
                // hash the input
                data = sha256hasher.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

            // create output
            var s = new StringBuilder();

            // loop through the data
            for (int i = 0; i < data.Length; i++)
            {
                s.Append(data[i].ToString("x2").ToLower());
            }

            // convert s to a real string
            result = s.ToString();

            return result;
        }

        public UserVM loginUser(string username, string password)
        {
            UserVM userVM = null;
            try
            {
                // Get user first to check if account is locked
                User ind = GetUserByEmail(username);

                if (ind == null)
                {
                    throw new ApplicationException("Invalid email or password.");
                }

                // Check if account is locked BEFORE authentication
                if (ind.AccountLocked)
                {
                    throw new ApplicationException("Your account is locked. Please contact support.");
                }

                // Now authenticate

                if (AuthenticateUser(username, password))
                {
                    userVM = new UserVM()
                    {
                        UserID = ind.UserID,
                        TitleID = ind.TitleID,
                        FirstName = ind.FirstName,
                        LastName = ind.LastName,
                        StatusID = ind.StatusID,
                        AccountStatusID = ind.AccountStatusID,
                        MemLevelID = ind.MemLevelID,
                        Email = ind.Email,
                        Is_Active = ind.Is_Active,
                        AccountLocked = ind.AccountLocked, // Include this in VM
                        RegistrationDate = ind.RegistrationDate,
                        MustUpdatePassword = ind.MustUpdatePassword,
                        Access = GetAccessForUser(username),
                    };
                }
                else
                {
                    // Authentication failed - wrong password
                    throw new ApplicationException("Invalid email or password.");
                }
            }
            catch (ApplicationException)
            {
                throw;
            }

            catch (Exception ex)
            {
                throw new ApplicationException("Login failed. Please try again.", ex);
            }

            return userVM;
        }

        public bool ResetPassword(string email, string oldPassword, string newPassword)
        {
            bool isUpdated = false;

            try
            {
                oldPassword = HashSha256(oldPassword);
                newPassword = HashSha256(newPassword);
                isUpdated = (1 == _userAccessor.UpdatePasswordHash(email, oldPassword, newPassword));
                if (!isUpdated)
                {
                    throw new ArgumentException("Bad Username or Password");
                }
            }
            catch (Exception ex)
            {

                throw new ApplicationException("Update Password Failed", ex); ;
            }

            return isUpdated;
        }

        public List<User> GetUsersByActive(string accountStatusID)
        {
            List<User> users = null;
            try
            {
                users = _userAccessor.SelectUserByActiveStatus(accountStatusID);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Users not found", ex);
            }
            return users;
        }

        public bool EditUser(User user, User oldUser)
        {
            bool result = false;

            try
            {
                result = (1 == _userAccessor.UpdateUser(user, oldUser));
                if (result == false)
                {
                    throw new ApplicationException("Update Failed. Data may have been modified by another user.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Update Failed.", ex);
            }
            return result;
        }

        public int AddUser(User user)
        {
            int newUserID = 0;

            try
            {
                newUserID = _userAccessor.InsertUser(user);
                if (newUserID == 0)
                {
                    throw new ApplicationException("Add User Failed. User was not created.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Add User Failed.", ex);
            }
            return newUserID;
        }

        public bool SetAccountLocked(string email, bool locked)
        {
            bool result = false;

            try
            {
                result = (1 == _userAccessor.UpdateAccountLocked(email, locked));
                if (!result)
                {
                    throw new ApplicationException("Account lock status update failed. User may not exist.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to {(locked ? "lock" : "unlock")} account.", ex);
            }

            return result;
        }

    }
}