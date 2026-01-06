using MARS_Project.Connection;
using MARS_Project.Models.FairAdmin;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MARS_Project.Repositories
{
    public class AddFair : IAddFair
    {
        private readonly StringConnection _conn;
        public AddFair(StringConnection conn)
        {
            _conn = conn;

        }
        public Task<string> AddBlock(Block block)
        {
            throw new NotImplementedException();
        }

        public async Task<int> AddSector(Sector sector)
        {
           using(SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("ADDSECTOR",con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FairID",sector.FairID);
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

        public Task<string> AddSubSector(Subsector subsector)
        {
            throw new NotImplementedException();
        }
        public async Task<long> GetFairID(string EmailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                await con.OpenAsync();
                SqlCommand cmd = new SqlCommand("GetFairByEmail",con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
               object result  = await cmd.ExecuteScalarAsync();
                if (result == null)
                    return 0; // user not found

                return Convert.ToInt32(result);

            }
        }
    }
}

