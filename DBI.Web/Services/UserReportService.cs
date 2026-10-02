using Dapper;
using DBI_eDahab.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace DBI_eDahab.Web.Services
{
    public class UserReportService
    {
        private readonly string _connectionString;

        public UserReportService()
        {
            _connectionString =
                ConfigurationManager
                    .ConnectionStrings["DBI"]
                    .ConnectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public List<UserDropdownItem> GetUsersByBranch(
            string branch)
        {
            const string sql = @"
                SELECT
                    Id,
                    UserName,
                    FullName
                FROM dbo.dbi_users
                WHERE Branch = @Branch
                ORDER BY FullName, UserName;";

            using (var connection = GetConnection())
            {
                return connection
                    .Query<UserDropdownItem>(
                        sql,
                        new { Branch = branch })
                    .ToList();
            }
        }

        public List<UserReportItem> GetReport(
            string branch,
            int? userId = null)
        {
            var sql = new StringBuilder();

            sql.Append(@"
                SELECT
                    Id,
                    UserName,
                    FullName,
                    Email,
                    MobileNumber,
                    RegisteredDate,
                    CAST(Branch AS VARCHAR(10)) AS Branch,
                    Active
                FROM dbo.dbi_users
                WHERE 1 = 1
            ");

            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(branch))
            {
                sql.Append(" AND Branch = @Branch ");
                parameters.Add("@Branch", branch);
            }

            if (userId.HasValue)
            {
                sql.Append(" AND Id = @UserId ");
                parameters.Add("@UserId", userId.Value);
            }

            sql.Append(@"
                ORDER BY
                    Branch,
                    FullName,
                    UserName;
            ");

            using (var connection = GetConnection())
            {
                return connection
                    .Query<UserReportItem>(
                        sql.ToString(),
                        parameters)
                    .ToList();
            }
        }

        public List<UserReportItem> GetReportByBranches(
            IEnumerable<string> branches,
            int? userId = null)
        {
            const string sql = @"
                SELECT
                    Id,
                    UserName,
                    FullName,
                    Email,
                    MobileNumber,
                    RegisteredDate,
                    CAST(Branch AS VARCHAR(10)) AS Branch,
                    Active
                FROM dbo.dbi_users
                WHERE Branch IN @Branches
                  AND (@UserId IS NULL OR Id = @UserId)
                ORDER BY
                    Branch,
                    FullName,
                    UserName;";

            using (var connection = GetConnection())
            {
                return connection.Query<UserReportItem>(
                    sql,
                    new
                    {
                        Branches = branches,
                        UserId = userId
                    }).ToList();
            }
        }
    }
}