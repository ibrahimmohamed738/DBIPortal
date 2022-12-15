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

        public async Task<DahabBalanceResponse> GetAgentBalance(string Msisdn)
        {
            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["EDahabLoacalAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.GetAsync($"EDahab/GetAgentBalance/{Msisdn}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var result = await response.Content.ReadAsAsync<DahabResponse<DahabBalanceResponse>>();
                if (result is null)
                {
                    return null;
                }
                var balanceResponse = new DahabBalanceResponse
                {
                    USDBalance = result.Data.USDBalance,
                    SLSBalance = result.Data.SLSBalance

                };
                return balanceResponse;
            }
        }

        public async Task<UserInfo> GetUserInfo(string Msisdn)
        {
            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["EDahabLoacalAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.GetAsync($"EDahab/GetUserInfo/{Msisdn}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var result = await response.Content.ReadAsAsync<DahabResponse<UserInfo>>();
                if (result is null)
                {
                    return null;
                }
                var user = new UserInfo
                {
                    UserId = result.Data.UserId,
                    Msisdn = result.Data.Msisdn,
                    FullName = result.Data.FullName,
                    CategoryCode = result.Data.CategoryCode,
                    Gender = result.Data.Gender,
                    Status = result.Data.Status
                };
                return user;
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
            var responseObj = new CashResponse();
               var xmlFormat = $@"<COMMAND>
                                        <TYPE>IMTCIREQ</TYPE>
                                        <TRANSACTIONID>{request.TransactionId}</TRANSACTIONID>  
                                        <SENDONLINEID>{request.TransactionId}</SENDONLINEID>  
                                        <MSISDN>{request.AgentLongCode}</MSISDN>
                                        <SENPROVID>{request.Currency}</SENPROVID>
                                        <RECPROVID>{request.Currency}</RECPROVID>
                                        <SENPAYID>12</SENPAYID>
                                        <RECPAYID>12</RECPAYID>
                                        <MPIN></MPIN>
                                        <MSISDN2>{request.Phone}</MSISDN2>   
                                        <AMOUNT>{request.Amount}</AMOUNT>
                                        <BLOCKSMS></BLOCKSMS>
                                        <CELLID>{request.Phone}</CELLID>
                                        <FTXNID>{request.GTransactionId}</FTXNID>
                                  </COMMAND>";
            var xml = string.Format(xmlFormat);
            var address = ConfigurationManager.AppSettings["ComvivaApiEndpointWeb"].ToString();
            var client = new HttpClient();
            var data = Encoding.UTF8.GetBytes("requestText=" + xml);
            var body = new ByteArrayContent(data);
            var response = await client.PostAsync(address, body);
            if (!response.IsSuccessStatusCode)
            {
                return responseObj;
            }
            var resText = await response.Content.ReadAsStringAsync();
            var resXml = XDocument.Parse(resText);
            var commandElement = resXml.Elements().SingleOrDefault(e => e.Name.LocalName.Equals("COMMAND"));
            var statusElement = commandElement.Elements().SingleOrDefault(e => e.Name.LocalName.Equals("TXNSTATUS"));
            var messageElement = commandElement.Elements().SingleOrDefault(e => e.Name.LocalName.Equals("MESSAGE"));
            var transactionIdElement = commandElement.Elements().SingleOrDefault(e => e.Name.LocalName.Equals("TXNID"));

            responseObj.StatusCode = statusElement.Value;
            responseObj.Message = messageElement.Value;

            if (responseObj.StatusCode.Equals("200"))
            {
                responseObj.TransactionId = transactionIdElement.Value;
            }
           
            return responseObj;
        }

        public async Task<CashResponse> MerchantInAsync(CashinRequest request)
        {
            var responseObj = new CashResponse();
            var xmlFormat = $@"<COMMAND>
                                    <TYPE>RTMREQ</TYPE>
                                    <MSISDN>{request.AgentLongCode}</MSISDN>
                                    <MSISDN2>{request.Phone}</MSISDN2>
                                    <AMOUNT>{request.Amount}</AMOUNT>
                                    <MPIN></MPIN>
                                    <PIN></PIN>
                                    <PROVIDER>{request.Currency}</PROVIDER>
                                    <PROVIDER2>{request.Currency}</PROVIDER2>
                                    <PAYID>12</PAYID>
                                    <PAYID2>12</PAYID2>
                                    <AGENT_CODE></AGENT_CODE>
                                    <MERCHANT_CODE></MERCHANT_CODE>
                                    <BLOCKSMS></BLOCKSMS>
                                    <TXNMODE>eDahabDBI</TXNMODE>
                                    <LANGUAGE1>2</LANGUAGE1>
                                    <LANGUAGE2>2</LANGUAGE2>
                                    <CELLID>{request.Phone}</CELLID>
                                    <FTXNID>{request.TransactionId}</FTXNID> 
                                    </COMMAND>";

            var xml = string.Format(xmlFormat);
            var address = ConfigurationManager.AppSettings["ComvivaApiEndpointWeb"].ToString();

            var client = new HttpClient();

            var data = Encoding.UTF8.GetBytes("requestText=" + xml);
            var body = new ByteArrayContent(data);

            var response = await client.PostAsync(address, body);
            if (!response.IsSuccessStatusCode)
            {
                return responseObj;
            }

            var resText = await response.Content.ReadAsStringAsync();

            var resXml = XDocument.Parse(resText);
            var commandElement = resXml.Elements().SingleOrDefault(e => e.Name.LocalName.Equals("COMMAND"));
            var statusElement = commandElement.Elements().SingleOrDefault(e => e.Name.LocalName.Equals("TXNSTATUS"));
            var messageElement = commandElement.Elements().SingleOrDefault(e => e.Name.LocalName.Equals("MESSAGE"));
            var transactionIdElement = commandElement.Elements().SingleOrDefault(e => e.Name.LocalName.Equals("TXNID"));

            responseObj.StatusCode = statusElement?.Value ?? "";
            responseObj.Message = messageElement?.Value;

            if (responseObj.StatusCode.Equals("200"))
            {
                responseObj.TransactionId = transactionIdElement.Value;
            }
            return responseObj;
        }

        public async Task<DBITransaction> GetDBITransaction(string transactionId)
        {
            using (var connection = new OracleConnection(ConfigurationManager.ConnectionStrings["oracleConnection"].ConnectionString))
            {
                string sql = @"SELECT TRANSFER_ID,(TRANSFER_VALUE / 100) AS AMOUNT,CELL_ID,FTXN_ID from mtx_transaction_header where TRANSFER_STATUS='TS' AND REFERENCE_NUMBER = :transId ";
                OracleCommand cmd = new OracleCommand(sql, connection);
                cmd.Parameters.Add(new OracleParameter(":transId", transactionId));
                cmd.CommandType = CommandType.Text;
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();

                OracleDataReader dr = cmd.ExecuteReader();
                dr.Read();
                if (dr.HasRows)
                {
                    return new DBITransaction
                    {
                        TransferId = dr.GetString(0),
                        Amount = dr.GetDecimal(1),
                        CELLID = dr.GetString(2),
                        FTXNID = dr.GetString(3)
                    };
                }

                else
                    return null;

            }   
        }
    }
}