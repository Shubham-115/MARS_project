using MARS_Project.Connection;
using MARS_Project.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace MARS_Project.Repositories
{
    public class Fair : IFair
    {
        private readonly StringConnection _conn;
        public Fair(StringConnection conn)
        {
            _conn = conn;

        }


        public async Task<string> CreateFair(Createfair model)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                await con.OpenAsync();
                string query = @"
                    INSERT INTO TradeFair
                    (FairName, Division, District, Tehsil, City, StartDate, EndDate, ApplyStartDate, ApplyEndDate, FairLogoPath, ContactMobile1, ContactMobile2, ContactEmail, Status, CreatedBy, CreatedAt)
                    VALUES
                    (@FairName, @Division, @District, @Tehsil, @City, @StartDate, @EndDate, @ApplyStartDate, @ApplyEndDate, @FairLogoPath, @ContactMobile1, @ContactMobile2, @ContactEmail, @Status, @CreatedBy, @CreatedAt);";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FairName", model.FairName);
                    cmd.Parameters.AddWithValue("@Division", model.Division ?? "");
                    cmd.Parameters.AddWithValue("@District", model.District ?? "");
                    cmd.Parameters.AddWithValue("@Tehsil", model.Tehsil ?? "");
                    cmd.Parameters.AddWithValue("@City", model.City ?? "");
                    cmd.Parameters.AddWithValue("@StartDate", (object)model.StartDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndDate", (object)model.EndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApplyStartDate", (object)model.ApplyStartDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApplyEndDate", (object)model.ApplyEndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FairLogoPath", model.FairLogoPathString ?? "");
                    cmd.Parameters.AddWithValue("@ContactMobile1", model.ContactMobile1 ?? "");
                    cmd.Parameters.AddWithValue("@ContactMobile2", model.ContactMobile2 ?? "");
                    cmd.Parameters.AddWithValue("@ContactEmail", model.ContactEmail ?? "");
                    cmd.Parameters.AddWithValue("@Status", model.Status);
                    cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy ?? "Super Admin");
                    cmd.Parameters.AddWithValue("@CreatedAt", model.CreatedAt);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0 ? "SUCCESS" : "FAILED";
                }
            }
        }



        public Task<string> DeleteFair()
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateFair()
        {
            throw new NotImplementedException();
        }
    }
}

