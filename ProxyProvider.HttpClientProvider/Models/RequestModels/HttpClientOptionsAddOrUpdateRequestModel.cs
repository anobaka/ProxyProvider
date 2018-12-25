using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProxyProvider.HttpClientProvider.Models.RequestModels
{
    public class HttpClientOptionsAddOrUpdateRequestModel
    {
        [Required]
        public string Purpose { get; set; }
        public Dictionary<string, string> DefaultHeaders { get; set; }
        public bool UseProxy { get; set; }
        public int ProxyLockLifetime { get; set; }
    }
}