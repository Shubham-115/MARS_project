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
       
        public AddFair(StringConnection conn)
        {
            _conn = conn;
            
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

        public async Task<Sector> getSetor(long SectorID, long FairID)
        {
            List<Sector> sectors = new List<Sector>();
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("GetSector", con);
                cmd.CommandType= CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SectorID",SectorID);
                cmd.Parameters.AddWithValue("@FairID", FairID);


                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    return new Sector
                    {

                        SectorID = (int)reader.GetInt64(reader.GetOrdinal("SectorID")),
                        FairID = (int)reader.GetInt64(reader.GetOrdinal("FairID")),
                        SectorName = reader.GetString(reader.GetOrdinal("SectorName")),
                        SectorGroup = reader.GetString(reader.GetOrdinal("SectorGroup")),
                        Area = reader.GetString(reader.GetOrdinal("Area")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))

                    };
                }
            }
            return null;
        }

        public async Task<int> UpdateSector(Sector sector)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("UpdateSector", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SectorID", sector.SectorID);
                cmd.Parameters.AddWithValue("@FairID", sector.FairID);
                cmd.Parameters.AddWithValue("@SectorName", sector.SectorName);
                cmd.Parameters.AddWithValue("@SectorGroup", sector.SectorGroup);
                cmd.Parameters.AddWithValue("@Area", sector.Area);
                cmd.Parameters.AddWithValue("@Description", sector.Description);
                cmd.Parameters.AddWithValue("@IsActive", sector.IsActive);

                int reader = await cmd.ExecuteNonQueryAsync();

                return reader;
            }
        }


        public async Task<Subsector> getSubSetor(long SectorID, long SubSectorID)
        {
            List<Subsector> sectors = new List<Subsector>();
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("GetSubSector", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SectorID", SectorID);
                cmd.Parameters.AddWithValue("@SubSectorID", SubSectorID);


                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    return new Subsector
                    {

                        SectorID = (int)reader.GetInt64(reader.GetOrdinal("SectorID")),
                        SubSectorID = (int)reader.GetInt64(reader.GetOrdinal("SubSectorID")),
                        SubSectorName = reader.GetString(reader.GetOrdinal("SubSectorName")),
                        GroupName = reader.GetString(reader.GetOrdinal("GroupName")),                       
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))

                    };
                }
            }
            return null;
        }

        public async Task<int> UpdateSubSector(Subsector Subsector)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("UpdateSubSector", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SectorID", Subsector.SectorID);
                cmd.Parameters.AddWithValue("@SubSectorID", Subsector.SubSectorID);
                cmd.Parameters.AddWithValue("@SubSectorName", Subsector.SubSectorName);
                cmd.Parameters.AddWithValue("@GroupName", Subsector.GroupName);                ;
                cmd.Parameters.AddWithValue("@Description", Subsector.Description);
                cmd.Parameters.AddWithValue("@IsActive", Subsector.IsActive);

                int reader = await cmd.ExecuteNonQueryAsync();

                return reader;
            }
        }

       
    }
}


