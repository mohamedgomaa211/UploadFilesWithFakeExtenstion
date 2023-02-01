using Microsoft.AspNetCore.Mvc;
using UploadFilesWithFakeExtenstions.Data;
using UploadFilesWithFakeExtenstions.Models;
using UploadFilesWithFakeExtenstions.ViewModels;

namespace UploadFilesWithFakeExtenstions.Controllers
{
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FilesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var files = _context.UploadedFiles.ToList();
            return View(files);
        }
        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(UploadFileViewModel model)
        {

            List<UploadedFile> uploadedFiles = new ();


            foreach (var file in model.Files)
            {
                var FakeFileName = Path.GetRandomFileName();

                UploadedFile uploadedFile = new ()
                {
                    ContentType=file.ContentType,
                    FileName = file.FileName,
                    StoredFileName= FakeFileName
                };
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", FakeFileName);
                using FileStream fileStream = new (path,FileMode.Create);
                file.CopyTo(fileStream);
                fileStream.Flush();
                uploadedFiles.Add(uploadedFile);

            }
            _context.AddRange(uploadedFiles);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]

        public IActionResult DownloadFile(string fileName)
        {
            var uploadfile = _context.UploadedFiles.SingleOrDefault(x => x.StoredFileName == fileName);
            if (uploadfile is null)
                return NotFound();
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", fileName);

            MemoryStream memoryStream = new MemoryStream();
            using FileStream FileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            return File(memoryStream, uploadfile.ContentType, uploadfile.FileName);
        }
    }
}
