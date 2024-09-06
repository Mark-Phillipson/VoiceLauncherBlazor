using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
    public class ApplicationDetail
    {
        public int Id { get; set; }
        [StringLength(60)]
        [Required]
        public required string ProcessName { get; set; }
        [Required]
        [StringLength(255)]
        public required string ApplicationTitle { get; set; }

    }
}
