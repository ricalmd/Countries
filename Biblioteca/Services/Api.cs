using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Biblioteca
{
    public class Api
    {
        public async Task<Response> GetCountries(string url, string controller)
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(url);

                var response = await client.GetAsync(controller);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        Connect = false,
                        Message = result,
                    };
                }
                var countries = JsonConvert.DeserializeObject<List<Country>>(result);

                return new Response
                {
                    Connect = true,
                    Result = countries,
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Connect = false,
                    Message = ex.Message,
                };
            }
        }
    }
}
