using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
     public  class Idiosyncrasy
    {
         public int Id { get; set; }
        [StringLength(60)]
        public string FindString { get; set; }
        [StringLength(60)]
        public string ReplaceWith { get; set; }
    }
}
