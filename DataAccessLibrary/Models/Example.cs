using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class Example
    {
        public int Id { get; set; }
        public int NumberValue { get; set; }
        [Required]
        public required string Text { get; set; }
        [Required]
        [Display(Name = "Large Text")]
        public required string LargeText { get; set; }
        public bool Boolean { get; set; }
        [Display(Name = "Date Value")]
        public DateTime DateValue { get; set; }
    }
}
