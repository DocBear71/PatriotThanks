using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessInterfaces;
using DataDomain;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class IncentiveAccessor : IIncentiveAccessor
    {
       
        public List<Incentive> SelectIncentivesByBusinessID(int businessID)
        {
            List<Incentive> incentives = new List<Incentive>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_select_incentives_by_business";

            using (var cmd = new SqlCommand(cmdText, conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BusinessID", businessID);

                try
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                incentives.Add(new Incentive()
                                {
                                    IncentiveID = reader.GetInt32(reader.GetOrdinal("IncentiveID")),
                                    BusinessID = reader.GetInt32(reader.GetOrdinal("BusinessID")),
                                    IncentiveAmount = reader.GetDecimal(reader.GetOrdinal("IncentiveAmount")),
                                    IncentiveDescription = reader.GetString(reader.GetOrdinal("IncentiveDescription")),
                                    StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                    EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                    Limitations = reader.IsDBNull(reader.GetOrdinal("Limitations"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("Limitations")),
                                    IncentiveTypesDisplay = reader.IsDBNull(reader.GetOrdinal("IncentiveTypesDisplay"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("IncentiveTypesDisplay")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    LastUpdated = reader.GetDateTime(reader.GetOrdinal("LastUpdated"))
                                });
                            }
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
            }

            return incentives;
        }

        public Incentive SelectIncentiveByID(int incentiveID)
        {
            Incentive incentive = null;

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_select_incentive_by_id";

            using (var cmd = new SqlCommand(cmdText, conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IncentiveID", incentiveID);

                try
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        // First result set: Main incentive data
                        if (reader.HasRows && reader.Read())
                        {
                            incentive = new Incentive()
                            {
                                IncentiveID = reader.GetInt32(reader.GetOrdinal("IncentiveID")),
                                BusinessID = reader.GetInt32(reader.GetOrdinal("BusinessID")),
                                BusinessName = reader.GetString(reader.GetOrdinal("BusinessName")),
                                IncentiveAmount = reader.GetDecimal(reader.GetOrdinal("IncentiveAmount")),
                                IsPercentage = reader.GetBoolean(reader.GetOrdinal("IsPercentage")),
                                IncentiveDescription = reader.GetString(reader.GetOrdinal("IncentiveDescription")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                Limitations = reader.IsDBNull(reader.GetOrdinal("Limitations"))
                                    ? ""
                                    : reader.GetString(reader.GetOrdinal("Limitations")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                LastUpdated = reader.GetDateTime(reader.GetOrdinal("LastUpdated"))
                            };
                        }

                        // Second result set: Related incentive types
                        if (incentive != null && reader.NextResult())
                        {
                            var incentiveTypes = new List<string>();
                            while (reader.Read())
                            {
                                incentiveTypes.Add(reader.GetString(reader.GetOrdinal("IncentiveTypeDescription")));
                            }
                            incentive.IncentiveTypesDisplay = string.Join(", ", incentiveTypes);
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
            }

            return incentive;
        }

        public List<Incentive> SearchIncentives(IncentiveSearchCriteria criteria)
        {
            List<Incentive> incentives = new List<Incentive>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_search_incentives";

            using (var cmd = new SqlCommand(cmdText, conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Incentive filter parameters
                cmd.Parameters.AddWithValue("@BusinessID",
                    criteria.BusinessID.HasValue ? (object)criteria.BusinessID.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@IncentiveTypeID",
                    string.IsNullOrWhiteSpace(criteria.IncentiveTypeID) ? (object)DBNull.Value : criteria.IncentiveTypeID);
                cmd.Parameters.AddWithValue("@MinAmount",
                    criteria.MinAmount.HasValue ? (object)criteria.MinAmount.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@MaxAmount",
                    criteria.MaxAmount.HasValue ? (object)criteria.MaxAmount.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@ActiveOnly", criteria.ActiveOnly);

                // Business filter parameters
                cmd.Parameters.AddWithValue("@BusinessName",
                    string.IsNullOrWhiteSpace(criteria.BusinessName) ? (object)DBNull.Value : criteria.BusinessName);
                cmd.Parameters.AddWithValue("@StreetAddress",
                    string.IsNullOrWhiteSpace(criteria.StreetAddress) ? (object)DBNull.Value : criteria.StreetAddress);
                cmd.Parameters.AddWithValue("@City",
                    string.IsNullOrWhiteSpace(criteria.City) ? (object)DBNull.Value : criteria.City);
                cmd.Parameters.AddWithValue("@StateID",
                    string.IsNullOrWhiteSpace(criteria.StateID) ? (object)DBNull.Value : criteria.StateID);
                cmd.Parameters.AddWithValue("@Zip",
                    string.IsNullOrWhiteSpace(criteria.Zip) ? (object)DBNull.Value : criteria.Zip);
                cmd.Parameters.AddWithValue("@BusinessTypeID",
                    string.IsNullOrWhiteSpace(criteria.BusinessTypeID) ? (object)DBNull.Value : criteria.BusinessTypeID);
                


                try
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                incentives.Add(new Incentive()
                                {
                                    IncentiveID = reader.GetInt32(reader.GetOrdinal("IncentiveID")),
                                    BusinessID = reader.GetInt32(reader.GetOrdinal("BusinessID")),
                                    BusinessName = reader.GetString(reader.GetOrdinal("BusinessName")),
                                    BusinessType = reader.IsDBNull(reader.GetOrdinal("BusinessType"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("BusinessType")),
                                    LocationName = reader.IsDBNull(reader.GetOrdinal("LocationName"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("LocationName")),
                                    IncentiveAmount = reader.GetDecimal(reader.GetOrdinal("IncentiveAmount")),
                                    IsPercentage = reader.GetBoolean(reader.GetOrdinal("IsPercentage")),
                                    IncentiveDescription = reader.GetString(reader.GetOrdinal("IncentiveDescription")),
                                    StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                    EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                    Limitations = reader.IsDBNull(reader.GetOrdinal("Limitations"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("Limitations")),
                                    IncentiveTypesDisplay = reader.IsDBNull(reader.GetOrdinal("IncentiveTypesDisplay"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("IncentiveTypesDisplay")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    LastUpdated = reader.GetDateTime(reader.GetOrdinal("LastUpdated"))
                                });
                            }
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
            }

            return incentives;
        }

        public int InsertIncentive(Incentive incentive, List<string> incentiveTypeIDs)
        {
            int newIncentiveID = 0;

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_insert_incentive";

            using (var cmd = new SqlCommand(cmdText, conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Add parameters
                cmd.Parameters.AddWithValue("@BusinessID", incentive.BusinessID);
                cmd.Parameters.AddWithValue("@IncentiveAmount", incentive.IncentiveAmount);
                cmd.Parameters.AddWithValue("@IsPercentage", incentive.IsPercentage);
                cmd.Parameters.AddWithValue("@IncentiveDescription", incentive.IncentiveDescription);
                cmd.Parameters.AddWithValue("@StartDate", incentive.StartDate);
                cmd.Parameters.AddWithValue("@EndDate",
                    incentive.EndDate.HasValue ? (object)incentive.EndDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Limitations",
                    string.IsNullOrWhiteSpace(incentive.Limitations) ? (object)DBNull.Value : incentive.Limitations);

                // Convert list of incentive types to comma-separated string
                string incentiveTypesString = string.Join(",", incentiveTypeIDs);
                cmd.Parameters.AddWithValue("@IncentiveTypeIDs", incentiveTypesString);

                // Output parameter for the new IncentiveID
                var outputParam = new SqlParameter("@IncentiveID", System.Data.SqlDbType.Int)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                cmd.Parameters.Add(outputParam);

                try
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            newIncentiveID = reader.GetInt32(reader.GetOrdinal("NewIncentiveID"));
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
            }

            return newIncentiveID;
        }

        public int UpdateIncentive(Incentive incentive, List<string> incentiveTypeIDs)
        {
            int rowsAffected = 0;

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_update_incentive";

            using (var cmd = new SqlCommand(cmdText, conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Add parameters
                cmd.Parameters.AddWithValue("@IncentiveID", incentive.IncentiveID);
                cmd.Parameters.AddWithValue("@BusinessID", incentive.BusinessID);
                cmd.Parameters.AddWithValue("@IncentiveAmount", incentive.IncentiveAmount);
                cmd.Parameters.AddWithValue("@IsPercentage", incentive.IsPercentage);
                cmd.Parameters.AddWithValue("@IncentiveDescription", incentive.IncentiveDescription);
                cmd.Parameters.AddWithValue("@StartDate", incentive.StartDate);
                cmd.Parameters.AddWithValue("@EndDate",
                    incentive.EndDate.HasValue ? (object)incentive.EndDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Limitations",
                    string.IsNullOrWhiteSpace(incentive.Limitations) ? (object)DBNull.Value : incentive.Limitations);

                // Convert list of incentive types to comma-separated string
                string incentiveTypesString = string.Join(",", incentiveTypeIDs);
                cmd.Parameters.AddWithValue("@IncentiveTypeIDs", incentiveTypesString);

                try
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            rowsAffected = reader.GetInt32(reader.GetOrdinal("RowsAffected"));
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
            }

            return rowsAffected;
        }
    }
}
