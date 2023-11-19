using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
   [Table("PhraseListGrammars")]
     public  class PhraseListGrammar
    {
         public int Id { get; set; }
        [StringLength(100)]
        [Required]
        public string PhraseListGrammarValue { get; set; }
    }
}
