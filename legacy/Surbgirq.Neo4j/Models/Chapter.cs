using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.Models
{
    public class Chapter
    {
        [JsonProperty("book")]
        public string Book { get; set; }
        
        [JsonProperty("num")]
        public int Number { get; set; }

        public override string ToString()
        {
            return $"{Book}-{Number}";
        }
    }

}
