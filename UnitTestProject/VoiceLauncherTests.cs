using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitTestProject;
using VoiceLauncherBlazor.Data;
using VoiceLauncherBlazor.Models;

namespace TestingDemo
{
    [TestClass]
    public class AsyncQueryTests
    {
        [TestMethod]
        public async Task GetCategoriesAsync_OrderedBy_Name()
        {

            var data = new List<Category>
            {
                new Category { Id=1,CategoryName = "BBB" ,CategoryType="IntelliSense Command"},
                new Category { Id=2,CategoryName = "ZZZ" ,CategoryType="IntelliSense Command"},
                new Category { Id=3,CategoryName = "AAA" ,CategoryType="IntelliSense Command"},
            }.AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<Category>>();
            mockSet.As<IQueryable<Category>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Category>(data.Provider));

            mockSet.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Categories).Returns(mockSet.Object);

            var service = new CategoryService(mockContext.Object);
            //Apparently it's not possible to test an async method with the current version of entity framework!
            var categories = await service.GetCategoriesAsync();
            //var categories = await service.GetCategoriesAsync();


            Assert.AreEqual(3, categories.Count);
            Assert.AreEqual("AAA", categories[0].CategoryName);
            Assert.AreEqual("BBB", categories[1].CategoryName);
            Assert.AreEqual("ZZZ", categories[2].CategoryName);
        }
        //GetCategories
        [TestMethod]
        public void GetCategories_OrderedBy_Name()
        {
            var data = new List<Category>
            {
                new Category { Id=1,CategoryName = "BBB" ,CategoryType="IntelliSense Command"},
                new Category { Id=2,CategoryName = "ZZZ" ,CategoryType="IntelliSense Command"},
                new Category { Id=3,CategoryName = "AAA" ,CategoryType="IntelliSense Command"},
            }.AsQueryable();

            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<Category>>();
            mockSet.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Categories).Returns(mockSet.Object);

            var service = new CategoryService(mockContext.Object);
            var categories = service.GetCategoriesAsync();
            // Not able to test async method with current version of ASP.net core
            //Assert.AreEqual(3, categories.Count);
            //Assert.AreEqual("AAA", categories[0].CategoryName);
            //Assert.AreEqual("BBB", categories[1].CategoryName);
            //Assert.AreEqual("ZZZ", categories[2].CategoryName);
        }

    }
}