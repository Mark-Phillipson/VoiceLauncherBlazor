using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class AspNetUserTokens
    {
        [Key]
        public string UserId { get; set; }
        [Key]
        [StringLength(128)]
        public string LoginProvider { get; set; }
        [Key]
        [StringLength(128)]
        public string Name { get; set; }
        public string Value { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(AspNetUsers.AspNetUserTokens))]
        public virtual AspNetUsers User { get; set; }
    }
}
