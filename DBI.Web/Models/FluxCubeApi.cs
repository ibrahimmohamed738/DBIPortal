using DBI_eDahab.Web.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DBI_eDahab.Web.Models
{
    public class FluxCubeApi : IFluxCubeApi
    {
        private readonly HttpClient _client;

        public FluxCubeApi(HttpClient client)
        {
            _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["Url"]);
            _client.DefaultRequestHeaders.Add("ApiKey", ConfigurationManager.AppSettings["ApiKey"]);
            _client.DefaultRequestHeaders.Add("ApiSecret", ConfigurationManager.AppSettings["ApiSecret"]);
            _client = client;
        }

        public async Task<AccountInfoRespone> GetCustomerInfo(AccountInfoRequest accountInfoRequest)
        {
            var request = new StringContent(JsonConvert.SerializeObject(accountInfoRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/getcustomerinfo", request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AccountInfoRespone>(jsonString);
            }

            return null;
        }


        public async Task SendSmsAsync(string title, string phone, string message)
        {
            using (var client = new HttpClient())
            {
                if (phone.StartsWith("65") || phone.StartsWith("66"))
                    client.BaseAddress = new Uri("http://192.168.21.45:50030/");
                else
                    client.BaseAddress = new Uri("http://192.168.23.90:5000/");

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
    }
}