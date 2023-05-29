using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
    public class Prompt
    {
        public int Id { get; set; }
        [Required]
        [StringLength(3000)]
        public string PromptText { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}
