using System.ComponentModel.DataAnnotations;

namespace apitocatalog.models
{
    public class ApiDefination
    {
        public ApiDefination()
        {
        }
        [Required]
        public string ApiManagerHost { get; set; }
        [Required]
        public string ApiAdminUsername { get; set; }
        [Required]
        public string ApiAdminPassword { get; set; }
        public string OrganizationName { get; set; }
        public string ApiName { get; set; }
        [Required]
        public string SwaggerURL { get; set; }
    }
}