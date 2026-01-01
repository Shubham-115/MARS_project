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

        public Task<int> AddSector(Sector sector)
        {
            throw new NotImplementedException();
        }

        public Task<string> AddSubSector(Subsector subsector)
        {
            throw new NotImplementedException();
        }
        public async Task<long> GetFairID(string EmailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                SqlCommand cmd = new SqlCommand("GetFairByEmail",con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
               long fairID =  await cmd.ExecuteNonQueryAsync();
                return fairID;

            }
        }
    }
}

