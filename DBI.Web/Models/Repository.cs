using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using DBI_eDahab.Web.ViewModels;
using Oracle.ManagedDataAccess.Client;

namespace DBI_eDahab.Web.Models
{
    public class Repository
    {
        readonly string _connectionString = ConfigurationManager.ConnectionStrings["DBI"].ConnectionString;
        readonly string _hqBranch = ConfigurationManager.AppSettings["HQBranch"];

        internal void RegisterCustomer(CustomerForm customer)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO DahabCard
                                                   (MSISDN, eDahabName, eDahabType, CreatedOn, PIN, CreatedBy, Active, Verified,Remarks)
                                             VALUES (@MSISDN, @eDahabName, @eDahabType, GetDate(), @PIN, @CreatedBy, 'Y', 0, @Remarks)";
                string userName = HttpContext.Current.User.Identity.Name;
                command.Parameters.AddWithValue("@MSISDN", customer.MSISDN);
                command.Parameters.AddWithValue("@eDahabName", customer.eDahabName);
                command.Parameters.AddWithValue("@eDahabType", customer.eDahabType);
                command.Parameters.AddWithValue("@PIN", customer.PIN);
                command.Parameters.AddWithValue("@CreatedBy", userName);
                command.Parameters.AddWithValue("@Remarks", userName + ": New entry.");
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        internal int RegisterCustomerAccount(CustomerAccountForm customer)
        {
            int result = 0;
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT INTO dbi_Customers
                                        (MSISDN, AccountNo, AccountType, Currency, NewDailyLimit, Branch, eDahabType, AccountHolder, CreatedBy,DailyLimit, CreatedOn, eDahabName, Remarks)
                                        VALUES (@MSISDN, @AccountNo, @AccountType, @Currency, @NewDailyLimit, @Branch, @eDahabType ,@AccountHolder, @CreatedBy,@NewDailyLimit,GetDate(), @eDahabName, @Remarks)";
                string userName = HttpContext.Current.User.Identity.Name;
                command.Parameters.AddWithValue("@MSISDN", customer.MSISDN);
                command.Parameters.AddWithValue("@AccountNo", customer.AccountNo);
                command.Parameters.AddWithValue("@AccountType", customer.AccountType);
                command.Parameters.AddWithValue("@Currency", customer.Currency);
                command.Parameters.AddWithValue("@DailyLimit", customer.NewDailyLimit);
                command.Parameters.AddWithValue("@NewDailyLimit", customer.NewDailyLimit);
                command.Parameters.AddWithValue("@Branch", customer.Branch);
                command.Parameters.AddWithValue("@eDahabType", customer.eDahabType);
                command.Parameters.AddWithValue("@AccountHolder", customer.AccountHolder);
                command.Parameters.AddWithValue("@eDahabName", customer.eDahabName);
                command.Parameters.AddWithValue("@CreatedBy", userName);
                command.Parameters.AddWithValue("@Remarks", userName + ": New entry.");
                try
                {
                    connection.Open();
                    result = command.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    if (e.Message.Contains("KEY constraint"))
                        result = -2;
                }
                catch(Exception)
                {
                    result = -1;
                }
            }
            return result;
        }

        internal void UpdateCustomer(CustomerForm customer, string Active)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE DahabCard
                                               SET Active= @Active, ModifiedBy= @ModifiedBy, ModifiedOn = GetDate(), Remarks = @Remarks 
                                             WHERE MSISDN = @MSISDN";
                string userName = HttpContext.Current.User.Identity.Name;
                command.Parameters.AddWithValue("@ModifiedBy", userName);
                command.Parameters.AddWithValue("@MSISDN", customer.MSISDN);

                command.Parameters.AddWithValue("@Remarks", userName + ": " + customer.Remarks);
                command.Parameters.AddWithValue("@Active", customer.Active);
                connection.Open();
                if (!(Active.Equals(customer.Active)))
                    command.ExecuteNonQuery();
                command.Parameters.Clear();
                if (customer.CustomerAccount.NewDailyLimit != customer.CustomerAccount.DailyLimit && customer.CustomerAccount.NewDailyLimit > 0)
                {
                    command.CommandText = @"UPDATE dbi_Customers
                                                   SET Verified= 0, NewDailyLimit = @NewDailyLimit, ModifiedBy= @ModifiedBy, ModifiedOn = GetDate(), Remarks = @Remarks 
                                                 WHERE AccountNo = @AccountNo and AccountType = @AccountType";
                    command.Parameters.AddWithValue("@AccountNo", customer.CustomerAccount.AccountNo);
                    command.Parameters.AddWithValue("@AccountType", customer.CustomerAccount.AccountType);
                    command.Parameters.AddWithValue("@ModifiedBy", userName);
                    command.Parameters.AddWithValue("@NewDailyLimit", customer.CustomerAccount.NewDailyLimit);
                    command.Parameters.AddWithValue("@Remarks", userName + ": " + customer.CustomerAccount.Remarks);

                    command.ExecuteNonQuery();
                }
            }
        }


        //1- I need to archive the entry before deletion
        //2- I need to save the remarks for deletion
        internal void DeleteCustomer(CustomerForm customer)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"delete from dbi_Customers
                                             WHERE AccountNo = @AccountNo and AccountType = @AccountType and Branch = @Branch";
                string userName = HttpContext.Current.User.Identity.Name;
                command.Parameters.AddWithValue("@AccountNo", customer.CustomerAccount.AccountNo);
                command.Parameters.AddWithValue("@AccountType", customer.CustomerAccount.AccountType);
                command.Parameters.AddWithValue("@Remarks", userName + ": " + customer.Remarks);
                command.Parameters.AddWithValue("@Branch", customer.CustomerAccount.Branch);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }



        internal void VerifyCustomer(string AccountNo, string AccountType)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE dbi_Customers
                                               SET Verified = 1, VerifiedBy = @VerifiedBy, VerifiedOn = @VerifiedOn, DailyLimit = NewDailyLimit  
                                             WHERE AccountNo = @AccountNo and AccountType = @AccountType";
                string userName = HttpContext.Current.User.Identity.Name;
                command.Parameters.AddWithValue("@AccountNo", AccountNo);
                command.Parameters.AddWithValue("@VerifiedBy", userName);
                command.Parameters.AddWithValue("@VerifiedOn", DateTime.Now);
                command.Parameters.AddWithValue("@AccountType", AccountType);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public CustomerForm CheckIfMSISDNExists(string MSISDN)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT * FROM DahabCard WHERE MSISDN = @MSISDN";
                command.Parameters.AddWithValue("@MSISDN", MSISDN);
                if(connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                var reader = command.ExecuteReader();
                CustomerForm customer = null;
                if (reader.Read())
                {
                    customer = new CustomerForm();
                    customer.MSISDN = reader["MSISDN"] as string;
                    customer.Active = reader["Active"] as string;
                    customer.eDahabName = reader["eDahabName"] as string;
                }
                return customer;
            }
        }

        public CustomerAccountForm CheckIfAccountExists(string AccountNo)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT * FROM dbi_Customers WHERE AccountNo = @AccountNo";
                command.Parameters.AddWithValue("@AccountNo", AccountNo);
                connection.Open();
                var reader = command.ExecuteReader();
                CustomerAccountForm customer = null;
                if (reader.Read())
                {
                    customer = new CustomerAccountForm();
                    customer.MSISDN = reader["MSISDN"] as string;
                    customer.AccountNo = reader["AccountNo"] as string;
                }
                return customer;
            }
        }

        public CustomerForm GetAccountInfo(string MSISDN, string AccountNo, string AccountType)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"select d.MSISDN MSISDN, d.eDahabName eDahabName, d.Active Active, d.eDahabType eDahabType, isnull(c.Verified,0) Verified
                                      ,   c.AccountHolder AccountHolder, c.CreatedBy CreatedBy, c.AccountNo AccountNo, c.Currency,
                                      c.AccountHolder AccountHolder, c.AccountType AccountType, isnull(c.DailyLimit,0) DailyLimit, 
                                        isnull(c.NewDailyLimit,0) NewDailyLimit, c.CreatedOn CreatedOn, c.Remarks AccountRemarks, d.Remarks Remarks 
                                      from DahabCard d, dbi_Customers c
                                      --left join  dbi_Customers c on d.MSISDN = c.MSISDN
                                      WHERE (d.MSISDN = c.MSISDN and d.MSISDN = @MSISDN and c.AccountNo = @AccountNo) ";

                command.Parameters.AddWithValue("@MSISDN", MSISDN);
                command.Parameters.AddWithValue("@AccountNo", AccountNo);
                command.Parameters.AddWithValue("@AccountType", AccountType);
                connection.Open();

                var reader = command.ExecuteReader();

                CustomerForm customer = null;

                if (reader.Read())
                {
                    customer = new CustomerForm();
                    customer.MSISDN = reader["MSISDN"] as string;
                    customer.eDahabName = reader["edahabName"] as string;
                    customer.eDahabType = reader["eDahabType"] as string;
                    customer.Active = reader["Active"] as string;
                    customer.CustomerAccount.Verified = (bool)reader["Verified"];
                    customer.CustomerAccount.AccountNo = reader["AccountNo"] as string;
                    customer.CustomerAccount.AccountType = reader["AccountType"] as string;
                    customer.CustomerAccount.AccountHolder = reader["AccountHolder"] as string;
                    customer.CustomerAccount.CreatedOn = (DateTime)reader["CreatedOn"];
                    customer.CustomerAccount.CreatedBy = reader["CreatedBy"] as string;
                    customer.CustomerAccount.Currency = reader["Currency"] as string;
                    customer.CustomerAccount.DailyLimit = Convert.ToDecimal(reader["DailyLimit"].ToString());
                    customer.CustomerAccount.NewDailyLimit = Convert.ToDecimal(reader["NewDailyLimit"].ToString());
                    customer.CustomerAccount.Remarks = reader["AccountRemarks"] as string;
                    customer.Remarks = reader["Remarks"] as string;
                }

                return customer;
            }
        }

        internal void ChangePin(string MSISDN, string hashedNewPassword)
        {
            var currentUser = HttpContext.Current.Session["User"] as Users;
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE DahabCard SET
                                        Pin = @PIN, ModifiedBy = @ModifiedBy, ModifiedOn = GetDate() 
                                        WHERE MSISDN = @MSISDN
                                      ";
                string userName = HttpContext.Current.User.Identity.Name;
                command.Parameters.AddWithValue("@MSISDN", MSISDN);
                command.Parameters.AddWithValue("@ModifiedBy", userName);
                command.Parameters.AddWithValue("@PIN", hashedNewPassword);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<CustomerForm> GetCustomers(string Term, DateTime? DateFrom = null, DateTime? DateTo = null, string Currency=null, string Branch=null, string Active = "", bool? Verified = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"select d.MSISDN, d.eDahabName, d.Active, d.eDahabType, isnull(c.Verified,0) Verified, c.AccountHolder,
                                      c.CreatedBy, c.AccountNo,  c.Currency,
                                      c.AccountHolder, c.AccountType, isnull(c.DailyLimit,0) DailyLimit, c.CreatedOn 
                                      from DahabCard d, dbi_Customers c
                                      --left join  dbi_Customers c on d.MSISDN = c.MSISDN
                                      WHERE (d.MSISDN = c.MSISDN and (d.MSISDN LIKE @MSISDN or c.AccountNo like @AccountNo) ) and c.Currency like @Currency and c.Branch like @Branch 
                                      and d.Active like @Active and (c.Verified = @Verified1 or c.Verified = @Verified2) AND d.eDahabType != 'AGNT'";



                if (DateFrom.HasValue && DateTo.HasValue)
                {
                    command.CommandText += "and d.CreatedOn between @DateFrom and @DateTo ";
                    command.Parameters.AddWithValue("@DateFrom", DateFrom.Value);
                    command.Parameters.AddWithValue("@DateTo", DateTo.Value.AddDays(1).AddSeconds(-1));
                }
                command.CommandText += " ORDER BY CreatedOn DESC    ";
                string branch = new UsersRepository().GetUser(HttpContext.Current.User.Identity.Name.ToString()).Branch.ToString();
                if (branch == _hqBranch)
                {
                    branch = Branch;
                }
                command.Parameters.AddWithValue("@Branch", branch + "%%");
                command.Parameters.AddWithValue("@MSISDN", Term + "%%");
                command.Parameters.AddWithValue("@AccountNo", Term + "%%");
                command.Parameters.AddWithValue("@Currency", Currency + "%%");
                command.Parameters.AddWithValue("@Active", Active + "%%");
                if (Verified == null)
                {
                    command.Parameters.AddWithValue("@Verified1", true);
                    command.Parameters.AddWithValue("@Verified2", false);
                }
                else
                {
                    command.Parameters.AddWithValue("@Verified1", Verified);
                    command.Parameters.AddWithValue("@Verified2", Verified);
                }
                connection.Open();

                var reader = command.ExecuteReader();

                List<CustomerForm> customers = new List<CustomerForm>();

                while (reader.Read())
                {
                    var customer = new CustomerForm();
                    customer.MSISDN = reader["MSISDN"] as string;
                    customer.eDahabName = reader["edahabName"] as string;
                    customer.Active = reader["Active"] as string;
                    customer.CustomerAccount.AccountHolder = reader["AccountHolder"] as string;
                    customer.CustomerAccount.DailyLimit = Convert.ToDecimal(reader["DailyLimit"].ToString());
                    customer.CustomerAccount.AccountNo = reader["AccountNo"] as string;
                    customer.CustomerAccount.AccountType = reader["AccountType"] as string;
                    customer.CustomerAccount.Currency = reader["Currency"] as string;
                    customer.CustomerAccount.Verified = (reader["Verified"] as bool?).GetValueOrDefault();
                    customer.CustomerAccount.CreatedOn = (DateTime)reader["CreatedOn"];
                    customer.CustomerAccount.CreatedBy = reader["CreatedBy"] as string;
                    customers.Add(customer);
                }
                return customers;
            }
        }

        public List<Transaction> Transactions(FilterTransactions filter)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT t.MSISDN MSISDN, t.AccountId AccountId, t.AccountType AccountType, t.Currency Currency, t.Branch Branch
                                      ,  t.Amount Amount, t.Dated Dated, t.TransactionType TransactionType, t.Narration Narration, t.Status Status
                                       FROM Transactions t
                                       WHERE  
                                       (t.MSISDN LIKE @MSISDN OR AccountId LIKE @AccountNo )
                                       AND (Status LIKE @Status) AND (t.Currency LIKE @Currency) 
                                       and t.Amount like @Amount  and t.Branch like @Branch ";

                if (filter.DateFrom.HasValue && filter.DateTo.HasValue)
                {
                    command.CommandText += "and Dated between @DateFrom and @DateTo ";
                    command.Parameters.AddWithValue("@DateFrom", filter.DateFrom.Value);
                    command.Parameters.AddWithValue("@DateTo", filter.DateTo.Value.AddDays(1).AddSeconds(-1));
                }
                command.CommandText += " ORDER BY Dated DESC    ";
                command.Parameters.AddWithValue("@MSISDN", filter.term + "%%");
                command.Parameters.AddWithValue("@AccountNo", filter.term + "%%");
                command.Parameters.AddWithValue("@Status", filter.Status + "%%");
                command.Parameters.AddWithValue("@Currency", filter.Currency + "%%");
                command.Parameters.AddWithValue("@Amount", "%%" + filter.Amount);
                string branch = new UsersRepository().GetUser("ibrahim").Branch.ToString();
                if (branch == _hqBranch)
                {
                    branch = filter.Branch;
                }
                command.Parameters.AddWithValue("@Branch", branch + "%%");
                

                connection.Open();
                var reader = command.ExecuteReader();

                var transactions = new List<Transaction>();

                while (reader.Read())
                {
                    var transaction = new Transaction();
                    transaction.MSISDN = reader["MSISDN"] as string;
                    transaction.AccountId = reader["AccountId"] as string;
                    transaction.AccountType = reader["AccountType"] as string;
                    transaction.Currency = reader["Currency"] as string;
                    transaction.Branch = reader["Branch"] as string;
                    transaction.Amount = (decimal)reader["Amount"];
                    transaction.Dated = (DateTime)reader["Dated"];
                    transaction.TransactionType = reader["TransactionType"] as string;
                    transaction.Narration = reader["Narration"] as string;
                    transaction.Status = (bool)reader["Status"];
                    transactions.Add(transaction);
                }
                return transactions;
            }
        }

    }
}