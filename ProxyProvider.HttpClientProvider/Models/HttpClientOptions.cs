using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ProxyProvider.HttpClientProvider.Models
{
    [Table("http_client_options")]
    public class HttpClientOptions
    {
        [Key]
        [Column("purpose")]
        public string Purpose { get; set; }

        [Column("default_headers")]
        public string DefaultHeaderString
        {
            get => DefaultHeaders == null ? null : JsonConvert.SerializeObject(DefaultHeaders);
            set => DefaultHeaders = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
        }

        [NotMapped]
        public Dictionary<string, string> DefaultHeaders { get; set; }

        public bool UseProxy { get; set; }
        public int ProxyLockLifetime { get; set; }
    }
}