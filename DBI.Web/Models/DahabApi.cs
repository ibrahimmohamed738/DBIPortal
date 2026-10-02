using DBI_eDahab.Web.ViewModels;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace DBI_eDahab.Web.Models
{
    public class DahabApi
    {
        private string _token = "";

        public async Task<string> GetToken()
        {
            if (!string.IsNullOrEmpty(_token))
            {
                return _token;
            }

            var request = new
            {
                username = "dbiuser",
                password = "dbiuser@#$%2022"
            };
            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["EDahabLoacalAPI"]);
                var response = await _client.PostAsJsonAsync("Auth/login", request);
                if (!response.IsSuccessStatusCode)
                {
                    return "";
                }

                var result = await response.Content.ReadAsAsync<DahabResponse<string>>();
                if (result is null)
                {
                    return "";
                }

                _token = result.Data;
                return _token;
            }
        }


        private HttpClient CreatePayxClient()
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri(
                ConfigurationManager.AppSettings["PayxApiUrl"]);

            client.DefaultRequestHeaders.TryAddWithoutValidation(
                "X-API-KEY",
                ConfigurationManager.AppSettings["PayxApiKey"]);

            client.DefaultRequestHeaders.TryAddWithoutValidation(
                "X-API-SECRET",
                ConfigurationManager.AppSettings["PayxApiSecret"]);

            client.DefaultRequestHeaders.TryAddWithoutValidation(
                "Accept",
                "*/*");

            return client;
        }

        public async Task<DahabBalanceResponse> GetAgentBalance(string Msisdn)
        {
            using (var client = CreatePayxClient())
            {
                try
                {
                    var response = await client.GetAsync(
                        "edahab/" + Msisdn + "/balance");

                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content
                            .ReadAsAsync<PayxErrorResponse>();

                        return null;
                    }

                    var result = await response.Content
                        .ReadAsAsync<PayxBalanceResponse>();

                    if (result == null || result.Balances == null)
                    {
                        return null;
                    }

                    var dollar = result.Balances
                        .FirstOrDefault(x =>
                            string.Equals(
                                x.Currency,
                                "dollar",
                                StringComparison.OrdinalIgnoreCase));

                    var shilling = result.Balances
                        .FirstOrDefault(x =>
                            string.Equals(
                                x.Currency,
                                "shilling",
                                StringComparison.OrdinalIgnoreCase));

                    return new DahabBalanceResponse
                    {
                        USDBalance = dollar != null
                            ? dollar.Balance
                            : 0,

                        SLSBalance = shilling != null
                            ? shilling.Balance
                            : 0
                    };
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public async Task<UserInfo> GetUserInfo(string Msisdn)
        {
            using (var client = CreatePayxClient())
            {
                var response = await client.GetAsync(
                    "edahab/" + Msisdn);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var result = await response.Content
                    .ReadAsAsync<PayxUserResponse>();

                if (result == null)
                {
                    return null;
                }

                return new UserInfo
                {
                    UserId = result.Id,
                    Msisdn = result.Phone,
                    FullName = result.FullName,
                    CategoryCode = result.Category,
                    Gender = result.Gender,
                    Status = result.Status,
                    AgentCode = result.Code
                };
            }
        }

        public async Task<byte[]> GetSubscriberPhoto(string Msisdn)
        {
            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["EDahabLoacalAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.GetAsync($"EDahab/GetUserPhoto/{Msisdn}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var result = await response.Content.ReadAsAsync<DahabResponse<byte[]>>();
                if (result is null)
                {
                    return null;
                }
                return result.Data;
            }
        }

        public async Task<CashResponse> SubscriberCashInAsync(CashinRequest request)
        {
            using (var client = CreatePayxClient())
            {
                var body = new
                {
                    receiverPhone = request.Phone,
                    transactorPhone = request.AgentLongCode,
                    amount = request.Amount,
                    currency = request.Currency,
                    externalReferenceId = request.TransactionId
                };

                var response = await client.PostAsJsonAsync(
                    "edahab/imt-cashin",
                    body);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content
                        .ReadAsAsync<PayxErrorResponse>();

                    return new CashResponse
                    {
                        StatusCode = ((int)response.StatusCode).ToString(),
                        Message = error != null
                            ? error.Message
                            : "Transaction failed"
                    };
                }

                var result = await response.Content
                    .ReadAsAsync<PayxCashInResponse>();

                if (result == null)
                {
                    return new CashResponse
                    {
                        StatusCode = "500",
                        Message = "Invalid response"
                    };
                }

                return new CashResponse
                {
                    StatusCode = result.Status == "SUCCEEDED"
                        ? "200"
                        : "400",

                    Message = result.Message,
                    TransactionId = result.TransactionId
                };
            }
        }

        public async Task<CashResponse> MerchantInAsync(
     CashinRequest request)
        {
            using (var client = CreatePayxClient())
            {
                var body = new
                {
                    senderPhone = request.AgentLongCode,
                    receiverPhone = request.Phone,
                    amount = request.Amount,
                    currency = request.Currency,
                    remarks = "DBI"
                };

                var response = await client.PostAsJsonAsync(
                    "edahab/c2c-without-pin",
                    body);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content
                        .ReadAsAsync<PayxErrorResponse>();

                    return new CashResponse
                    {
                        StatusCode = ((int)response.StatusCode).ToString(),

                        Message = error != null
                            ? error.Message
                            : "Transaction failed"
                    };
                }

                var result = await response.Content
                    .ReadAsAsync<PayxTransactionResponse>();

                if (result == null)
                {
                    return new CashResponse
                    {
                        StatusCode = "500",
                        Message = "Invalid response"
                    };
                }

                return new CashResponse
                {
                    StatusCode = result.StatusCode.ToString(),
                    Message = result.Message,
                    TransactionId = result.TransactionId
                };
            }
        }

        public async Task<DBITransaction> GetDBITransaction(string transactionId,string userPhone)
        {
            using (var client = CreatePayxClient())
            {
                var body = new
                {
                    transactionId = transactionId,
                    userPhone = userPhone
                };

                var response = await client.PostAsJsonAsync(
                    "edahab/transaction-details",
                    body);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var result = await response.Content
                    .ReadAsAsync<PayxTransactionDetailsResponse>();

                if (result == null)
                {
                    return null;
                }

                decimal amount = 0;

                decimal.TryParse(
                    result.TransferValue,
                    out amount);

                return new DBITransaction
                {
                    TransferId = result.TransactionId,
                    Amount = amount,
                    CELLID = result.Receiver != null
                        ? result.Receiver.MobileNumber
                        : "",

                    FTXNID = result.ServiceRequestId
                };
            }
        }
    }
}