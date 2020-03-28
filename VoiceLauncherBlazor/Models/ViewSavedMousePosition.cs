using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoiceLauncherBlazor.Models
{
    public partial class ViewSavedMousePosition
    {
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string NamedLocation { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? Created { get; set; }
    }
}
