using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.Models
{
    public class Company : BaseModel
    {
        [JsonProperty("name")]
        public string name { get; set; }


        public override string ToString()
        {
            return this.name;
        }
    }
}
