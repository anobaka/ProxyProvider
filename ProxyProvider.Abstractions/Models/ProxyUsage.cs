using System;
using System.ComponentModel.DataAnnotations;

namespace ProxyProvider.Abstractions.Models
{
    public class ProxyUsage
    {
        [Key]
        public int Id { get; set; }
        public int ProxyId { get; set; }
        public string Purpose { get; set; }
        [ConcurrencyCheck]
        public DateTime LockReleaseDt { get; set; }
    }
}
