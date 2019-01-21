using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProxyProvider.Abstractions.Models
{
    public class Proxy
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(64)] public string Ip { get; set; }
        [MaxLength(8)] public string Port { get; set; }
        public string PhysicalAddress { get; set; }
        public int ConnectionSpeed { get; set; }
        public int Speed { get; set; }
        [MaxLength(16)] public string Schema { get; set; }
        [MaxLength(32)] public string Username { get; set; }
        [MaxLength(64)] public string Password { get; set; }
        public ProxyType Type { get; set; }
        public DateTime LastCheckDt { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool Alive { get; set; }
        [MaxLength(64)] public string Source { get; set; }
        /// <summary>
        /// todo: test getter-only field
        /// </summary>

        [MaxLength(72)] [Required] public string UniqueKey => $"{Ip}:{Port}";
    }
}