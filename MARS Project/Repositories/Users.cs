using Humanizer;
using MARS_Project.Connection;
using MARS_Project.Models;
using MARS_Project.Models.Citizen;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;
using System.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MARS_Project.Repositories
{
    public class Users : IUsers
    {
        private readonly StringConnection _conn;
        private string dbcs;

        public Users(StringConnection conn)
        {
            _conn = conn;

        }

        public Users(string dbcs)
        {
            this.dbcs = dbcs;
        }

        public async Task<int> UserSingUp(SignUp signup)
        {
            using SqlConnection con = new SqlConnection(_conn.Dbcs);
            await con.OpenAsync();

            using SqlCommand cmd = new SqlCommand("sp_UserSignUp", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@MobileNo", SqlDbType.VarChar).Value = signup.MobileNo;
            cmd.Parameters.Add("@EmailID", SqlDbType.VarChar).Value = signup.EmailID;
            cmd.Parameters.Add("@FirstName", SqlDbType.VarChar).Value = signup.FirstName;
            cmd.Parameters.Add("@LastName", SqlDbType.VarChar).Value = signup.LastName;
            cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = signup.FirstName;

            SqlParameter userIdParam = new SqlParameter("@UserID", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(userIdParam);

            await cmd.ExecuteNonQueryAsync();

            return Convert.ToInt32(userIdParam.Value);
        }





        public async Task<int> UserLogin(Login login)
        {
            string passwordHash = ConvertHashPassword(login.Password);

            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
               
                using (SqlCommand cmd = new SqlCommand("sp_UserLogin", con))
                {
                    // ✅ PARAMETER NAMES MUST MATCH SQL
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@EmailID", SqlDbType.NVarChar, 150).Value = login.EmailID;
                    cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 512).Value = passwordHash;


                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                            return 0;   // Invalid login

                        int statusOrdinal = reader.GetOrdinal("Status");

                        // Handle NULL safely
                        byte status = reader.IsDBNull(statusOrdinal)
                            ? (byte)0   // treat NULL as inactive
                            : reader.GetByte(statusOrdinal);

                        if (status != 1)
                            return -1;  // Inactive user

                        return reader.GetInt32(reader.GetOrdinal("RoleID")); // ✅ ROLE ID
                    }
                }
            }
        }





        public bool IsexistEmail(string EmailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {

                SqlCommand cmd = new SqlCommand("IsEmailExist", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                // cmd.Parameters.AddWithValue("@MobileNo", MobileNo);
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }

        }

        public async Task<bool> isVerifiedMobile(string mobileNo)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            using (SqlCommand cmd = new SqlCommand("dbo.IsVerifiedMobile", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@MobileNo", SqlDbType.NVarChar, 20).Value = mobileNo;

                await con.OpenAsync();
                object result = await cmd.ExecuteScalarAsync();

                return result != null && Convert.ToInt32(result) == 1;
            }
        }


        public async Task<bool> isVerifiedEmail(string EmailID)
        {

            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
               
                SqlCommand cmd = new SqlCommand("IsVerifiedEmail", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                con.OpenAsync();
                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        //  write a function to check the mailId and password in valid or not
        public bool VerifyEmailPassword(string emailID, string password)
        {
            string passwordHash = password; // use hashed password if applicable

            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            using (SqlCommand cmd = new SqlCommand("dbo.VerifyEmailPassword", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@EmailID", emailID);
                cmd.Parameters.AddWithValue("@PasswordHash",passwordHash);

                con.Open();

                object result = cmd.ExecuteScalar();

                return result != null && Convert.ToInt32(result) == 1;
            }
        }



        public bool IsValidEmailAndMobile(string EmailID, string MobileNo)
        {


            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                
                SqlCommand cmd = new SqlCommand("IsvalidEmailandMobile", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@Mobile", MobileNo);

                con.Open();

                int count = (int)cmd.ExecuteScalar();
                return count > 0;

            }
        }



        // write a function to add login 
        //public void UpdateLoginTime(string EmailID)
        //{
        //    using (SqlConnection con = new SqlConnection(_conn.Dbcs))
        //    {
        //        string UpdateQuery = " UPDATE dbo.Users  SET LastLoginAt = @LastLoginAt WHERE EmailID = @EmailID";
        //        SqlCommand cmd = new SqlCommand(UpdateQuery, con);
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Parameters.AddWithValue("@EmailID", EmailID);
        //        cmd.Parameters.AddWithValue("@LastLoginAt", DateTime.Now);
        //        con.Open();
        //        int id = (int)cmd.ExecuteNonQuery();
        //    }
        //    return;
        //}



        // Write a function to Generate Random String for OTP And Password


        public string GenerateRandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] result = new char[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] buffer = new byte[length];

                rng.GetBytes(buffer);

                for (int i = 0; i < length; i++)
                {
                    result[i] = chars[buffer[i] % chars.Length];
                }
            }

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
                
                SqlCommand cmd = new SqlCommand("PasswordChange", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", EmailID);
                cmd.Parameters.AddWithValue("@password", PasswordHash);
                cmd.Parameters.AddWithValue("@status", 1);

                con.Open();
                int id = cmd.ExecuteNonQuery();
                if(id>0)
                    return "Password Change Successfully ";
            }
            
            return "Invalid Crediatials ";
        }



        // write a functio to check the status of the user 
        public bool UserStatus(string EmailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
               
                SqlCommand cmd = new SqlCommand("dbo.GetUserStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;
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
                SqlCommand cmd = new SqlCommand("SetUserStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@Status", Status);
                con.Open();
                cmd.ExecuteNonQuery();

            }
            return;

        }


        // write a functio to verify Token 

        public DateTime VerifyUser(string token, string emailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            using (SqlCommand cmd = new SqlCommand("dbo.VerifyUser", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Token", SqlDbType.NVarChar, 200).Value = token;
                cmd.Parameters.Add("@EmailID", SqlDbType.NVarChar, 100).Value = emailID;

                con.Open();
                object result = cmd.ExecuteScalar();

                return result == null || result == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(result);
            }
        }



        // write a funciton to generate the Token and link 

        public string UpdateToken(string emailID)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            using (SqlCommand cmd = new SqlCommand("dbo.UpdateToken", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmailID", emailID);

                con.Open();
                object result = cmd.ExecuteScalar();

                return result.ToString();
            }
        }

        // write a function to reset token 
        //public bool resetToken(string EmailID)
        //{
        //    using (SqlConnection con = new SqlConnection(_conn.Dbcs))
        //    {
        //        string UpdateToken = @"UPDATE Users SET token = @token,TokenGeneratedAt = @TokenGeneratedAt, IsUsedToken = 0,EmailVerified=0 WHERE EmailID = @EmailID ";
        //        SqlCommand cmd = new SqlCommand(UpdateToken, con);
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Parameters.AddWithValue("@token", DBNull.Value);
        //        cmd.Parameters.AddWithValue("@TokenGeneratedAt", DBNull.Value);
        //        cmd.Parameters.AddWithValue("@EmailID", EmailID);
        //        con.Open();
        //        int row = cmd.ExecuteNonQuery();
        //        return row > 0;
        //    }
        //}

        // Write a function to verify mobile and Email address of the user ie valid or not
        public bool IsMobileExist(string MobileNo)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
                
                SqlCommand cmd = new SqlCommand("IsExistMobile", con);
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@MobileNo", MobileNo);
                con.Open();

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                return count > 0;

            }
        }

        // write a function to Update Otp 
        public string GetOTP(string MobileNo)
        {
            string OTP = GenerateRandomString(6);

            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
               
                SqlCommand cmd = new SqlCommand("GetOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@OTPCode", OTP);
                cmd.Parameters.AddWithValue("@MobileNo", MobileNo);
                con.Open();
                int row = cmd.ExecuteNonQuery();
                if(row>0)
                return OTP;

            }
            return "Invalid Email and  Mobile";
        }

        // write a function to verify otp 
        public DateTime VerifyOTP(string otpCode, string mobileNo)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            using (SqlCommand cmd = new SqlCommand("VerifyOTP", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@OTPCode", otpCode);
                cmd.Parameters.AddWithValue("@MobileNo", mobileNo);

                con.Open();
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return DateTime.MinValue;

                return Convert.ToDateTime(result);
            }
        }


        public bool SetPassWord(string EmailID, string Password)
        {

            string PasswordHash = ConvertHashPassword(Password);
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            {
               
                SqlCommand cmd = new SqlCommand("SetPassword", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmailID", EmailID);
                cmd.Parameters.AddWithValue("@PasswordHash", PasswordHash);
                cmd.Parameters.AddWithValue("@Status", 0);

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0;

            }

        }

        public async Task<Myprofile> profile(Myprofile myprofile)
        {
            using (SqlConnection con = new SqlConnection(_conn.Dbcs))
            using (SqlCommand cmd = new SqlCommand("GetProfile", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmailID", myprofile.Email);

                await con.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Myprofile
                        {
                            Id = (int)reader.GetInt64(reader.GetOrdinal("UserID")),
                            FName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LName = reader.GetString(reader.GetOrdinal("LastName")),
                            Email = reader.GetString(reader.GetOrdinal("EmailID")),
                            MobileNo = reader.GetString(reader.GetOrdinal("MobileNo")),
                            createdAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                        };
                    }
                }
            }
            return null;
        }
    }
}

