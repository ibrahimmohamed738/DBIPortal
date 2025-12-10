using DBI_eDahab.Web.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DBI_eDahab.Web.Models
{
    public class MobileBankAPI
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
                username = "dbiportaluser",
                password = "dbiportaluser@2032$"
            };
            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
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

        public async Task<GetBankAccount> GetCustomerAccount(string Msisdn)
        {
            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.GetAsync($"DahabBank/get-customer-account/{Msisdn}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var result = await response.Content.ReadAsAsync<DahabResponse<GetBankAccount>>();
                if (result is null)
                {
                    return null;
                }
                var account = new GetBankAccount
                {
                    MSISDN = result.Data.MSISDN,
                    EDahabName = result.Data.EDahabName,
                    EDahabType = result.Data.EDahabType,
                    PIN = result.Data.PIN,
                    CreatedOn = result.Data.CreatedOn,
                    CreatedBy = result.Data.CreatedBy,
                    Active = result.Data.Active,
                    ModifiedBy = result.Data.ModifiedBy,
                    ModifiedOn = result.Data.ModifiedOn,
                    Remarks = result.Data.Remarks,
                    Email = result.Data.Email,
                    AgentCode = result.Data.AgentCode
                };
                return account;
            }
        }

        public async Task<DahabResponse<bool>> CreateAccount(CreateBankAccount createRequest)
        {
         
            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.PostAsJsonAsync("DahabBank/register-customer", createRequest);
                var result = await response.Content.ReadAsAsync<DahabResponse<bool>>();
                return result;
            }
            
        }

        public async Task<GetCustomerAccount> GetByAccountByMsisdn(string msisdn, string accountNo)
        {

            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.GetAsync($"DahabBank/get-account/{msisdn}/{accountNo}");
                var result = await response.Content.ReadAsAsync<DahabResponse<GetCustomerAccount>>();
                if (result == null)
                    return null;

                return result.Data;
            }

        }

        public async Task<DahabResponse<bool>> LinkAccount(LinkAccount linkRequest)
        {

            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.PostAsJsonAsync("DahabBank/link-account", linkRequest);
                var result = await response.Content.ReadAsAsync<DahabResponse<bool>>();
                return result;
            }

        }

        public async Task<DahabResponse<int>> UpdateAccount(ModifyRequest request)
        {

            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
   
                var response = await _client.PostAsJsonAsync("DahabBank/update-account", request);
                var result = await response.Content.ReadAsAsync<DahabResponse<int>>();
                return result;
            }

        }


        public async Task<DahabResponse<int>> UpdateAccountLimit(UpdateLimitRequest request)
        {

            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);

                var response = await _client.PostAsJsonAsync("DahabBank/update-account-limit", request);
                var result = await response.Content.ReadAsAsync<DahabResponse<int>>();
                return result;
            }

        }

        public async Task<DahabResponse<int>> UpdateAccountStatus(UpdateStatusRequest request)
        {

            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);

                var response = await _client.PostAsJsonAsync("DahabBank/update-account-status", request);
                var result = await response.Content.ReadAsAsync<DahabResponse<int>>();
                return result;
            }

        }

        public async Task<DahabResponse<bool>> VerifyAccount(VerifyCustomer verifyRequest)
        {

            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.PostAsJsonAsync("DahabBank/verify-customer", verifyRequest);
                var result = await response.Content.ReadAsAsync<DahabResponse<bool>>();
                return result;
            }

        }


        public async Task<DahabResponse<bool>> DeleteAccount(string accountNo)
        {

            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.PostAsJsonAsync($"DahabBank/delete-customer-account/{accountNo}","");
                var result = await response.Content.ReadAsAsync<DahabResponse<bool>>();
                return result;
            }

        }

        public async Task<DahabResponse<GetCustomerAccountResponse>> GetCustomers(int page,string term,string branch)
        {
            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.GetAsync($"DahabBank/get-customers?page={page}&term={term}&branch={branch}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var result = await response.Content.ReadAsAsync<DahabResponse<GetCustomerAccountResponse>>();
                if (result is null)
                {
                    return null;
                }
                return result;
            }
        }

        public async Task<DahabResponse<bool>> ChangePin(ChangePinRequest request)
        {

            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.PostAsJsonAsync("DahabBank/reset-bank-pin", request);
                var result = await response.Content.ReadAsAsync<DahabResponse<bool>>();
                return result;
            }

        }

        public async Task<GetCustomerAccount> GetMerchantsRegistration(string msisdn, string accountNo)
        {

            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.GetAsync($"DahabBank/get-account/{msisdn}/{accountNo}");
                var result = await response.Content.ReadAsAsync<DahabResponse<GetCustomerAccount>>();
                if (result == null)
                    return null;

                return result.Data;
            }

        }


        public async Task<List<GetRegistrationReport>> GetSubscribersRegistration(string market, DateTime? fromDate, DateTime? toDate, string branch)
        {

            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.GetAsync($"DahabBank/get-registration-report-subscribers?market={market}&fromDate={fromDate?.ToShortDateString()}&toDate={toDate?.ToShortDateString()}&branch={branch}");
                var result = await response.Content.ReadAsAsync<DahabResponse<List<GetRegistrationReport>>>();
                if (result is null)
                {
                    return new List<GetRegistrationReport>();
                }

                return result.Data;
            }

        }


        public async Task<List<GetRegistrationReport>> GetMerchantsRegistration(string market, DateTime? fromDate, DateTime? toDate, string branch)
        {

            using (var _client = new HttpClient())
            {
                _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MobileBankAPI"]);
                await GetToken();
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _token);
                var response = await _client.GetAsync($"DahabBank/get-registration-report-merchants?market={market}&fromDate={fromDate?.ToShortDateString()}&toDate={toDate?.ToShortDateString()}&branch={branch}");
                var result = await response.Content.ReadAsAsync<DahabResponse<List<GetRegistrationReport>>>();
                if (result is null)
                {
                    return new List<GetRegistrationReport>();
                }

                return result.Data;
            }

        }

    }
}