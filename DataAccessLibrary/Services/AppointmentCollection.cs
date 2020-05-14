using DataAccessLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;


namespace DataAccessLibrary.Services
{

    public static partial class AppointmentCollection
    {
        public static List<Appointment> GetAppointments()
        {
            DateTime date = DateTimeUtils.CreateWeekStart(DateTime.Now);
            var dataSource = new List<Appointment>() {
            new Appointment {
                Id=1,
                Caption = "Install New Router in Dev Room",
                StartDate = date + (new TimeSpan(0, 10, 0, 0)),
                EndDate = date + (new TimeSpan(0, 12, 0, 0)),
                Label = 6,
                Status = 1
            },
            new Appointment {
                Id=2,
                Caption = "Upgrade Personal Computers",
                StartDate = date + (new TimeSpan(0,  13, 0, 0)),
                EndDate = date + (new TimeSpan(0, 14, 30, 0)),
                Label = 1,
                Status = 1
            },
            new Appointment {
                Id=3,
                Caption = "Website Re-Design Plan",
                StartDate = date + (new TimeSpan(1, 9, 30, 0)),
                EndDate = date + (new TimeSpan(1, 11, 30, 0)),
                Label = 1,
                Status = 1
            },
            new Appointment {
                Id=4,
                Caption = "New Brochures",
                StartDate = date + (new TimeSpan(1, 13, 30, 0)),
                EndDate = date + (new TimeSpan(1, 15, 15, 0)),
                Label = 8,
                Status = 1
            },
            new Appointment {
                Id=5,
                Caption = "Book Flights to San Fran for Sales Trip",
                StartDate = date + (new TimeSpan(1, 12, 0, 0)),
                EndDate = date + (new TimeSpan(1, 13, 0, 0)),
                AllDay = true,
                Label = 8,
                Status = 1
            },
            new Appointment {
                Id=6,
                Caption = "Approve Personal Computer Upgrade Plan",
                StartDate = date + (new TimeSpan(2, 10, 0, 0)),
                EndDate = date + (new TimeSpan(2, 12, 0, 0)),
                Label = 8,
                Status = 1
            },
            new Appointment {
                Id=7,
                Caption = "Final Budget Review",
                StartDate = date + (new TimeSpan(2, 13, 0, 0)),
                EndDate = date + (new TimeSpan(2, 15, 0, 0)),
                Label = 1,
                Status = 1
            },
            new Appointment {
                Id=8,
                Caption = "Install New Database",
                StartDate = date + (new TimeSpan(3, 9, 45, 0)),
                EndDate = date + (new TimeSpan(3, 11, 15, 0)),
                Label = 6,
                Status = 1
            },
            new Appointment {
                Id=9,
                Caption = "Approve New Online Marketing Strategy",
                StartDate = date + (new TimeSpan(3,  12, 0, 0)),
                EndDate = date + (new TimeSpan(3, 14, 0, 0)),
                Label = 1,
                Status = 1
            },
            new Appointment {
                Id= 10,
                Caption = "Customer Workshop",
                StartDate = date + (new TimeSpan(4,  11, 0, 0)),
                EndDate = date + (new TimeSpan(4, 12, 0, 0)),
                AllDay = true,
                Label = 8,
                Status = 1
            },
            new Appointment {
                Id= 11,
                Caption = "Prepare 2015 Marketing Plan",
                StartDate = date + (new TimeSpan(4,  11, 0, 0)),
                EndDate = date + (new TimeSpan(4, 13, 30, 0)),
                Label = 1,
                Status = 1
            },
            new Appointment {
                Id=12,
                Caption = "Brochure Design Review",
                StartDate = date + (new TimeSpan(4, 14, 0, 0)),
                EndDate = date + (new TimeSpan(4, 15, 30, 0)),
                Label = 1,
                Status = 1
            },
            new Appointment {
                Id=13,
                Caption = "Create Icons for Website",
                StartDate = date + (new TimeSpan(5, 10, 0, 0)),
                EndDate = date + (new TimeSpan(5, 11, 30, 0)),
                Label = 1,
                Status = 1
            },
            new Appointment {
                Id=14,
                Caption = "Launch New Website",
                StartDate = date + (new TimeSpan(5, 12, 20, 0)),
                EndDate = date + (new TimeSpan(5, 14, 0, 0)),
                Label = 8,
                Status = 1
            },
            new Appointment {
                Id=15,
                Caption = "If lockdown is over go for a bike ride with San Fairy Ann cycling club",
                StartDate = date + (new TimeSpan(6, 09, 15, 0)),
                EndDate = date + (new TimeSpan(6, 13, 30, 0)),
                Label = 8,
                Status = 1
            }
        };
            return dataSource;
        }
    }
}