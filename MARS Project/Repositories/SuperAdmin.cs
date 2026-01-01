using MARS_Project.Connection;
using MARS_Project.Models;
using MARS_Project.Models.Citizen;
using MARS_Project.Models.SuperAdmin;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Collections.Generic;
using System.Data;

namespace MARS_Project.Repositories
{
    public class SuperAdmin : IFair
    {
        private readonly StringConnection _conn;
        public SuperAdmin(StringConnection conn)
        {
            _conn = conn;

        }

        public async Task<int> CheckFairStatus(SetFairStatus status)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string query = @"SELECT Status 
                         FROM dbo.TradeFair 
                         WHERE ContactEmail = @ContactEmail AND FairID = @FairID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ContactEmail", status.FairEmailId);
                    cmd.Parameters.AddWithValue("@FairID", status.FairId);

                    await con.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result); // 0 or 1 from DB
                    }

                    // Return null if no record found
                    return -1;
                }
            }
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
                    cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
                    cmd.Parameters.AddWithValue("@CreatedAt", model.CreatedAt);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0 ? "SUCCESS" : "FAILED";
                }
            }
        }

       

        public async Task<int> SetFairStatus(SetFairStatus status)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string updatestatus = @"UPDATE TradeFair SET Status = @Status  WHERE ContactEmail = @ContactEmail  AND FairID = @FairID";

                using (SqlCommand cmd = new SqlCommand(updatestatus, con))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@Status", status.Status);
                    cmd.Parameters.AddWithValue("@ContactEmail", status.FairEmailId);
                    cmd.Parameters.AddWithValue("@FairID", status.FairId);

                    await con.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected; // returns number of rows updated
                }
            }
        }




        public Task<string> UpdateFair()
        {
            throw new NotImplementedException();
        }



        public async Task<int> AddFairAdmin(SignUp signup)
        {           

            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                await con.OpenAsync();

                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Insert User
                        string userQuery = @"
                    INSERT INTO dbo.Users
                    (MobileNo, EmailID, EmailVerified, MobileVerified, FirstName, LastName, Status, CreatedBy)
                    VALUES
                    (@MobileNo, @EmailID, 0, 0, @FirstName, @LastName, 0, @CreatedBy);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        using SqlCommand userCmd = new SqlCommand(userQuery, con, transaction);
                        userCmd.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = signup.MobileNo;
                        userCmd.Parameters.Add("@EmailID", SqlDbType.VarChar).Value = signup.EmailID;
                        userCmd.Parameters.Add("@FirstName", SqlDbType.VarChar).Value = signup.FirstName;
                        userCmd.Parameters.Add("@LastName", SqlDbType.VarChar).Value = signup.LastName;
                        userCmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = signup.FirstName;

                        int userId = Convert.ToInt32(await userCmd.ExecuteScalarAsync());

                        // 2️⃣ Get or Create Role
                        string roleName = "FairAdmin";

                        string roleQuery = "SELECT RoleID FROM dbo.UserRoles WHERE RoleName = @RoleName";
                        using SqlCommand roleCmd = new SqlCommand(roleQuery, con, transaction);
                        roleCmd.Parameters.Add("@RoleName", SqlDbType.VarChar).Value = roleName;

                        object roleResult = await roleCmd.ExecuteScalarAsync();
                        int roleId;

                        if (roleResult == null)
                        {
                            string insertRoleQuery = @"
                        INSERT INTO dbo.UserRoles (RoleName)
                        VALUES (@RoleName);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                            using SqlCommand insertRoleCmd = new SqlCommand(insertRoleQuery, con, transaction);
                            insertRoleCmd.Parameters.Add("@RoleName", SqlDbType.VarChar).Value = roleName;

                            roleId = Convert.ToInt32(await insertRoleCmd.ExecuteScalarAsync());
                        }
                        else
                        {
                            roleId = Convert.ToInt32(roleResult);
                        }

                        // 3️⃣ Map User to Role
                        string mapQuery = @"
                    INSERT INTO dbo.UserRoleMapping (UserID, RoleID, AssignedAt)
                    VALUES (@UserID, @RoleID, @AssignedAt)";

                        using SqlCommand mapCmd = new SqlCommand(mapQuery, con, transaction);
                        mapCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                        mapCmd.Parameters.Add("@RoleID", SqlDbType.Int).Value = roleId;
                        mapCmd.Parameters.Add("@AssignedAt", SqlDbType.DateTime).Value = DateTime.UtcNow;

                        await mapCmd.ExecuteNonQueryAsync();

                        transaction.Commit();
                        return 1;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return 0;
                    }
                }
            }
        }
    }
}

