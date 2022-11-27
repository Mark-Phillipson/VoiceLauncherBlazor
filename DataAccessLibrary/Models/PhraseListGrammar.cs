using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
     public  class PhraseListGrammar
    {
         public int Id { get; set; }
        [StringLength(100)]
        [Required]
        public string PhraseListGrammarValue { get; set; }
    }
}
