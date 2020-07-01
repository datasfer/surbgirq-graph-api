using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.Models
{
    public class Book
    {
        public string bookID { get; set; }
        public string book { get; set; }
        public string title { get; set; }
        public int bookOrder { get; set; }
        public int chapters { get; set; }
        public string testament { get; set; }
    }
}
