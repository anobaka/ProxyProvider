using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ProxyProvider.Models
{
    public class PurposeHttpClientOptions
    {
        [Key]
        public string Purpose { get; set; }

        [Column(nameof(DefaultHeaders))]
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