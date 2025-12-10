using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBI_eDahab.Web.ViewModels;
using System.Data.SqlClient;

namespace DBI_eDahab.Web.Models
{
    public class UsersRepository
    {
        readonly string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DBI"].ConnectionString;
 
        internal void AddUser(Users addUser)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO dbi_users
                                               (FullName,UserName,Email,Password,MobileNumber,RegisteredDate,Branch,Active)
                                         VALUES
                                               (@FullName,@UserName,@Email,@Password,@MobileNumber,GetDate(),@Branch,'True')";
                command.Parameters.AddWithValue("@Branch", addUser.Branch);
                command.Parameters.AddWithValue("@FullName", addUser.FullName);
                command.Parameters.AddWithValue("@UserName", addUser.UserName);
                command.Parameters.AddWithValue("@Email", addUser.Email);
                command.Parameters.AddWithValue("@MobileNumber", addUser.MobileNumber);
                command.Parameters.AddWithValue("@Password", addUser.Password);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        internal void AddBranch(Branchs branch)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO dbi_branchs
                                               (BranchName
                                               ,Date)
                                         VALUES
                                               (@BranchName
                                               ,GetDate()
                                                )";
                command.Parameters.AddWithValue("@BranchName", branch.BranchName);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public Tuple<int, List<Users>> SearchUsers(string searchTerm, int firstRow = 1, int lastRow = 10)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                var sql = @"SELECT COUNT(*)
                            FROM dbi_users
                            WHERE {0}
                            SELECT *
                            FROM (  SELECT ROW_NUMBER() OVER(ORDER BY Id) AS RowNumber, *
                                    FROM dbi_users
                                    WHERE {0}
                                    ) AS t1
                            WHERE t1.RowNumber BETWEEN @FirstRow AND @LastRow";

                var whereClause = @"(UserName LIKE @SearchTerm 
                                        OR FullName LIKE @SearchTerm
                                        OR MobileNumber LIKE @SearchTerm)";


                //if (userType.HasValue)
                //{
                //    whereClause += " AND Type = @UserType";

                //    command.Parameters.AddWithValue("@UserType", userType.Value.ToString());
                //}

                searchTerm = searchTerm.Replace("%", "");
                searchTerm = searchTerm.Replace("_", "");
                searchTerm = searchTerm.Replace("^", "");
                searchTerm = searchTerm.Replace("[", "");
                searchTerm = searchTerm.Replace("]", "");

                command.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                command.CommandText = string.Format(sql, whereClause);

                command.Parameters.AddWithValue("@FirstRow", firstRow);
                command.Parameters.AddWithValue("@LastRow", lastRow);

                connection.Open();

                var reader = command.ExecuteReader();

                var users = new List<Users>();

                reader.Read();

                var count = Convert.ToInt32(reader[0]);

                reader.NextResult();

                while (reader.Read())
                {
                    var user = new Users
                    {
                        Id = (int) reader["Id"],
                        FullName = reader["FullName"] as string,
                        UserName = reader["UserName"] as string,
                        MobileNumber = reader["MobileNumber"] as string,
                        RegisteredDate = (DateTime) reader["RegisteredDate"],
                        Email = reader["Email"] as string,
                        Branch = (int)reader["Branch"]
                    };

                    users.Add(user);
                }

                return Tuple.Create(count, users);
            }
        }


        internal Users GetUserByLogin(string identity)
        {
            Users user = null;

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand { Connection = connection })
            {
                command.CommandType = System.Data.CommandType.Text;

                command.CommandText = "Select top 1 * from dbi_users where UserName=@UserName";

                command.Parameters.AddWithValue("@UserName", identity);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var u = new Users();
                    u.UserName = reader["UserName"] as string;

                    return u;
                };
            }
            return user;
        }


        public void UpdateUser(UpdateUserForm updateUserForm)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE dbi_users SET 
                                        FullName = @FullName,
                                       
                                        
                                        --Password = @Password,
                                        Active = @Active,
                                        Email = @Email,
                                        MobileNumber = @MobileNumber,
                                        CurrentPermissions = @CurrentPermissions                                      
                                        WHERE Id = @Id";

                command.Parameters.AddWithValue("@FullName", updateUserForm.Name);
                command.Parameters.AddWithValue("@CurrentPermissions", updateUserForm.CurrentPermissions);
                command.Parameters.AddWithValue("@Active", updateUserForm.Active);
                command.Parameters.AddWithValue("@Email", updateUserForm.Email);
                command.Parameters.AddWithValue("@MobileNumber", updateUserForm.MobileNumber);
                command.Parameters.AddWithValue("@Id", updateUserForm.Id);

                connection.Open();

                command.ExecuteNonQuery();
            }

        }


        internal void ChangePassword(string username, string hashedNewPassword)
        {
            var currentUser = HttpContext.Current.Session["User"] as Users;
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE dbi_users SET
                                        Password = @Password
                                        WHERE UserName = @UserName
                                      ";
                command.Parameters.AddWithValue("@UserName", username);
                command.Parameters.AddWithValue("@Password", hashedNewPassword);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }




        public Users GetUser(string userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM dbi_users WHERE UserName = @UserName ";
                command.Parameters.AddWithValue("@UserName", userId);
                connection.Open();
                var reader = command.ExecuteReader();
                Users user = null;

                if (reader.Read())
                {
                    user = new Users();

                    user.Id = (int)reader["Id"];
                    user.FullName = reader["FullName"] as string;
                    user.UserName = reader["UserName"] as string;
                    if (reader["CurrentPermissions"] != DBNull.Value)
                    {
                        user.CurrentPermissions = (Users.Permissions)reader["CurrentPermissions"];
                    }
                    user.MobileNumber = reader["MobileNumber"] as string;
                    user.Email = reader["Email"] as string;
                    user.Branch =(int)reader["Branch"];
                    user.Active = (bool)reader["Active"];

                }

                return user;
            }
        }



        public Users AuthenticateUser(string loginId, string hashedPassword)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT * 
                                        FROM dbi_users
                                        WHERE UserName =@UserName 
                                        and Password = @Password;

                                       ";

                command.Parameters.AddWithValue("@UserName", loginId);
                command.Parameters.AddWithValue("@Password", hashedPassword);

                connection.Open();

                var reader = command.ExecuteReader();

                Users user = null;

                if (reader.Read())
                {
                    user = new Users();
                    user.Id = Convert.ToInt32(reader["Id"]);
                    user.FullName = reader["FullName"] as string;
                    user.UserName = reader["UserName"] as string;
                    user.MobileNumber = reader["MobileNumber"] as string;
                    user.Password = reader["Password"] as string;
                    user.Branch = Convert.ToInt32( reader["Branch"] );
                    user.CurrentPermissions = (Users.Permissions)reader["CurrentPermissions"];



                }

                return user;
            }
        }

        internal void LogUserAction(AuditLog auditLogRecord)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO AuditLog
                                               (UserName, ActivityType, ActivityTime, Description, AffectedParty)
                                         VALUES
                                               (@UserName,@ActivityType,GetDate(),@Description, @AffectedParty)";
                command.Parameters.AddWithValue("@UserName", auditLogRecord.UserName);
                command.Parameters.AddWithValue("@ActivityType", auditLogRecord.ActivityType); 
                command.Parameters.AddWithValue("@Description", auditLogRecord.Description);
                command.Parameters.AddWithValue("@AffectedParty", auditLogRecord.AffectedParty);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }


    }
}