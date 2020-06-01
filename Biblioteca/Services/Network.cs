using System.Net;

namespace Biblioteca
{
    public class Network
    {
        /// <summary>
        /// This method is used to check if there is an Internet connection. This requires a URL as an argument. 
        /// If no connection is made to the website indicated by the URL, the connection to the Internet has failed.
        /// </summary>
        /// <returns>Response</returns>
        public Response CheckConnection()
        {
            var client = new WebClient();

            try
            {
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return new Response
                    {
                        Connect = true,
                    };
                }
            }
            catch
            {
                return new Response
                {
                    Connect = false,
                    Message = "Sem ligação à Internet",
                };
            }
        }
    }
}
