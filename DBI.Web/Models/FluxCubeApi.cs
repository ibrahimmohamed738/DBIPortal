using DBI_eDahab.Web.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DBI_eDahab.Web.Models
{
    public class FluxCubeApi
    {
        public async Task<AccountInfoRespone> GetCustomerInfo(AccountInfoRequest accountInfoRequest)
        {
            using (var _client = new HttpClient())
            {
                bool isProd;
                bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["IsProduction"], out isProd);
                if (isProd)
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["Url"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKeyP"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecretP"]);
                }
                else
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["UrlUAT"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKey"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecret"]);
                }

                var request = new StringContent(JsonConvert.SerializeObject(accountInfoRequest), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("api/getcustomerinfo", request);
                var jsonString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        eventLog.WriteEntry(jsonString, EventLogEntryType.Information, 101, 1);
                    }
                    return JsonConvert.DeserializeObject<AccountInfoRespone>(jsonString);
                }

                return null;
            }
            
        }

        public async Task SendSmsAsync(string title, string phone, string message)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["SMSUrl"]);
                var request = new
                {
                    phone,
                    title,
                    message
                };
                var json = JsonConvert.SerializeObject(request);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync("SMS", data);
            }
        }

        public async Task<string> SendSMS(SendSMSRequest smsRequest)
        {
            var request = new StringContent(JsonConvert.SerializeObject(smsRequest), Encoding.UTF8, "application/json");
            using (var _client = new HttpClient())
            {
                bool isProd;
                bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["IsProduction"], out isProd);
                if (isProd)
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["Url"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKeyP"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecretP"]);
                }
                else
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["UrlUAT"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKey"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecret"]);
                }

                var response = await _client.PostAsync("api/SMS", request);
                var jsonString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        eventLog.WriteEntry(jsonString, EventLogEntryType.Information, 101, 1);
                    }

                    return JsonConvert.DeserializeObject<string>(jsonString);
                }
                return null;
            }
             
        }

        public async Task<List<GLAccountResponse>> GetGLAccountBalance(GLAccountRequest accountInfo)
        {
            using (var _client = new HttpClient())
            {
                bool isProd;
                bool.TryParse(ConfigurationManager.AppSettings["IsProduction"], out isProd);
                if (isProd)
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["Url"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKeyP"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecretP"]);
                }
                else
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["UrlUAT"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKey"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecret"]);
                }
               
                var request = new StringContent(JsonConvert.SerializeObject(accountInfo), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("api/getglbalances", request);
                var jsonString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        eventLog.WriteEntry(jsonString, EventLogEntryType.Information, 101, 1);
                    }
                    return JsonConvert.DeserializeObject<List<GLAccountResponse>>(jsonString);
                }

                return null;
            }

        }

        public async Task<CreateTransactionResponse> CreateTransaction(CreateTransactionRequest CreateRequest)
        {
            using (var _client = new HttpClient())
            {
                bool isProd;
                bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["IsProduction"], out isProd);
                if (isProd)
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["Url"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKeyP"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecretP"]);
                }
                else
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["UrlUAT"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKey"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecret"]);
                }
               
                var request = new StringContent(JsonConvert.SerializeObject(CreateRequest), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("api/CreateTransaction", request);
                string jsonString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        eventLog.WriteEntry(jsonString, EventLogEntryType.Information, 101, 1);
                    }
                    return JsonConvert.DeserializeObject<CreateTransactionResponse>(jsonString);
                }
                Console.WriteLine(jsonString);
                return null;
            }
        }

        public async Task<CheckDBITransResponse> GetDBITransaction(CheckDBITransRequest checkrequest)
        {
            using (var _client = new HttpClient())
            {
                bool isProd;
                bool.TryParse(ConfigurationManager.AppSettings["IsProduction"], out isProd);
                if (isProd)
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["Url"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKeyP"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecretP"]);
                }
                else
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["UrlUAT"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKey"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecret"]);
                }

                var request = new StringContent(JsonConvert.SerializeObject(checkrequest), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("api/CheckTransactionStatus", request);
                var jsonString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        eventLog.WriteEntry(jsonString, EventLogEntryType.Information, 101, 1);
                    }
                    return JsonConvert.DeserializeObject<CheckDBITransResponse>(jsonString);
                }
                Console.WriteLine(jsonString);
                return null;
            }

        }


        public async Task<byte []> GetCustomerSignature(AccountInfoRequest accountInfoRequest)
        {
            using (var _client = new HttpClient())
            {
                bool isProd;
                bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["IsProduction"], out isProd);
                if (isProd)
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["Url"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKeyP"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecretP"]);
                }
                else
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["UrlUAT"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKey"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecret"]);
                }

                var request = new StringContent(JsonConvert.SerializeObject(accountInfoRequest), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("api/GetCustomerSignature", request);
                var jsonString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        eventLog.WriteEntry(jsonString, EventLogEntryType.Information, 101, 1);
                    }
                    return JsonConvert.DeserializeObject<byte[]>(jsonString);
                }

                return null;
            }

        }

        public async Task<byte[]> GetCustomerPhoto(AccountInfoRequest accountInfoRequest)
        {
            using (var _client = new HttpClient())
            {
                bool isProd;
                bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["IsProduction"], out isProd);
                if (isProd)
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["Url"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKeyP"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecretP"]);
                }
                else
                {
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["UrlUAT"]);
                    _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKey"]);
                    _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecret"]);
                }

                var request = new StringContent(JsonConvert.SerializeObject(accountInfoRequest), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("api/GetCustomerPhoto", request);
                var jsonString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        eventLog.WriteEntry(jsonString, EventLogEntryType.Information, 101, 1);
                    }
                    return JsonConvert.DeserializeObject<byte[]>(jsonString);
                }

                return null;
            }

        }




    }
}