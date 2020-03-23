using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace document.Model
{
    public class Log
    {
        [Key]
        public int ID { get; set; }
        public string Pin { get; set; } // FK
        public string Action { get; set; }
        public string Location { get; set; }
        public string OriginalLocation { get; set; } // can be null
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
    }
} 