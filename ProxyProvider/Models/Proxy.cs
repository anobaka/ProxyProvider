using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProxyProvider.Models
{
    public class Proxy
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(64)]
        public string Ip { get; set; }
        [MaxLength(8)]
        public string Port { get; set; }
        public string PhysicalAddress { get; set; }
        public int ConnectionSpeed { get; set; }
        public int Speed { get; set; }
        public string Schema { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public ProxyType Type { get; set; }
        public DateTime LastCheckDt { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime UpdateDt { get; set; }
        public bool Alive { get; set; }
        public string Source { get; set; }
    }
}