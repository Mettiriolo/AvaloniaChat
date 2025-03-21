using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaChat.Models
{
    public class Config
    {
        public int ID { get; set; }
        public required string Key { get; set; }
        public required string Value { get; set; }
    }
}
