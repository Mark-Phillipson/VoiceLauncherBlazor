using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{
    public class Todo
    {
        public int Id { get; set; }
        [StringLength(255)]
        [Required]
        [MinLength(1, ErrorMessage = "Please enter a title between one and 255 characters!")]
        public string Title { get; set; }
        [StringLength(1000)]
        [Required]
        [MinLength(1, ErrorMessage = "Please enter a description between one and a thousand characters!")]
        public string Description { get; set; }
        public bool Completed { get; set; } = false;
        [StringLength(255, ErrorMessage = "Please enter 255 characters or less for the project!")]
        public string Project { get; set; }
        public bool Archived { get; set; } = false;
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
