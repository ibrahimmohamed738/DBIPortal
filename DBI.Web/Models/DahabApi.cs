using DBI_eDahab.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

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
    }
}