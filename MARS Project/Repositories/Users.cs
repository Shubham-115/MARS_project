
using Humanizer;
using MARS_Project.Connection;
using MARS_Project.Models;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MARS_Project.Repositories
{
    public class Users : IUsers
    {
        private readonly StringConnection _conn;
        public Users(StringConnection conn)
        {
            _conn = conn;

        }





        public async Task<int> UserSingUp(SignUp signup)
        {
            if (IsexistEmail(signup.EmailID))
                return 0;

            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                await con.OpenAsync();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    // 1️⃣ Insert User & get UserID
                    string userQuery = @"
                INSERT INTO dbo.Users
                (MobileNo, EmailID, EmailVerified, MobileVerified, FirstName, LastName, Status, CreatedBy)
                VALUES
                (@MobileNo, @EmailID, 0, 0, @FirstName, @LastName, 0, @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    SqlCommand userCmd = new SqlCommand(userQuery, con, transaction);
                    userCmd.Parameters.AddWithValue("@MobileNo", signup.MobileNo);
                    userCmd.Parameters.AddWithValue("@EmailID", signup.EmailID);
                    userCmd.Parameters.AddWithValue("@FirstName", signup.FirstName);
                    userCmd.Parameters.AddWithValue("@LastName", signup.LastName);
                    userCmd.Parameters.AddWithValue("@CreatedBy", signup.FirstName);

                    int userId = (int)await userCmd.ExecuteScalarAsync();

                    // 2️⃣ Get RoleID (Citizen)
                    string roleQuery = "SELECT RoleID FROM dbo.UserRoles WHERE RoleName = @RoleName";
                    SqlCommand roleCmd = new SqlCommand(roleQuery, con, transaction);
                    roleCmd.Parameters.AddWithValue("@RoleName", "Citizen");

                    object roleResult = await roleCmd.ExecuteScalarAsync();
                    int roleId;

                    if (roleResult == null)
                    {
                        // Insert role if not exists
                        string insertRole = @"
                    INSERT INTO dbo.UserRoles (RoleName)
                    VALUES (@RoleName);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        SqlCommand insertRoleCmd = new SqlCommand(insertRole, con, transaction);
                        insertRoleCmd.Parameters.AddWithValue("@RoleName", "Citizen");
                        roleId = (int)await insertRoleCmd.ExecuteScalarAsync();
                    }
                    else
                    {
                        roleId = Convert.ToInt32(roleResult);
                    }

                    // 3️⃣ Map User with Role
                    string mapQuery = @"
                INSERT INTO dbo.UserRoleMapping (UserID, RoleID, AssignedAt)
                VALUES (@UserID, @RoleID, @AssignedAt)";

                    SqlCommand mapCmd = new SqlCommand(mapQuery, con, transaction);
                    mapCmd.Parameters.AddWithValue("@UserID", userId);
                    mapCmd.Parameters.AddWithValue("@RoleID", roleId);
                    mapCmd.Parameters.AddWithValue("@AssignedAt", DateTime.Now);

                    await mapCmd.ExecuteNonQueryAsync();

                    // 4️⃣ Commit
                    transaction.Commit();
                    return 1;
                }
                catch
                {
                    transaction.Rollback();
                    return 0;
                }
            }
        }



        public Task<int> UserLogin(Login login)
        {
            return Task.FromResult(0);
        }



        public bool IsexistEmail(string EmailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string query = @"Select COUNT(*) from  dbo.Users where EmailID = @EmailID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                // cmd.Parameters.AddWithValue("@MobileNo", MobileNo);
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }

        }

        public async Task<bool> isVerifiedMobile(string MobileNo)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string query = @"Select COUNT(*) from  dbo.Users where MobileNo = @MobileNo and MobileVerified =1";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@MobileNo", MobileNo);
                con.Open();
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return false;

                return Convert.ToBoolean(result);
            }
        }

        public async Task<bool> isVerifiedEmail(string EmailID)
        {

            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string query = @"Select COUNT(*) from  dbo.Users where EmailID = @EmailID and EmailVerified = 1 and MobileVerified = 1";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                con.OpenAsync();
                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        //  write a function to check the mailId and password in valid or not
        public bool VerifyEmailPassword(string EmailID, string Password)
        {

            string PasswordHash = Password; //ConvertHashPassword(Password);
            int result =0;

            using (SqlConnection conn = new SqlConnection(_conn.Dbcs))
            {
                string CheckQuery = @"SELECT u.UserID, r.RoleName FROM dbo.Users u INNER JOIN dbo.UserRoleMapping urm  ON u.UserID = urm.UserID INNER JOIN dbo.UserRoles r  ON urm.RoleID = r.RoleID WHERE u.EmailID = @EmailID   AND u.PasswordHash = @PasswordHash  AND u.Status = 1;";

                SqlCommand cmd = new SqlCommand(CheckQuery, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@PasswordHash", PasswordHash);
                conn.Open();

                 result= cmd.ExecuteNonQuery();               
            }   
            if(result != 0) {
                using (SqlConnection con = new SqlConnection(_conn.Dbcs))
                {
                    string query = @"UPDATE dbo.Users  SET LastLoginAt = @LastLoginAt where EmailID = @EmailID and PasswordHash = @PasswordHash";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@EmailID", EmailID);
                    cmd.Parameters.AddWithValue("@PasswordHash", PasswordHash);
                    cmd.Parameters.AddWithValue("@LastLoginAt", DateTime.Now);
                    con.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
                
            }
            return false;
        }


        public bool IsValidEmailAndMobile(string EmailID, string MobileNo)
        {


            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string query = @"Select Count(*) from dbo.Users  where EmailID = @EmailID and MobileNo = @MobileNo";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@MobileNo", MobileNo);

                con.Open();

                int count = (int)cmd.ExecuteScalar();
                return count > 0;

            }
        }



        // write a function to add login 
        public void UpdateLoginTime(string EmailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string UpdateQuery = " UPDATE dbo.Users  SET LastLoginAt = @LastLoginAt WHERE EmailID = @EmailID";
                SqlCommand cmd = new SqlCommand(UpdateQuery, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@LastLoginAt", DateTime.Now);
                con.Open();
                int id = (int)cmd.ExecuteNonQuery();
            }
            return;
        }



        // Write a function to Generate Random String for OTP And Password
        public string GenerateRandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
                result[i] = chars[random.Next(chars.Length)];

            return new string(result);
        }



        // write a function to Convert the password into HashPassword
        public string ConvertHashPassword(string password)
        {
            using (SHA512 sha = SHA512.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);

                byte[] hashBytes = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }



        // write a function to change Password Force fully
        public string PassWordChange(string EmailID, string Password)
        {

            string PasswordHash = ConvertHashPassword(Password);
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string QUERY = "UPDATE dbo.Users SET PasswordHash = @PasswordHash,Status = @Status where EmailID = @EmailID";
                SqlCommand cmd = new SqlCommand(QUERY, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@PasswordHash", PasswordHash);
                cmd.Parameters.AddWithValue("@Status", 1);

                con.Open();
                int id = cmd.ExecuteNonQuery();
            }
            return "Password Change Successfully ";
        }



        // write a functio to check the status of the user 
        public bool UserStatus(string EmailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string query = "Select Status from dbo.Users where EmailID = @EmailID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);

                con.Open();
                SqlDataReader read = cmd.ExecuteReader();

                if (read.Read())
                {
                    int Status = Convert.ToInt32(read["status"]);
                    return Status == 1;
                }

            }
            return false;
        }


        // write a function to set the Status value afert SignUp and forgot password
        public void setStatus(string EmailID, int Status)
        {

            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string query = "Update  Users set Status = @Status where EmailID = @EmailID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@Status", Status);
                con.Open();
                cmd.ExecuteNonQuery();

            }
            return;

        }


        // write a functio to verify Token 

        public DateTime VerifyUser(string token, string EmailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                // Step 1: Get token generated time
                string checkQuery = "SELECT TokenGeneratedAt FROM Users WHERE token = @token AND EmailID = @EmailID";

                DateTime GeneratetokenTime;

                using (var cmd = new SqlCommand(checkQuery, con))
                {
                    cmd.Parameters.AddWithValue("@token", token);
                    cmd.Parameters.AddWithValue("@EmailID", EmailID);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    con.Close();

                    if (result == null)
                        return DateTime.MinValue; // Invalid token

                    GeneratetokenTime = Convert.ToDateTime(result);
                }

                // Step 2: Check expiry (30 minutes)
                if ((DateTime.Now - GeneratetokenTime).TotalMinutes > 30)
                {
                    return DateTime.MinValue; // Token expired
                }

                // Step 3: Verify user
                string updateQuery = "UPDATE Users SET IsUsedToken = 1, token = NULL, EmailVerified = 1 WHERE token = @token";

                using (var cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@token", token);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    con.Close();

                    if (rows > 0)
                        return GeneratetokenTime; // Verified
                    else
                        return DateTime.MinValue; // Token mismatch (should not normally happen)
                }
            }
        }


        // write a funciton to generate the Token and link 

        public string UpdateToken(string EmailID)
        {
            string token = Guid.NewGuid().ToString();

            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string UpdateToken = @"UPDATE Users SET token = @token,TokenGeneratedAt = @TokenGeneratedAt, IsUsedToken = 0 ,EmailVerified = 0, MobileVerified =0 WHERE EmailID = @EmailID AND token IS NULL";
                SqlCommand cmd = new SqlCommand(UpdateToken, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@TokenGeneratedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@EmailVerified", 0);
                cmd.Parameters.AddWithValue("@MobileVerified", 0);


                con.Open();
                cmd.ExecuteNonQuery();
                return token;

            }

            return token;
        }

        // write a function to reset token 
        public bool resetToken(string EmailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string UpdateToken = @"UPDATE Users SET token = @token,TokenGeneratedAt = @TokenGeneratedAt, IsUsedToken = 0,EmailVerified=0 WHERE EmailID = @EmailID ";
                SqlCommand cmd = new SqlCommand(UpdateToken, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@token", DBNull.Value);
                cmd.Parameters.AddWithValue("@TokenGeneratedAt", DBNull.Value);
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                con.Open();
                int row = cmd.ExecuteNonQuery();
                return row > 0;
            }
        }

        // Write a function to verify mobile and Email address of the user ie valid or not
        public bool IsMobileExist(string MobileNo)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string query = @"select count(*) from dbo.Users where MobileNO = @MobileNo";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                //cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@MobileNo", MobileNo);
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                return count > 0;

            }
        }

        // write a function to Update Otp 
        public string GetOTP(string MobileNo)
        {
            string OTP = GenerateRandomString(6);

            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string UpdateToken = @"UPDATE Users SET OTPCode = @OTPCode,OTPGeneratedAt = @OTPGeneratedAt WHERE MobileNo = @MobileNo ";
                SqlCommand cmd = new SqlCommand(UpdateToken, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@OTPCode", OTP);
                cmd.Parameters.AddWithValue("OTPGeneratedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@MobileNo", MobileNo);
                con.Open();
                cmd.ExecuteNonQuery();
                return OTP;

            }
            return "Invalid Email and  Mobile";
        }

        // write a function to verify otp 
        public DateTime VerifyOTP(string OTPCode, string MobileNo)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                // Step 1: Get token generated time
                string checkQuery = "SELECT OTPGeneratedAt FROM Users WHERE OTPCode = @OTPCode AND MobileNo = @MobileNo";

                DateTime GenerateOTPTime;

                using (var cmd = new SqlCommand(checkQuery, con))
                {
                    cmd.Parameters.AddWithValue("@OTPCode", OTPCode);
                    cmd.Parameters.AddWithValue("@MobileNo", MobileNo);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    con.Close();

                    if (result == null)
                        return DateTime.MinValue; // Invalid token

                    GenerateOTPTime = Convert.ToDateTime(result);
                }

                // Step 2: Check expiry (30 minutes)
                if ((DateTime.Now - GenerateOTPTime).TotalMinutes > 5)
                {
                    return DateTime.MinValue; // Token expired
                }

                // Step 3: Verify user
                string updateQuery = "UPDATE Users SET MobileVerified = 1  WHERE OTPCode = @OTPCode";

                using (var cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@OTPCode", OTPCode);

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    con.Close();

                    if (rows > 0)
                        return GenerateOTPTime; // Verified
                    else
                        return DateTime.MinValue; // Token mismatch (should not normally happen)
                }
            }
        }

        public bool SetPassWord(string EmailID, string Password)
        {

            string PasswordHash = ConvertHashPassword(Password);
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                string QUERY = "UPDATE dbo.Users SET PasswordHash = @PasswordHash,Status = @Status where EmailID = @EmailID";
                SqlCommand cmd = new SqlCommand(QUERY, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@PasswordHash", PasswordHash);
                cmd.Parameters.AddWithValue("@Status", 0);

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0;

            }

        }

       
    }
}

