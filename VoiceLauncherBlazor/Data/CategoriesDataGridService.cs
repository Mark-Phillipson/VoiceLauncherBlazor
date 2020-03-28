using Microsoft.EntityFrameworkCore;
using System.Linq;
using VoiceLauncherBlazor.Models;

namespace VoiceLauncherBlazor.Data
{
    public class CategoriesDataGridService
    {
        ApplicationDbContext db = new ApplicationDbContext();

        //To Get all Orders details   
        public DbSet<Category> GetAllOrders()
        {
            try
            {
                return db.Categories;
            }
            catch
            {
                throw;
            }
        }
        // To Add new Order record
        public void AddOrder(Category Order)
        {
            try
            {
                db.Categories.Add(Order);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        //To Update the records of a particluar Order    
        public void UpdateOrder(Category Order)
        {
            try
            {
                var data = db.Categories.Where(x => x.Id == Order.Id).SingleOrDefault();
                data.Id = Order.Id;
                data.CategoryName = Order.CategoryName;
                data.CategoryType = Order.CategoryType;
                data.CustomIntelliSense = Order.CustomIntelliSense;
                data.Launcher = Order.Launcher;


                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        //Get the details of a particular Order    
        public Category GetOrderData(int id)
        {
            try
            {
                Category Order = db.Categories.Find(id);
                return Order;
            }
            catch
            {
                throw;
            }
        }
    }

}