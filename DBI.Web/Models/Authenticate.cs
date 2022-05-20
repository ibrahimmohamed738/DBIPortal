using System.Data.SqlClient;
using DBI_eDahab.Web.ViewModels;

namespace DBI_eDahab.Web.Models
{
    public class Authenticate
    {
        private UsersRepository _usersRepository;

        public Authenticate(UsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public Users AuthenticateUser(string loginId, string hashedPassword)
        {
            using (var connection = new SqlConnection(_usersRepository._connectionString))
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
                    user.Id = (int)reader["Id"];
                    user.FullName = reader["FullName"] as string;
                    user.UserName = reader["UserName"] as string;
                    user.MobileNumber = reader["MobileNumber"] as string;
                    user.Password = reader["Password"] as string;
                    user.CurrentPermissions = (Users.Permissions)reader["CurrentPermissions"];



                }

                return user;
            }
        }
    }
}