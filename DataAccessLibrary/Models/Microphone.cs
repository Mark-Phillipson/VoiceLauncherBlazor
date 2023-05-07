using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
     public  class Microphone
    {
        public int Id { get; set; }
        [StringLength(100)]
        [Required]
        public string MicrophoneName { get; set; } = "";
        public bool Default { get; set; }
    }
}
