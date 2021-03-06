﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using apitocatalog.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Newtonsoft.Json;
using System.Text;

namespace apitocatalog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiCatalogController : ControllerBase
    {
        private readonly ILogger<ApiCatalogController> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public ApiCatalogController(ILogger<ApiCatalogController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
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
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(def.ApiAdminUsername + ":" + def.ApiAdminPassword));


                var httpClient = _clientFactory.CreateClient("HttpClientWithUntrustedSSL");

                var request = new HttpRequestMessage();
                request.Method = new HttpMethod("Get");
                StringBuilder orgURL = new StringBuilder();
                orgURL.Append("https://"); 
                orgURL.Append(def.ApiManagerHost);
                orgURL.Append(":8075/api/portal/v1.2/organizations");
                request.Headers.Add("Authorization", "Basic " + encoded);
                if (!String.IsNullOrWhiteSpace(def.OrganizationName))
                {
                    orgURL.Append("?field=name&op=Like&value=");
                    orgURL.Append(def.OrganizationName);
                }
                request.RequestUri = new Uri(orgURL.ToString());

                using (var response = await httpClient.SendAsync(request))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("received API response -" + apiResponse);
                    var organizations = JsonConvert.DeserializeObject<List<Organization>>(apiResponse);
                    if (organizations != null && organizations.Count() > 0)
                    {
                        OrgId = organizations.FirstOrDefault().Id.ToString();
                        _logger.LogInformation("Organization found - Id " + OrgId);
                    }
                }

                if (!String.IsNullOrWhiteSpace(OrgId))
                {
                    //add api as backend api  
                    httpClient = _clientFactory.CreateClient("HttpClientWithUntrustedSSL");
                    var request2 = new HttpRequestMessage();
                    request2.Method = new HttpMethod("Post");
                    request2.RequestUri = new Uri("https://" + def.ApiManagerHost + ":8075/api/portal/v1.2/apirepo/importFromUrl");
                    request2.Headers.Add("Authorization", "Basic " + encoded);
                    var postdata = new Dictionary<string, string> {
                                {"organizationId", OrgId},
                                {"name", def.ApiName},
                                {"type", "swagger"},
                                {"url", def.SwaggerURL}
                            };
                    request2.Content = new FormUrlEncodedContent(postdata);

                    using (var response = await httpClient.SendAsync(request2))
                    {
                        if (response.IsSuccessStatusCode)
                        { _logger.LogInformation("API added in the catalog"); }
                        else { _logger.LogError("Error occurred while adding API in the catalog" + response.Content); }
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
