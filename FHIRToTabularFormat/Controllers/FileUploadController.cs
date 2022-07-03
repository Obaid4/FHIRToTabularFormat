using FHIRToTabularFormat.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace FHIRToTabularFormat.Controllers
{
    public class FileUploadController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("FileUpload")]
        public async Task<IActionResult> Index(List<IFormFile> files) 
        {
            var size = files.Sum(f => f.Length);

            var filePaths = new List<string>();

            foreach (var file in files) 
            {
                // File Exists
                if (file.Length > 0) 
                {
                    // Create a path 
                    var filePath = Path.Combine($"{Directory.GetCurrentDirectory()}/wwwroot/data", file.FileName);
                    filePaths.Add(filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create)) 
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            ProcessData(filePaths);

            return Ok(new { files.Count, size, filePaths});
        }

        public void ProcessData(List<string> filePaths) 
        {
            foreach (string path in filePaths) 
            {
                ReadFile(path);
            }
        }

        public void ReadFile(string path) 
        {
            String line;
            string curResource = "";
            string curProp = "";

            int x = 0;

            // Lists off supported models below, currently only patient supported
            List<Patient> patients = new List<Patient>();

            // Read from given file
            try
            {
                StreamReader sr = new StreamReader(path);

                line = sr.ReadLine();

                //Continue to read until you reach end of file
                while (line != null)
                {
                    if (!string.IsNullOrEmpty(curResource))
                    {
                        switch (curResource) 
                        {
                            case "patient":
                                Patient pat = patients[patients.Count - 1];
                                switch (line) 
                                {
                                    case var i when line.Contains("\"telecom\":"):
                                        curProp = "telecom";
                                        break;
                                    case var i when line.Contains("\"address\":"):
                                        curProp = "address";
                                        break;
                                    case var i when line.Contains("\"maritalStatus\":"):
                                        curProp = "maritalStatus";
                                        break;
                                    case var i when line.Contains("\"language\":"):
                                        curProp = "language";
                                        break;
                                }
                                // Extract and assign data from the given line 
                                AddPatientData(pat, line, curProp);
                                break;
                        }
                    }

                    if (line.Contains("resourceType")) 
                    {
                        // Check resource type
                        switch (line)
                        {
                            case var i when line.Contains("Patient"):
                                Patient pat = new Patient();
                                patients.Add(pat);
                                curResource = "patient";
                                break;
                        }
                    }

                    //Read the next line
                    line = sr.ReadLine();

                    if (Equals(curProp, "language"))
                    {
                        x++;
                        if (x == 7)
                        {
                            x = 0;
                            curProp = "";
                        }
                    }
                }

                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }

            if(patients.Count > 0) WriteFile(patients);
        }

        public void WriteFile(List<Patient> pat) 
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(Patient));

            DataTable dT = new DataTable();

            foreach (PropertyDescriptor p in props)
                dT.Columns.Add(p.Name, p.PropertyType);

            try 
            {
                Random r = new Random();
                string path = Path.Combine($"{Directory.GetCurrentDirectory()}/wwwroot/data", "output" + r.Next(1, 100));
                StreamWriter sw = new StreamWriter(path, true);

                foreach (Patient p in pat) 
                {
                    sw.WriteLine("***   START   ***");
                    for (int i = 0; i < dT.Columns.Count; i++)
                    {
                        string name = dT.Columns[i].ColumnName;

                        if (Equals(name, "Address") || Equals(name, "Telecom"))
                        {
                            List<Dictionary<string,string>> prop;

                            if (Equals(name, "Address"))
                                prop = p.Address;
                            else
                                prop = p.Telecom;

                            sw.WriteLine(dT.Columns[i].ColumnName);
                            foreach (var d in prop)
                            {
                                foreach (var kvp in d)
                                {
                                    sw.WriteLine($" {kvp.Key}: {kvp.Value}");
                                }
                            }
                        }
                        else if (Equals(name, "Communication")) 
                        {
                            sw.WriteLine(dT.Columns[i].ColumnName);
                            foreach (var languange in p.Communication) 
                            {
                                sw.WriteLine($" Language: {languange}");
                            }
                        }
                        else
                        {
                            sw.WriteLine($"{name}: {p.GetType().GetProperty(name).GetValue(p, null)}");
                        }
                    }
                    sw.WriteLine("***   END ***");
                }

                sw.Flush();
                sw.Close();

                Console.WriteLine("Output file in: " + path);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
    

        public void AddPatientData(Patient pat, string line, string curProp) 
        {
            switch (line) 
            {
                case var i when line.Contains("\"family\""): 
                    pat.Surname = ProcessValue(line, null);
                    Console.WriteLine("Surname : " + pat.Surname);
                    break;
                case var i when line.Contains("\"given\""):
                    pat.Name = ProcessValue(line, null);
                    Console.WriteLine("Name : " + pat.Name);
                    break;
                case var i when line.Contains("\"prefix\""):
                    pat.Prefix = ProcessValue(line, null);
                    Console.WriteLine("Prefix : " + pat.Prefix);
                    break;
                case var i when Equals(curProp,"telecom") && line.Contains("\"system\""):
                    pat.Telecom[pat.Telecom.Count - 1]["system"] = ProcessValue(line, null);
                    Console.WriteLine("System : " + pat.Telecom[pat.Telecom.Count - 1]["system"]);
                    break;
                case var i when Equals(curProp, "telecom") && line.Contains("\"value\""):
                    pat.Telecom[pat.Telecom.Count - 1]["value"] = ProcessValue(line, null);
                    Console.WriteLine("Value : " + pat.Telecom[pat.Telecom.Count - 1]["value"]);
                    break;
                case var i when Equals(curProp, "telecom") && line.Contains("\"use\""):
                    pat.Telecom[pat.Telecom.Count - 1]["use"] = ProcessValue(line, null);
                    Console.WriteLine("Use : " + pat.Telecom[pat.Telecom.Count - 1]["use"]);
                    break;
                case var i when line.Contains("\"gender\""):
                    pat.Gender = ProcessValue(line, null);
                    Console.WriteLine("Gender : " + pat.Gender);
                    break;
                case var i when line.Contains("\"birthDate\""):
                    pat.DOB = ProcessDateTimeValue(line, "DOB");
                    Console.WriteLine("DOB : " + pat.DOB);
                    break;
                case var i when line.Contains("\"deceasedDateTime\""):
                    pat.DeceasedDateTime = ProcessDateTimeValue(line, "deceased");
                    Console.WriteLine("Deceased : " + pat.DeceasedDateTime);
                    break;
                case var i when Equals(curProp, "address") && line.Contains("\"line\""):
                    pat.Address[pat.Address.Count - 1]["line"] = ProcessValue(line, "address line");
                    Console.WriteLine("Line : " + pat.Address[pat.Address.Count - 1]["line"]);
                    break;
                case var i when Equals(curProp, "address") && line.Contains("\"city\""):
                    pat.Address[pat.Address.Count - 1]["city"] = ProcessValue(line, null);
                    Console.WriteLine("City : " + pat.Address[pat.Address.Count - 1]["city"]);
                    break;
                case var i when Equals(curProp, "address") && line.Contains("\"state\""):
                    pat.Address[pat.Address.Count - 1]["state"] = ProcessValue(line, null);
                    Console.WriteLine("State : " + pat.Address[pat.Address.Count - 1]["state"]);
                    break;
                case var i when Equals(curProp, "address") && line.Contains("\"country\""):
                    pat.Address[pat.Address.Count - 1]["country"] = ProcessValue(line, null);
                    Console.WriteLine("Country : " + pat.Address[pat.Address.Count - 1]["country"]);
                    break;
                case var i when Equals(curProp, "maritalStatus") && line.Contains("\"text\""):

                    pat.MaritalStatus = char.Parse(ProcessValue(line, null));
                    Console.WriteLine("MaritalStatus : " + pat.MaritalStatus);
                    break;
                case var i when line.Contains("\"multipleBirthBoolean\""):
                    pat.MultipleBirth = bool.Parse(ProcessValue(line, null));
                    Console.WriteLine("MultipleBirth : " + pat.MultipleBirth);
                    break;
                case var i when Equals(curProp, "language") && line.Contains("\"text\""):
                    pat.Communication.Add(ProcessValue(line, null));
                    Console.WriteLine("Language : " + pat.Communication[0]);
                    break;
            }
        }

        public string ProcessValue(string line, string specialType) 
        {
            char[] chars = line.Trim().ToCharArray();
            string processedLine = "";
            foreach (char c in chars) 
            {
                if (c == '"' || c == ',' || c == '[' || c == ']') { }
                else processedLine += c;
            }
            processedLine = processedLine.Trim();

            string[] words = processedLine.Split(" ");

            string output = words[words.Length - 1];

            if (specialType != null) 
            {
                output = "";
                switch (specialType)
                {
                    case "address line":
                        for (int i = 1; i < words.Length; i++)
                        {
                            output += words[i] + " ";
                        }
                        break;
                }
            }

            return output.Trim();
        }

        public DateTime ProcessDateTimeValue(string line, string type) 
        {
            DateTime dateTime = new DateTime();

            string year = "";
            string month = "";
            string day = "";

            string hour = "";
            string min = "";
            string sec = "";

            string timeZone = "";

            char[] chars = line.Trim().ToCharArray();
            string processedLine = "";
            foreach (char c in chars)
            {
                if (c == '-' || c == ',' || c == ':' || c == 'T' || c == '"') { }
                else processedLine += c;
            }
            processedLine = processedLine.Trim();

            string[] words = processedLine.Split(" ");

            char[] dateT = words[words.Length - 1].ToCharArray();

            for (int i = 0; i < 8; i++)
            {
                if (i < 4)
                    year += dateT[i];
                else if (i >= 4 && i < 6)
                    month += dateT[i];
                else if (i >= 6 && i < 8)
                    day += dateT[i];
            }

            if (Equals(type, "DOB"))
                dateTime = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
            if (Equals(type, "deceased")) 
            {
                for (int i = 8; i < 14; i++)
                {
                    if (i >= 8 && i < 10)
                        hour += dateT[i];
                    else if (i >= 10 && i < 12)
                        min += dateT[i];
                    else
                        sec += dateT[i];
                }

                dateTime = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day),
                    int.Parse(hour), int.Parse(min), int.Parse(sec), DateTimeKind.Utc);
            }
              
            return dateTime;
        }
    }
}
