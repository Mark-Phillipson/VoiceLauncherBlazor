using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
    [Index(nameof(NameOfGrammar), IsUnique = true)]
    public class GrammarName
    {
        public GrammarName()
        {
            GrammarItems = new HashSet<GrammarItem>();
        }
        public int Id { get; set; }
        [Required]
        [StringLength(40)]
        public required string NameOfGrammar { get; set; }
        public ICollection<GrammarItem> GrammarItems { get; }
    }
}
