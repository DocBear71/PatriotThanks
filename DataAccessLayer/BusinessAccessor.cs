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
    public class BusinessAccessor : IBusinessAccessor
    {

        public List<Business> SearchBusinesses(BusinessSearchCriteria criteria)
        {
            List<Business> businesses = new List<Business>();

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_search_businesses";

            using (var cmd = new SqlCommand(cmdText, conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Add parameters - null if empty string
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
                cmd.Parameters.AddWithValue("@IsActive", criteria.IsActive);

                try
                {
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                businesses.Add(new Business()
                                {
                                    BusinessID = reader.GetInt32(reader.GetOrdinal("BusinessID")),
                                    BusinessName = reader.GetString(reader.GetOrdinal("BusinessName")),
                                    BusinessTypeID = reader.GetString(reader.GetOrdinal("BusinessTypeID")),
                                    BusinessType = reader.GetString(reader.GetOrdinal("BusinessType")),
                                    LocationID = reader.IsDBNull(reader.GetOrdinal("LocationID"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("LocationID")),
                                    LocationName = reader.IsDBNull(reader.GetOrdinal("LocationName"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("LocationName")),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("Phone")),
                                    StreetAddress = reader.IsDBNull(reader.GetOrdinal("StreetAddress"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("StreetAddress")),
                                    Address2 = reader.IsDBNull(reader.GetOrdinal("Address2"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("Address2")),
                                    City = reader.IsDBNull(reader.GetOrdinal("City"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("City")),
                                    StateID = reader.IsDBNull(reader.GetOrdinal("StateID"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("StateID")),
                                    StateName = reader.IsDBNull(reader.GetOrdinal("StateName"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("StateName")),
                                    Zip = reader.IsDBNull(reader.GetOrdinal("Zip"))
                                        ? ""
                                        : reader.GetString(reader.GetOrdinal("Zip")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
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

            return businesses;
        }

        public Business SelectBusinessByID(int businessID)
        {
            Business business = null;

            var conn = DBConnection.GetConnection();

            // DEBUG: Check connection and database
            System.Diagnostics.Debug.WriteLine($"Connection String: {conn.ConnectionString}");
            System.Diagnostics.Debug.WriteLine($"Attempting to get BusinessID: {businessID}");

            var cmdText = "sp_select_business_by_id";

            using (var cmd = new SqlCommand(cmdText, conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BusinessID", businessID);

                try
                {
                    conn.Open();

                    // DEBUG: Confirm connection opened
                    System.Diagnostics.Debug.WriteLine($"Connected to database: {conn.Database}");

                    using (var reader = cmd.ExecuteReader())
                    {
                        // DEBUG: Check if data returned
                        System.Diagnostics.Debug.WriteLine($"HasRows: {reader.HasRows}");

                        if (reader.HasRows && reader.Read())
                        {
                            business = new Business()
                            {
                                BusinessID = reader.GetInt32(reader.GetOrdinal("BusinessID")),
                                BusinessName = reader.GetString(reader.GetOrdinal("BusinessName")),
                                BusinessTypeID = reader.GetString(reader.GetOrdinal("BusinessTypeID")),
                                BusinessType = reader.GetString(reader.GetOrdinal("BusinessType")),
                                ParentBusinessID = reader.IsDBNull(reader.GetOrdinal("ParentBusinessID"))
                                    ? (int?)null
                                    : reader.GetInt32(reader.GetOrdinal("ParentBusinessID")),
                                LocationID = reader.IsDBNull(reader.GetOrdinal("LocationID"))
                                    ? (int?)null
                                    : reader.GetInt32(reader.GetOrdinal("LocationID")),
                                LocationName = reader.IsDBNull(reader.GetOrdinal("LocationName"))
                                    ? ""
                                    : reader.GetString(reader.GetOrdinal("LocationName")),
                                Phone = reader.IsDBNull(reader.GetOrdinal("Phone"))
                                    ? ""
                                    : reader.GetString(reader.GetOrdinal("Phone")),
                                StreetAddress = reader.IsDBNull(reader.GetOrdinal("StreetAddress"))
                                    ? ""
                                    : reader.GetString(reader.GetOrdinal("StreetAddress")),
                                Address2 = reader.IsDBNull(reader.GetOrdinal("Address2"))
                                    ? ""
                                    : reader.GetString(reader.GetOrdinal("Address2")),
                                City = reader.IsDBNull(reader.GetOrdinal("City"))
                                    ? ""
                                    : reader.GetString(reader.GetOrdinal("City")),
                                StateID = reader.IsDBNull(reader.GetOrdinal("StateID"))
                                    ? ""
                                    : reader.GetString(reader.GetOrdinal("StateID")),
                                StateName = reader.IsDBNull(reader.GetOrdinal("StateName"))
                                    ? ""
                                    : reader.GetString(reader.GetOrdinal("StateName")),
                                Zip = reader.IsDBNull(reader.GetOrdinal("Zip"))
                                    ? ""
                                    : reader.GetString(reader.GetOrdinal("Zip")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                            };
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

            return business;
        }

        public int UpdateBusiness(Business business)
        {
            int rowsAffected = 0;

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_update_business";

            using (var cmd = new SqlCommand(cmdText, conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@BusinessID", business.BusinessID);
                cmd.Parameters.AddWithValue("@BusinessName", business.BusinessName);
                cmd.Parameters.AddWithValue("@BusinessTypeID", business.BusinessTypeID);
                cmd.Parameters.AddWithValue("@Phone",
                    string.IsNullOrWhiteSpace(business.Phone) ? (object)DBNull.Value : business.Phone);
                cmd.Parameters.AddWithValue("@StreetAddress",
                    string.IsNullOrWhiteSpace(business.StreetAddress) ? (object)DBNull.Value : business.StreetAddress);
                cmd.Parameters.AddWithValue("@Address2",
                    string.IsNullOrWhiteSpace(business.Address2) ? (object)DBNull.Value : business.Address2);
                cmd.Parameters.AddWithValue("@City",
                    string.IsNullOrWhiteSpace(business.City) ? (object)DBNull.Value : business.City);
                cmd.Parameters.AddWithValue("@StateID",
                    string.IsNullOrWhiteSpace(business.StateID) ? (object)DBNull.Value : business.StateID);
                cmd.Parameters.AddWithValue("@Zip",
                    string.IsNullOrWhiteSpace(business.Zip) ? (object)DBNull.Value : business.Zip);
                cmd.Parameters.AddWithValue("@IsActive", business.IsActive);

                try
                {
                    conn.Open();
                    rowsAffected = (int)cmd.ExecuteScalar();
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

       
        public int InsertBusiness(Business business)
        {
            int newBusinessID = 0;

            var conn = DBConnection.GetConnection();
            var cmdText = "sp_insert_business";

            using (var cmd = new SqlCommand(cmdText, conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@BusinessName", business.BusinessName);
                cmd.Parameters.AddWithValue("@BusinessTypeID", business.BusinessTypeID);
                cmd.Parameters.AddWithValue("@LocationName",
                    string.IsNullOrWhiteSpace(business.LocationName) ? (object)DBNull.Value : business.LocationName);
                cmd.Parameters.AddWithValue("@Phone",
                    string.IsNullOrWhiteSpace(business.Phone) ? (object)DBNull.Value : business.Phone);
                cmd.Parameters.AddWithValue("@StreetAddress", business.StreetAddress);
                cmd.Parameters.AddWithValue("@Address2",
                    string.IsNullOrWhiteSpace(business.Address2) ? (object)DBNull.Value : business.Address2);
                cmd.Parameters.AddWithValue("@City", business.City);
                cmd.Parameters.AddWithValue("@StateID", business.StateID);
                cmd.Parameters.AddWithValue("@Zip",
                    string.IsNullOrWhiteSpace(business.Zip) ? (object)DBNull.Value : business.Zip);

                // Output parameter
                var outputParam = new SqlParameter("@BusinessID", System.Data.SqlDbType.Int)
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
                            newBusinessID = reader.GetInt32(reader.GetOrdinal("NewBusinessID"));
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

            return newBusinessID;
        }
    }
}
