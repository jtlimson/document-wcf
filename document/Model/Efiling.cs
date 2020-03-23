using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace document.Model
{
    public class Efiling
    {
        [Key]
        public int ID { get; set; }
        public string PIN { get; set; }        
        public string FileName { get; set; }
        public string Directory { get; set; }
        public string FilePassword { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
    }
}