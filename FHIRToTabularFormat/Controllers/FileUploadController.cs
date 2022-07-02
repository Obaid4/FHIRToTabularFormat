using FHIRToTabularFormat.Models;
using Microsoft.AspNetCore.Mvc;

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

            try
            {
                StreamReader sr = new StreamReader(path);

                line = sr.ReadLine();

                // Lists off supported models below, currently only patient supported
                List<Patient> patients = new List<Patient>();

                //Continue to read until you reach end of file
                while (line != null)
                {
                    if (!string.IsNullOrEmpty(curResource))
                    {
                        switch (curResource) 
                        {
                            case "patient":
                                Patient pat = patients[patients.Count - 1];
                                // Extract and assign data from the given line
                                AddPatientData(pat, line);
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
                                //resourceFound = true;
                                curResource = "patient";
                                break;
                        }
                    }    

                    //Read the next line
                    line = sr.ReadLine();
                }

                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        public void AddPatientData(Patient pat, string line) 
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
                //case var i when line.Contains("\"system\""):
                //    pat.Telecom[pat.Telecom.Count - 1]["system"] = ProcessValue(line);
                //    Console.WriteLine("Assigned : " + pat.Telecom[pat.Telecom.Count - 1]["system"]);
                //    break;
                case var i when line.Contains("\"gender\""):
                    pat.Gender = ProcessValue(line, null);
                    Console.WriteLine("Gender : " + pat.Gender);
                    break;
                case var i when line.Contains("\"line\""):
                    pat.Address[pat.Address.Count - 1]["line"] = ProcessValue(line, "address line");
                    Console.WriteLine("Line : " + pat.Address[pat.Address.Count - 1]["line"]);
                    break;
                case var i when line.Contains("\"city\""):
                    pat.Address[pat.Address.Count - 1]["city"] = ProcessValue(line, null);
                    Console.WriteLine("City : " + pat.Address[pat.Address.Count - 1]["city"]);
                    break;
                case var i when line.Contains("\"state\""):
                    pat.Address[pat.Address.Count - 1]["state"] = ProcessValue(line, null);
                    Console.WriteLine("State : " + pat.Address[pat.Address.Count - 1]["state"]);
                    break;
                case var i when line.Contains("\"country\""):
                    pat.Address[pat.Address.Count - 1]["country"] = ProcessValue(line, null);
                    Console.WriteLine("Country : " + pat.Address[pat.Address.Count - 1]["country"]);
                    break;
                //case var i when line.Contains("\"maritalStatus\""):
                //    pat.MaritalStatus = char.Parse(ProcessValue(line, null));
                //    Console.WriteLine("MaritalStatus : " + pat.MaritalStatus);
                //    break;
                case var i when line.Contains("\"multipleBirthBoolean\""):
                    pat.MultipleBirth = bool.Parse(ProcessValue(line, null));
                    Console.WriteLine("MultipleBirth : " + pat.MultipleBirth);
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

            return output;
        }
    }
}
