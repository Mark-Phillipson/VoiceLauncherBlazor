using Microsoft.EntityFrameworkCore;
using System.Linq;
using VoiceLauncherBlazor.Models;

namespace VoiceLauncherBlazor.Data
{
    public class DataGridService
    {
        ApplicationDbContext db = new ApplicationDbContext();

        //To Get all Orders details   
        public DbSet<Language> GetAllOrders()
        {
            try
            {
                return db.Languages;
            }
            catch
            {
                throw;
            }
        }
        // To Add new Order record
        public void AddOrder(Language Order)
        {
            try
            {
                db.Languages.Add(Order);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        //To Update the records of a particluar Order    
        public void UpdateOrder(Language Order)
        {
            try
            {
                var data = db.Languages.Where(x => x.Id == Order.Id).SingleOrDefault();
                data.Id = Order.Id;
                data.LanguageName = Order.LanguageName;
                data.Active = Order.Active;
                data.CustomIntelliSense = Order.CustomIntelliSense;


                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        //Get the details of a particular Order    
        public Language GetOrderData(int id)
        {
            try
            {
                Language Order = db.Languages.Find(id);
                return Order;
            }
            catch
            {
                throw;
            }
        }
    }

}