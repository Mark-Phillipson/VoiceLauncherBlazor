using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLibrary.Models
{
    public partial class AspNetUserLogins
    {
        [Key]
        [StringLength(128)]
        public string LoginProvider { get; set; }
        [Key]
        [StringLength(128)]
        public string ProviderKey { get; set; }
        public string ProviderDisplayName { get; set; }
        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(AspNetUsers.AspNetUserLogins))]
        public virtual AspNetUsers User { get; set; }
    }
}
