using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Biblioteca.Models
{
    public class Api
    {
        /// <summary>
        /// Two parameters are received from the LoadApiCountries() method in MainWindow: the path to the API (url)
        /// and the path to its data (controller). The client variable instantiates the HttpClient class, receiving 
        /// the API URL. From there the response variable establishes the link to the API data and the result
        /// variable receives that same data. Specific connection problems can happen and, if they do, then the 
        /// Response is returned with the Connect property equal to false and the Message property equal to result. 
        /// If all goes well, the JsonConvert class's DeserializeObject method translates the data contained in 
        /// result into the countries variable. Finally, Response is returned with the Connect property equal to 
        /// true and the Result property with data from the countries variable.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="controller"></param>
        /// <returns>Response</returns>
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
                        Message = result
                    };
                }
                var countries = JsonConvert.DeserializeObject<List<Country>>(result);

                return new Response
                {
                    Connect = true,
                    Result = countries
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
