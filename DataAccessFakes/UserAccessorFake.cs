using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessInterfaces;
using DataDomain;

namespace DataAccessFakes
{
    public class UserAccessorFake : IUserAccessor
    {
        private List<DataDomain.User> _users = new List<DataDomain.User>();
        private List<UserVM> _userVMs = new List<UserVM>();
        private Dictionary<string, string> _credentials = new Dictionary<string, string>(); // email -> passwordHash
        private Dictionary<string, List<string>> _userAccess = new Dictionary<string, List<string>>(); // email -> access list
        private string _passwordHash;
        private int _nextUserID = 100;  // For generating new UserIDs


        public UserAccessorFake()
        {
            // Initialize with default test users
            _users.Add(new DataDomain.User()
            {
                UserID = 1,
                TitleID = "Mr.",
                FirstName = "Test",
                LastName = "User1",
                Address1 = "123 Test St.",
                Address2 = "Apt. 1",
                City = "Testville",
                State = "TX",
                Zip = "12345",
                StatusID = "Veteran",
                Email = "testUser1@test.com",
                Is_Active = true,
                AccountStatusID = "Active",
                MemLevelID = "Member",
                AccountLocked = false,
                RegistrationDate = DateTime.Now.AddMonths(-6),
                LastUpdated = DateTime.Now
            });
            _users.Add(new DataDomain.User()
            {
                UserID = 2,
                TitleID = "Mx.",
                FirstName = "Test",
                LastName = "User2",
                Address1 = "123 Test St.",
                Address2 = "Apt. 2",
                City = "Testville",
                State = "TX",
                Zip = "12345",
                StatusID = "Active Duty",
                Email = "testUser2@test.com",
                Is_Active = true,
                AccountStatusID = "Active",
                MemLevelID = "Member",
                AccountLocked = false,
                RegistrationDate = DateTime.Now.AddMonths(-3),
                LastUpdated = DateTime.Now
            });
            _users.Add(new DataDomain.User()
            {
                UserID = 3,
                TitleID = "Miss.",
                FirstName = "Test",
                LastName = "User3",
                Address1 = "123 Test St.",
                Address2 = "Apt. 3",
                City = "Testville",
                State = "TX",
                Zip = "12345",
                StatusID = "First Responder",
                Email = "testUser3@test.com",
                Is_Active = true,
                AccountStatusID = "Inactive",
                MemLevelID = "Guest",
                AccountLocked = false,
                RegistrationDate = DateTime.Now.AddYears(-1),
                LastUpdated = DateTime.Now
            });
            _users.Add(new DataDomain.User()
            {
                UserID = 4,
                TitleID = "Mrs.",
                FirstName = "Test",
                LastName = "User4",
                Address1 = "123 Test St.",
                Address2 = "Apt. 4",
                City = "Testville",
                State = "TX",
                Zip = "12345",
                StatusID = "Spouse",
                Email = "testUser4@test.com",
                Is_Active = true,
                AccountStatusID = "PendingVerify",
                MemLevelID = "Guest",
                AccountLocked = false,
                RegistrationDate = DateTime.Now.AddDays(-7),
                LastUpdated = DateTime.Now
            });
            _users.Add(new DataDomain.User()
            {
                UserID = 5,
                TitleID = "Dr.",
                FirstName = "Test",
                LastName = "User5",
                Address1 = "123 Test St.",
                Address2 = "Apt. 5",
                City = "Testville",
                State = "TX",
                Zip = "12345",
                StatusID = "Supporter",
                Email = "testUser5@test.com",
                Is_Active = false,
                AccountStatusID = "Suspended",
                MemLevelID = "Member",
                AccountLocked = true,
                RegistrationDate = DateTime.Now.AddYears(-2),
                LastUpdated = DateTime.Now
            });

            // Initialize UserVMs with access levels
            _userVMs.Add(new UserVM()
            {
                UserID = _users[0].UserID,
                Email = _users[0].Email,
                Access = new List<string> { "Guest", "Member" },
            });
            _userVMs.Add(new UserVM()
            {
                UserID = _users[1].UserID,
                Email = _users[1].Email,
                Access = new List<string> { "Guest", "Member" },
            });
            _userVMs.Add(new UserVM()
            {
                UserID = _users[2].UserID,
                Email = _users[2].Email,
                Access = new List<string>(),
            });

            // Default password hash for "newuser"
            _passwordHash = "9c9064c59f1ffa2e174ee754d2979be80dd30db552ec03e7e327e9b1a4bd594e";

            // Add default credentials for existing users
            _credentials["testUser1@test.com"] = _passwordHash;
            _credentials["testUser2@test.com"] = _passwordHash;
            _credentials["testUser3@test.com"] = _passwordHash;

            // Add default access mappings
            _userAccess["testUser1@test.com"] = new List<string> { "Guest", "Member" };
            _userAccess["testUser2@test.com"] = new List<string> { "Guest", "Member" };
            _userAccess["testUser3@test.com"] = new List<string>();
        }

        // Public method to add fake users for testing
        public void AddFakeUser(DataDomain.User user)
        {
            // Check if user already exists and update, otherwise add
            var existingUser = _users.FirstOrDefault(u => u.Email == user.Email);
            if (existingUser != null)
            {
                _users.Remove(existingUser);
            }
            _users.Add(user);
        }

        // Public method to add fake credentials for testing
        public void AddFakeCredential(string email, string passwordHash)
        {
            _credentials[email] = passwordHash;
        }

        // Public method to add fake access levels for testing
        public void AddFakeAccess(string email, params string[] accessLevels)
        {
            if (!_userAccess.ContainsKey(email))
            {
                _userAccess[email] = new List<string>();
            }
            _userAccess[email].AddRange(accessLevels);
        }

        // Public method to clear all fake data (useful for test cleanup)
        public void ClearFakeData()
        {
            _users.Clear();
            _userVMs.Clear();
            _credentials.Clear();
            _userAccess.Clear();
        }

        public int AuthenticateUserWithEmailAndPasswordHash(string email, string passwordHash)
        {
            int result = 0;

            // Check credentials dictionary first
            if (_credentials.ContainsKey(email) && _credentials[email] == passwordHash)
            {
                // Find the user and check if active
                var user = _users.FirstOrDefault(u => u.Email == email);
                if (user != null && user.Is_Active)
                {
                    result = 1;
                }
            }

            return result;
        }

        public DataDomain.User SelectUserByEmail(string email)
        {
            DataDomain.User user = null;

            foreach (DataDomain.User ind in _users)
            {
                if (ind.Email == email)
                {
                    user = ind;
                    break;
                }
            }
            if (user == null)
            {
                throw new ArgumentException("Email not found");
            }
            return user;
        }

        public List<string> SelectAccessByEmail(string email)
        {
            List<string> access = null;

            // Check the userAccess dictionary first
            if (_userAccess.ContainsKey(email))
            {
                access = _userAccess[email];
            }
            else
            {
                // Fall back to checking userVMs
                foreach (UserVM ind in _userVMs)
                {
                    if (ind.Email == email)
                    {
                        access = ind.Access;
                        break;
                    }
                }
            }

            if (access == null)
            {
                throw new ArgumentException("Email not found");
            }
            return access;
        }

        public int UpdatePasswordHash(string email, string oldPasswordHash, string newPasswordHash)
        {
            int rows = 0;
            try
            {
                rows = AuthenticateUserWithEmailAndPasswordHash(email, oldPasswordHash);
                if (rows == 1)
                {
                    _credentials[email] = newPasswordHash;
                    _passwordHash = newPasswordHash; // Keep legacy behavior
                }
                else
                {
                    throw new ArgumentException("invalid email or password");
                }
            }
            catch (Exception)
            {
                throw;
            }

            return rows;
        }

        public List<User> SelectUserByActiveStatus(string accountStatusID)
        {
            List<User> users = new List<User>();

            foreach (var user in _users)
            {
                if (user.AccountStatusID == accountStatusID)
                {
                    users.Add(user);
                }
            }

            return users;
        }

        public int UpdateUser(User user, User oldUser)
        {
            int rows = 0;

            foreach (User existingUser in _users)
            {
                // Check if this is the user we're updating by matching old values
                if (existingUser.UserID == user.UserID &&
                    existingUser.TitleID == oldUser.TitleID &&
                    existingUser.FirstName == oldUser.FirstName &&
                    existingUser.LastName == oldUser.LastName &&
                    existingUser.Email == oldUser.Email &&
                    existingUser.StatusID == oldUser.StatusID &&
                    existingUser.AccountStatusID == oldUser.AccountStatusID &&
                    existingUser.MemLevelID == oldUser.MemLevelID)
                {
                    // Update the user with new values
                    existingUser.TitleID = user.TitleID;
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.StatusID = user.StatusID;
                    existingUser.AccountStatusID = user.AccountStatusID;
                    existingUser.MemLevelID = user.MemLevelID;
                    existingUser.LastUpdated = DateTime.Now;

                    // Update credentials dictionary if email changed
                    if (oldUser.Email != user.Email && _credentials.ContainsKey(oldUser.Email))
                    {
                        string passwordHash = _credentials[oldUser.Email];
                        _credentials.Remove(oldUser.Email);
                        _credentials[user.Email] = passwordHash;
                    }

                    // Update access dictionary if email changed
                    if (oldUser.Email != user.Email && _userAccess.ContainsKey(oldUser.Email))
                    {
                        List<string> access = _userAccess[oldUser.Email];
                        _userAccess.Remove(oldUser.Email);
                        _userAccess[user.Email] = access;
                    }

                    rows = 1;
                    break;
                }
            }
            return rows;
        }

        public int InsertUser(User user)
        {
            // Generate a new UserID
            int newUserID = _nextUserID++;

            // Create a new user with the generated ID
            User newUser = new User()
            {
                UserID = newUserID,
                TitleID = user.TitleID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                StatusID = user.StatusID,
                AccountStatusID = user.AccountStatusID,
                MemLevelID = user.MemLevelID,
                Is_Active = true,
                AccountLocked = false,
                RegistrationDate = DateTime.Now,
                LastUpdated = DateTime.Now
            };

            // Check for duplicate email
            if (_users.Any(u => u.Email == user.Email))
            {
                throw new Exception("A user with this email already exists.");
            }

            _users.Add(newUser);

            // Add default credentials for the new user (using default password hash)
            _credentials[user.Email] = _passwordHash;

            // Add default access for the new user based on membership level
            _userAccess[user.Email] = new List<string> { user.MemLevelID };

            return newUserID;
        }

        /// <summary>
        /// Updates the AccountLocked status for a user in the fake data.
        /// </summary>
        /// <param name="email">The email of the user to update</param>
        /// <param name="locked">True to lock the account, false to unlock</param>
        /// <returns>Number of rows affected (1 if successful, 0 if user not found)</returns>
        public int UpdateAccountLocked(string email, bool locked)
        {
            var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                return 0;
            }

            user.AccountLocked = locked;
            user.LastUpdated = DateTime.Now;
            return 1;
        }
    }
}