using DataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLibrary.Services
{
	public class AppointmentService
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
		public AppointmentService(IDbContextFactory<ApplicationDbContext> context)
		{
			_contextFactory = context;
		}
		public async Task<string> SaveAppointment(Appointment appointment)
		{
			using var context = _contextFactory.CreateDbContext();
			var action = "Updated";
			if (appointment != null)
			{
				if (appointment.Id == -1)
				{
					appointment.Id = 0;
					context.Appointments.Add(appointment);
					action = "Added";
				}
				else
				{
					context.Appointments.Update(appointment);
				}
				await context.SaveChangesAsync();
				return $"Appointment {action} Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			return $"Cannot save appointment as not supplied";
		}
		public async Task<string> RemoveAppointment(string id)
		{
			using var context = _contextFactory.CreateDbContext();
			var appointment = await context.Appointments.Where(v => v.Id.ToString() == id).FirstOrDefaultAsync();
			if (appointment != null)
			{
				context.Appointments.Remove(appointment);
				await context.SaveChangesAsync();
				return $"Appointment Removed Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
			}
			return $"Appointment (Id: {id}) not found so cannot be removed!";
		}
		public async Task<string> SaveAppointments(List<Appointment> appointments)
		{
			using var context = _contextFactory.CreateDbContext();
			context.Appointments.UpdateRange(appointments);
			await context.SaveChangesAsync();
			return $"Appointments Saved Successfully! {DateTime.UtcNow:h:mm:ss tt zz}";
		}
		public async Task<List<Appointment>> GetAppointments()
		{
			using var context = _contextFactory.CreateDbContext(); 
			var appointments = context.Appointments.Where(v => v.StartDate >= DateTime.Now.AddMonths(-3));
			return await appointments.ToListAsync();
		}

		private async Task<List<Appointment>> CreateDemoAppointmentsAsync()
		{
			await RemoveAllAppointments();
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

		async Task RemoveAllAppointments()
		{
			using var context = _contextFactory.CreateDbContext();
			List<Appointment> appointments = await context.Appointments.ToListAsync();
			context.Appointments.RemoveRange(appointments);
			await context.SaveChangesAsync();
			return;
		}
	}
}