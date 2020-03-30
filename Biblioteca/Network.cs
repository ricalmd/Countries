using System.Net;

namespace Biblioteca
{
    public class Network
    {
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
                    Message = "Problemas com a ligação à Internet",
                };
            }
        }
    }
}
