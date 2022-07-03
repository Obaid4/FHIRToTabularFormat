using FHIRToTabularFormat.Controllers;
using FHIRToTabularFormat.Models;

namespace FHIRToTabularFormat.Test
{
    [TestClass]
    public class UnitTests
    {
        private FileUploadController fileUploadController;
        public UnitTests() 
        {
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

        [TestMethod]
        public void TestAddPatientDataWithProp() 
        {
            string testStr = "\"system\": \"phone\",";

            Patient pat = new Patient();

            fileUploadController.AddPatientData(pat, testStr, "telecom");

            Assert.AreEqual("phone", pat.Telecom[pat.Telecom.Count - 1]["system"]);
        }

        [TestMethod]
        public void TestDateTimeProcessDateTimeValue() 
        {
            string testStr = "\"deceasedDateTime\": \"1998-08-15T13:05:53+01:00\",";

            DateTime dateTime = fileUploadController.ProcessDateTimeValue(testStr, "deceased");

            DateTime test = new DateTime(1998,08,15,13,05,53, DateTimeKind.Utc);
            Assert.AreEqual(test, dateTime);
        }
    }
}