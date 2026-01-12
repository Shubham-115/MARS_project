using MARS_Project.Connection;
using MARS_Project.Models;
using MARS_Project.Models.Citizen;
using MARS_Project.Models.FairAdmin;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace MARS_Project.Repositories
{
    public class AddFair : IAddFair
    {
        private readonly StringConnection _conn;
        private readonly Users user;
        public AddFair(StringConnection conn, Users user)
        {
            _conn = conn;
            this.user = user;
        }
        public async Task<string> AddBlock(Block block)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                SqlCommand cmd = new SqlCommand("AddBlock", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SubSectorID", block.SubSectorID);
                cmd.Parameters.AddWithValue("BlockName", block.BlockName);
                cmd.Parameters.AddWithValue("BlockGroup", block.BlockGroup);
                cmd.Parameters.AddWithValue("Description", block.Description);
                cmd.Parameters.AddWithValue("IsActive", block.IsActive);

                try
                {
                    await con.OpenAsync();
                    cmd.ExecuteNonQuery();
                    return "Block Inserted Successfully .....";

                }
                catch (SqlException ex)
                {
                    // RAISERROR from SP will be caught here
                    return "SQL Error: " + ex.Message;
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message;
                }

            }
        }

        public async Task<int> AddSector(Sector sector)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("ADDSECTOR", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FairID", sector.FairID);
                cmd.Parameters.AddWithValue("@SectorName", sector.SectorName);
                cmd.Parameters.AddWithValue("@SectorGroup", sector.SectorGroup);
                cmd.Parameters.AddWithValue("@Area", sector.Area);
                cmd.Parameters.AddWithValue("@Description", sector.Description);
                cmd.Parameters.AddWithValue("@IsActive", sector.IsActive);
                object result = await cmd.ExecuteScalarAsync();
                if (result == null)
                    return 0; // user not found

                return Convert.ToInt32(result);
            }
        }

        public async Task<string> AddSubSector(Subsector subsector)
        {

            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                SqlCommand cmd = new SqlCommand("AddSubsector", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SectorID", subsector.SectorID);
                cmd.Parameters.AddWithValue("SubsectorName", subsector.SubSectorName);
                cmd.Parameters.AddWithValue("GroupName", subsector.GroupName);
                cmd.Parameters.AddWithValue("Description", subsector.Description);
                cmd.Parameters.AddWithValue("IsActive", subsector.IsActive);

                try
                {
                    await con.OpenAsync();
                    cmd.ExecuteNonQuery();

                    return "Subsector Inserted Successfully .....";

                }
                catch (SqlException ex)
                {
                    // RAISERROR from SP will be caught here
                    return "SQL Error: " + ex.Message;
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message;
                }

            }
        }
        public async Task<long> GetFairID(string EmailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("GetFairByEmail", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                object result = await cmd.ExecuteScalarAsync();
                if (result == null)
                    return 0; // user not found

                return Convert.ToInt32(result);

            }
        }

        public async Task<Myprofile> GetProfileAsync(string email)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            using (SqlCommand cmd = new SqlCommand("GetProfile", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmailID", SqlDbType.NVarChar, 100).Value = email;

                await con.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Myprofile
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FName = reader.GetString(reader.GetOrdinal("FName")),
                            LName = reader.GetString(reader.GetOrdinal("LName")),
                            Email = reader.GetString(reader.GetOrdinal("EmailID")),
                            MobileNo = reader.GetString(reader.GetOrdinal("MobileNo")),
                            role = reader.GetString(reader.GetOrdinal("Role")),
                            createdAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                        };
                    }
                }
            }
            return null; 
        }

        public Task<Myprofile> profile(Myprofile myprofile)
        {
            throw new NotImplementedException();
        }
    }
}


