using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessInterfaces;
using DataDomain;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class LookupAccessor : ILookupAccessor
    {
        public List<BusinessType> SelectAllBusinessTypes()
        {
            List<BusinessType> businessTypes = new List<BusinessType>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_select_all_business_types";
            var cmd = new SqlCommand(cmdText, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        businessTypes.Add(new BusinessType()
                        {
                            BusTypeID = reader.GetString("BusTypeID"),
                            BusTypeDescription = reader.GetString("BusTypeDescription"),
                            DisplayOrder = reader.GetInt32("DisplayOrder"),
                            IsActive = reader.GetBoolean("IsActive")
                        });
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

            return businessTypes;
        }

        public List<BusinessType> SelectActiveBusinessTypes()
        {
            List<BusinessType> businessTypes = new List<BusinessType>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_select_active_business_types";
            var cmd = new SqlCommand(cmdText, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        businessTypes.Add(new BusinessType()
                        {
                            BusTypeID = reader.GetString("BusTypeID"),
                            BusTypeDescription = reader.GetString("BusTypeDescription"),
                            DisplayOrder = reader.GetInt32("DisplayOrder"),
                            IsActive = reader.GetBoolean("IsActive")
                        });
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

            return businessTypes;
        }

        public List<State> SelectAllStates()
        {
            List<State> states = new List<State>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_select_all_states";
            var cmd = new SqlCommand(cmdText, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        states.Add(new State()
                        {
                            StateID = reader.GetString("StateID"),
                            StateDescription = reader.GetString("StateDescription"),
                            DisplayOrder = reader.GetInt32("DisplayOrder")
                        });
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

            return states;
        }

        public List<IncentiveType> SelectAllIncentiveTypes()
        {
            List<IncentiveType> incentiveTypes = new List<IncentiveType>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_select_all_incentive_types";
            var cmd = new SqlCommand(cmdText, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        incentiveTypes.Add(new IncentiveType()
                        {
                            IncentiveTypeID = reader.GetString("IncentiveTypeID"),
                            IncentiveTypeDescription = reader.GetString("IncentiveTypeDescription"),
                            DisplayOrder = reader.GetInt32("DisplayOrder"),
                            IsActive = reader.GetBoolean("IsActive")
                        });
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

            return incentiveTypes;
        }

        public List<Title> SelectAllTitles()
        {
            List<Title> titles = new List<Title>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_select_all_titles";
            var cmd = new SqlCommand(cmdText, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        titles.Add(new Title()
                        {
                            TitleID = reader.GetString("TitleID"),
                            TitleDescription = reader.GetString("TitleDescription"),
                            DisplayOrder = reader.GetInt32("DisplayOrder")
                        });
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

            return titles;
        }

        public List<Status> SelectAllStatuses()
        {
            List<Status> statuses = new List<Status>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_select_all_statuses";
            var cmd = new SqlCommand(cmdText, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        statuses.Add(new Status()
                        {
                            StatusID = reader.GetString("StatusID"),
                            StatusDescription = reader.GetString("StatusDescription"),
                            DisplayOrder = reader.GetInt32("DisplayOrder"),
                            IsActive = reader.GetBoolean("IsActive")
                        });
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

            return statuses;
        }

        public List<AccountStatusInfo> SelectAllAccountStatuses()
        {
            List<AccountStatusInfo> accountStatuses = new List<AccountStatusInfo>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_select_all_account_statuses";
            var cmd = new SqlCommand(cmdText, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        accountStatuses.Add(new AccountStatusInfo()
                        {
                            AccountStatusID = reader.GetString("AccountStatusID"),
                            AccountStatusDesc = reader.GetString("AccountStatusDesc"),
                            DisplayOrder = reader.GetInt32("DisplayOrder"),
                            IsActive = reader.GetBoolean("IsActive")
                        });
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

            return accountStatuses;
        }

        public List<MembershipLevelInfo> SelectAllMembershipLevels()
        {
            List<MembershipLevelInfo> membershipLevels = new List<MembershipLevelInfo>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_select_all_membership_levels";
            var cmd = new SqlCommand(cmdText, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        membershipLevels.Add(new MembershipLevelInfo()
                        {
                            MemLevelID = reader.GetString("MemLevelID"),
                            MemLevelDescription = reader.GetString("MemLevelDescription"),
                            DisplayOrder = reader.GetInt32("DisplayOrder"),
                            IsActive = reader.GetBoolean("IsActive")
                        });
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

            return membershipLevels;
        }

        public List<MembershipLevelInfo> SelectActiveMembershipLevels()
        {
            List<MembershipLevelInfo> membershipLevels = new List<MembershipLevelInfo>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_select_active_membership_levels";
            var cmd = new SqlCommand(cmdText, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        membershipLevels.Add(new MembershipLevelInfo()
                        {
                            MemLevelID = reader.GetString("MemLevelID"),
                            MemLevelDescription = reader.GetString("MemLevelDescription"),
                            DisplayOrder = reader.GetInt32("DisplayOrder"),
                            IsActive = reader.GetBoolean("IsActive")
                        });
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

            return membershipLevels;
        }
    }
}



















