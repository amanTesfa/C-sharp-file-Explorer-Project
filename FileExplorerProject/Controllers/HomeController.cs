
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FileExplorerProject.Models;

namespace FileExplorerProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DownloadZip()
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
                return NotFound("No uploaded files found.");

            var files = Directory.GetFiles(uploadPath);
            if (files.Length == 0)
                return NotFound("No files to download.");

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var filePath in files)
                    {
                        var fileName = Path.GetFileName(filePath);
                        var entry = archive.CreateEntry(fileName);
                        using (var entryStream = entry.Open())
                        using (var fileStream = System.IO.File.OpenRead(filePath))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
                memoryStream.Position = 0;
                return File(memoryStream.ToArray(), "application/zip", "uploads.zip");
            }
        }

        [HttpPost]
        public IActionResult Upload(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return Content("No files uploaded.");

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                var filePath = Path.Combine(uploadPath, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            return Content($"Uploaded {files.Count} file(s) successfully.");
        }
        [HttpDelete]
        public IActionResult DeleteFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Json(new { success = false, error = "No file specified." });
            try
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var filePath = Path.Combine(uploadPath, fileName);
                if (!System.IO.File.Exists(filePath))
                    return Json(new { success = false, error = "File not found." });
                System.IO.File.Delete(filePath);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult ListFiles()
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
                return Json(new string[0]);
            var files = Directory.GetFiles(uploadPath)
                .Select(Path.GetFileName)
                .ToArray();
            return Json(files);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}