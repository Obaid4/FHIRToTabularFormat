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

            return Ok(new { files.Count, size, filePaths});
        }
    }
}
