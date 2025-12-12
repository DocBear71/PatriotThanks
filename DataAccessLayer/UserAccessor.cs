using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessInterfaces;
using DataDomain;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class UserAccessor : IUserAccessor
    {
        public int AuthenticateUserWithEmailAndPasswordHash(string email, string passwordHash)
        {
            int count = 0;

            // ADO.NET needs a connection
            var conn = DBConnection.GetConnection();

            // next, We need command text
            var cmdText = "sp_authenticate_user";

            // create a command object from the connection and command text
            var cmd = new SqlCommand(cmdText, conn);

            // set command type
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            // add parameters
            cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 100);

            // parameter values
            cmd.Parameters["@Email"].Value = email;
            cmd.Parameters["@PasswordHash"].Value = passwordHash;

            // we need to open a connection, execute the command
            // and capture the results in a try block

            try
            {
                // open a connection
                conn.Open();

                // execute the command and capture the results
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

            return count;
        }

        public User SelectUserByEmail(string email)
        {
            User user = null;

            // ADO.NET needs a connection
            var conn = DBConnection.GetConnection();

            // next, We need command text
            var cmdText = "sp_select_user_by_email";

            // create a command object from the connection and command text
            var cmd = new SqlCommand(cmdText, conn);

            // set command type
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            // add parameters
            cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 256);

            // parameter values
            cmd.Parameters["@Email"].Value = email;

            // we need to open a connection, execute the command
            // and capture the results in a try block

            try
            {
                // open a connection
                conn.Open();

                // execute the command and capture the results
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();

                    Debug.WriteLine($"User_ID type: {reader["UserID"].GetType()}");
                    Debug.WriteLine($"User_ID value: {reader["UserID"]}");
                    Debug.WriteLine($"Title type: {reader["TitleID"].GetType()}");
                    Debug.WriteLine($"Title value: {reader["TitleID"]}");
                    Debug.WriteLine($"First_Name type: {reader["FirstName"].GetType()}");
                    Debug.WriteLine($"First_Name value: {reader["FirstName"]}");
                    Debug.WriteLine($"Last_Name type: {reader["LastName"].GetType()}");
                    Debug.WriteLine($"Last_Name value: {reader["LastName"]}");
                    Debug.WriteLine($"Email type: {reader["Email"].GetType()}");
                    Debug.WriteLine($"Email value: {reader["Email"]}");
                    Debug.WriteLine($"Status_ID type: {reader["StatusID"].GetType()}");
                    Debug.WriteLine($"Status_ID value: {reader["StatusID"]}");
                    Debug.WriteLine($"Mem_Level_ID type: {reader["MemLevelID"].GetType()}");
                    Debug.WriteLine($"Mem_Level_ID value: {reader["MemLevelID"]}");
                    Debug.WriteLine($"Account_Locked type: {reader["AccountLocked"].GetType()}");
                    Debug.WriteLine($"Account_Locked value: {reader["AccountLocked"]}");
                    Debug.WriteLine($"Created_At type: {reader["CreatedAt"].GetType()}");
                    Debug.WriteLine($"Created_At value: {reader["CreatedAt"]}");
                    Debug.WriteLine($"Last_Updated type: {reader["LastUpdated"].GetType()}");
                    Debug.WriteLine($"Last_Updated value: {reader["LastUpdated"]}");

                    user = new User()
                    {

                        // [UserID], [TitleID], [FirstName], [LastName],
                        // [Email], [StatusID], [MemLevelID], [CreatedAt], [LastUpdated]

                        UserID = reader.GetInt32("UserID"),
                        TitleID = reader.GetString("TitleID"),
                        FirstName = reader.GetString("FirstName"),
                        LastName = reader.GetString("LastName"),
                        Email = reader.GetString("Email"),
                        StatusID = reader.GetString("StatusID"),
                        MemLevelID = reader.GetString("MemLevelID"),
                        AccountLocked = reader.GetBoolean("AccountLocked"),
                        RegistrationDate = reader.GetDateTime("CreatedAt"),
                        LastUpdated = reader.GetDateTime("LastUpdated"),
                        MustUpdatePassword = reader.GetBoolean("MustUpdatePassword"),

                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conn.Close();
            }

            return user;
        }

        public List<string> SelectAccessByEmail(string email)
        {
            List<string> access = new List<string>();

            // ADO.NET needs a connection
            var conn = DBConnection.GetConnection();

            // next, We need command text
            var cmdText = "sp_select_access_by_email";

            // create a command object from the connection and command text
            var cmd = new SqlCommand(cmdText, conn);

            // set command type
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            // add parameters
            cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 256);

            // parameter values
            cmd.Parameters["@Email"].Value = email;

            // we need to open a connection, execute the command
            // and capture the results in a try block

            try
            {
                // open a connection
                conn.Open();

                // execute the command and capture the results
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        access.Add(reader.GetString(0));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                conn.Close();
            }

            return access;
        }

        public int UpdatePasswordHash(string email, string oldPasswordHash, string newPasswordHash)
        {
            int rows = 0;

            // ADO.NET needs a connection
            var conn = DBConnection.GetConnection();

            // next, We need command text
            var cmdText = "sp_update_passwordHash_by_email";

            // create a command object from the connection and command text
            var cmd = new SqlCommand(cmdText, conn);

            // set command type
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            // add parameters
            cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 250);
            cmd.Parameters.Add("@OldPasswordHash", System.Data.SqlDbType.NVarChar, 100);
            cmd.Parameters.Add("@NewPasswordHash", System.Data.SqlDbType.NVarChar, 100);

            // Add RETURN parameter to capture the stored procedure's return value
            var returnParameter = cmd.Parameters.Add("@ReturnValue", System.Data.SqlDbType.Int);
            returnParameter.Direction = System.Data.ParameterDirection.ReturnValue;

            // parameter values
            cmd.Parameters["@Email"].Value = email;
            cmd.Parameters["@OldPasswordHash"].Value = oldPasswordHash;
            cmd.Parameters["@NewPasswordHash"].Value = newPasswordHash;

            // we need to open a connection, execute the command
            // and capture the results in a try block

            try
            {
                // open a connection
                conn.Open();

                // execute the command
                cmd.ExecuteNonQuery();

                // Get the return value from the stored procedure
                rows = (int)returnParameter.Value;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

            return rows;
        }

        public List<User> SelectUserByActiveStatus(string AccountStatusID)
        {
            List<User> user = new List<User>();

            // ADO.NET needs a connection
            var conn = DBConnection.GetConnection();

            // next, We need command text
            var cmdText = "sp_select_users_by_active";

            // create a command object from the connection and command text
            var cmd = new SqlCommand(cmdText, conn);

            // set command type
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            // add parameters
            cmd.Parameters.Add("@AccountStatusID", System.Data.SqlDbType.NVarChar, 20);

            // parameter values
            cmd.Parameters["@AccountStatusID"].Value = AccountStatusID;

            // we need to open a connection, execute the command
            // and capture the results in a try block

            try
            {
                // open a connection
                conn.Open();

                // execute the command and capture the results
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        user.Add(new User()
                        {
                            // [UserID], [TitleID], [FirstName], [LastName],
                            // [Email], [StatusID], [MemLevelID], [CreatedAt], [LastUpdated], [AccountLocked]

                            UserID = reader.GetInt32("UserID"),
                            TitleID = reader.GetString("TitleID"),
                            FirstName = reader.GetString("FirstName"),
                            LastName = reader.GetString("LastName"),
                            Email = reader.GetString("Email"),
                            StatusID = reader.GetString("StatusID"),
                            AccountStatusID = reader.GetString("AccountStatusID"),
                            MemLevelID = reader.GetString("MemLevelID"),
                            RegistrationDate = reader.GetDateTime("CreatedAt"),
                            LastUpdated = reader.GetDateTime("LastUpdated"),
                            AccountLocked = reader.GetBoolean("AccountLocked")

                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

            return user;
        }

        public int UpdateUser(User user, User oldUser)
        {
            int rows = 0;

            // ADO.NET needs a connection
            var conn = DBConnection.GetConnection();

            // next, We need command text
            var cmdText = "sp_update_user";

            // create a command object from the connection and command text
            var cmd = new SqlCommand(cmdText, conn);

            // set command type
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            // add parameters - new values
            cmd.Parameters.Add("@UserID", System.Data.SqlDbType.Int);
            cmd.Parameters.Add("@TitleID", System.Data.SqlDbType.NVarChar, 10);
            cmd.Parameters.Add("@FirstName", System.Data.SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@LastName", System.Data.SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@StatusID", System.Data.SqlDbType.NVarChar, 20);
            cmd.Parameters.Add("@AccountStatusID", System.Data.SqlDbType.NVarChar, 20);
            cmd.Parameters.Add("@MemLevelID", System.Data.SqlDbType.NVarChar, 20);

            // add parameters - old values for concurrency check
            cmd.Parameters.Add("@OldTitleID", System.Data.SqlDbType.NVarChar, 10);
            cmd.Parameters.Add("@OldFirstName", System.Data.SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@OldLastName", System.Data.SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@OldEmail", System.Data.SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@OldStatusID", System.Data.SqlDbType.NVarChar, 20);
            cmd.Parameters.Add("@OldAccountStatusID", System.Data.SqlDbType.NVarChar, 20);
            cmd.Parameters.Add("@OldMemLevelID", System.Data.SqlDbType.NVarChar, 20);

            // parameter values - new
            cmd.Parameters["@UserID"].Value = user.UserID;
            cmd.Parameters["@TitleID"].Value = user.TitleID;
            cmd.Parameters["@FirstName"].Value = user.FirstName;
            cmd.Parameters["@LastName"].Value = user.LastName;
            cmd.Parameters["@Email"].Value = user.Email;
            cmd.Parameters["@StatusID"].Value = user.StatusID;
            cmd.Parameters["@AccountStatusID"].Value = user.AccountStatusID;
            cmd.Parameters["@MemLevelID"].Value = user.MemLevelID;

            // parameter values - old
            cmd.Parameters["@OldTitleID"].Value = oldUser.TitleID;
            cmd.Parameters["@OldFirstName"].Value = oldUser.FirstName;
            cmd.Parameters["@OldLastName"].Value = oldUser.LastName;
            cmd.Parameters["@OldEmail"].Value = oldUser.Email;
            cmd.Parameters["@OldStatusID"].Value = oldUser.StatusID;
            cmd.Parameters["@OldAccountStatusID"].Value = oldUser.AccountStatusID;
            cmd.Parameters["@OldMemLevelID"].Value = oldUser.MemLevelID;

            // we need to open a connection, execute the command
            // and capture the results in a try block

            try
            {
                // open a connection
                conn.Open();

                // execute the command and capture the results
                rows = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

            return rows;
        }

        public int InsertUser(User user)
        {
            int newUserID = 0;

            // ADO.NET needs a connection
            var conn = DBConnection.GetConnection();

            // next, We need command text
            var cmdText = "sp_insert_user_with_credentials";

            // create a command object from the connection and command text
            var cmd = new SqlCommand(cmdText, conn);

            // set command type
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            // add parameters
            cmd.Parameters.Add("@TitleID", System.Data.SqlDbType.NVarChar, 10);
            cmd.Parameters.Add("@FirstName", System.Data.SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@LastName", System.Data.SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@StatusID", System.Data.SqlDbType.NVarChar, 20);
            cmd.Parameters.Add("@AccountStatusID", System.Data.SqlDbType.NVarChar, 20);
            cmd.Parameters.Add("@MemLevelID", System.Data.SqlDbType.NVarChar, 20);

            // parameter values
            cmd.Parameters["@TitleID"].Value = user.TitleID;
            cmd.Parameters["@FirstName"].Value = user.FirstName;
            cmd.Parameters["@LastName"].Value = user.LastName;
            cmd.Parameters["@Email"].Value = user.Email;
            cmd.Parameters["@StatusID"].Value = user.StatusID;
            cmd.Parameters["@AccountStatusID"].Value = user.AccountStatusID;
            cmd.Parameters["@MemLevelID"].Value = user.MemLevelID;

            // we need to open a connection, execute the command
            // and capture the results in a try block

            try
            {
                // open a connection
                conn.Open();

                // execute the command and capture the results
                // SCOPE_IDENTITY() returns the new UserID
                newUserID = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

            return newUserID;
        }

        public int UpdateAccountLocked(string email, bool locked)
        {
            int rowsAffected = 0;

            // Create connection object
            var conn = DBConnection.GetConnection();

            // Create command object
            var cmd = new SqlCommand("sp_update_account_locked", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // Add parameters
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@AccountLocked", locked);

            try
            {
                conn.Open();
                rowsAffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

            return rowsAffected;
        }
    }
}