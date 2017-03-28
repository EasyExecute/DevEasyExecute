using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Xunit;

namespace EasyExecute.Tests
{
    public class wepapi_test
    {
        [Fact]
        public void ensure_all_api_works_has_id_has_command_has_result()
        {
            var products = Task.Run(() => TestMyWeb("http://localhost:8389/", "GetAllProducts/1")).Result;
            Console.WriteLine("Data:" + products.ToList().First());
        }

        public async Task<IEnumerable<object>> TestMyWeb(string endpoint, string action)
        {
            var config = new HttpSelfHostConfiguration(endpoint);
            config.Routes.MapHttpRoute("API Default", "api/{controller}/{action}/{id}", new
            {
                id = RouteParameter.Optional
            });
            using (var server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                ServicePointManager.MaxServicePointIdleTime = Timeout.Infinite;
                var client = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(10),
                    BaseAddress = new Uri(endpoint)
                };
                var result = await client.GetAsync("api/products/" + action).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsAsync<IEnumerable<object>>().ConfigureAwait(false);
            }
        }

        public class ProductsController : ApiController
        {
            private static EasyExecuteLib.EasyExecute EasyExecute { set; get; }

            public ProductsController()
            {
                ServicePointManager.MaxServicePointIdleTime = Timeout.Infinite;
                 EasyExecute = new EasyExecuteLib.EasyExecute();
            }

            [HttpGet]
            public async Task<HttpResponseMessage> GetAllProducts(string id)
            {
                var result= await EasyExecute.ExecuteAsync(id, id
                  , (command) =>Task.FromResult(new List<object> { command }));
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
        }
    }
}