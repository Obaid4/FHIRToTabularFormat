using FHIRToTabularFormat.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FHIRToTabularFormat.Test
{
    [TestClass]
    public class UnitTests
    {
        private FileUploadController fileUploadController;
        public UnitTests() 
        {
            //var webAppFactory = new WebApplicationFactory<Program>();
            //httpClient = webAppFactory.CreateDefaultClient();
            fileUploadController = new FileUploadController();
        }

        [TestMethod]
        public void DefaultProcessValueTest()
        {
            string testStr = "\"family\": \"Dickens475\",";
            string result = fileUploadController.ProcessValue(testStr, null);

            Assert.AreEqual("Dickens475", result);
        }

        [TestMethod]
        public void AddressLineProcessValueTest() 
        {
            string testStr = "\"line\": [ \"859 Altenwerth Run Unit 88\" ],";
            string result = fileUploadController.ProcessValue(testStr, "address line");

            Assert.AreEqual("859 Altenwerth Run Unit 88", result);
        }
    }
}