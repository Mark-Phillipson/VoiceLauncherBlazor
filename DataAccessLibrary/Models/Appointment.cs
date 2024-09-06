using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLibrary.Models
{

    public class Appointment
    {
        public int Id { get; set; }
        public int AppointmentType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [StringLength(255)]
        public string? Caption { get; set; }
        [StringLength(255)]
        public string? Description { get; set; }
        [StringLength(255)]
        public string? Location { get; set; }
        public int Label { get; set; }
        public int Status { get; set; }
        public bool AllDay { get; set; }
        [StringLength(255)]
        public string? Recurrence { get; set; }
    }
}
