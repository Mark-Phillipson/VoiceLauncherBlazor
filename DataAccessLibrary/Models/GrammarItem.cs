using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
    public class GrammarItem
    {
        public int Id { get; set; }
        public int GrammarNameId { get; set; }
        public GrammarName GrammarName { get; set; }
        [Required]
        [StringLength(60)]
        public string Value { get; set; }
    }
}
