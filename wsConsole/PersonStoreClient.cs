using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace wsConsole
{
    class PacketStoreClient
    {
        class Packet
        {
            public int ID { get; set; }
            public string Name { get; set; }

        }

        class Program
        {
            static void Main()
            {
                RunAsync().Wait();
            }

            static async Task RunAsync()
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:4561/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                    HttpResponseMessage response = await client.GetAsync("api/Packet");

                    // HTTP POST
                    var test = new Packet() { ID = 0, Name = "Orkun"};

                    response = await client.PostAsync("api/Packet", new StringContent(
                        JsonConvert.SerializeObject(test),
                        Encoding.UTF8,
                        "application/json"));

                    // HTTP GET
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var packet = JsonConvert.DeserializeObject<Packet[]>(result);



                        Console.WriteLine("Selam {0}", packet[0].Name);
                    }

                    
                    if (response.IsSuccessStatusCode)
                    {
                        Uri url = response.Headers.Location;
                        
                        // HTTP PUT
                        test.Name = "Ozan";   // Update Name
                        //response = await client.PutAsJsonAsync(url, test);

                        // HTTP DELETE
                        response = await client.DeleteAsync(url);
                    }
                }
            }
        }
    }
}