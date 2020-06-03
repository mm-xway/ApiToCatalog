using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using apitocatalog.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Newtonsoft.Json;

namespace apitocatalog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiCatalogController : ControllerBase
    {
        private readonly ILogger<ApiCatalogController> _logger;

        public ApiCatalogController(ILogger<ApiCatalogController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async void Post([FromBody] ApiDefination def)
        {
            // validate model 
            if (ModelState.IsValid)
            {
                // get Organization Id 
                // if Organization not found, use API Development 
                string OrgId = string.Empty;
                using (var httpClient = new HttpClient())
                {
                    var request = new HttpRequestMessage();
                    request.Method = new HttpMethod("Get");
                    request.RequestUri = new Uri(def.ApiManagerHost + "/api/portal/v1.2/organizations");
                    String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(def.ApiAdminUsername + ":" + def.ApiAdminPassword));
                    request.Headers.Add("Authorization", "Basic " + encoded);

                    using (var response = await httpClient.SendAsync(request))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        var organizations = JsonConvert.DeserializeObject<List<Organization>>(apiResponse);
                        if (organizations != null && organizations.Count() > 0)
                        {
                            OrgId = organizations.FirstOrDefault().Id.ToString();
                            _logger.LogInformation("Organization found - Id " + OrgId);
                        }
                    }
                }

                if (!String.IsNullOrWhiteSpace(OrgId))
                {

                    // add api as backend api  

                    using (var httpClient = new HttpClient())
                    {
                        var request = new HttpRequestMessage();
                        request.Method = new HttpMethod("Post");
                        request.RequestUri = new Uri(def.ApiManagerHost + "/api/portal/v1.2/apirepo/importFromUrl");
                        String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(def.ApiAdminUsername + ":" + def.ApiAdminPassword));
                        request.Headers.Add("Authorization", "Basic " + encoded);
                        var postdata = new Dictionary<string, string> {
                        {"organizationId", OrgId},
                        {"name", def.ApiName},
                        {"type", "swagger"},
                        {"url", def.SwaggerURL}
                    };
                        request.Content = new FormUrlEncodedContent(postdata);

                        using (var response = await httpClient.SendAsync(request))
                        {
                            if (response.IsSuccessStatusCode)
                            { _logger.LogInformation("API added in the catalog"); }
                            else { _logger.LogError("Error occurred while adding API in the catalog" + response.Content); }
                        }
                    }
                }
                else
                {
                    _logger.LogError("No Organization found");
                }
            }
            else
            {
                _logger.LogError("Invalid data posted");
            }

        }
    }
}
