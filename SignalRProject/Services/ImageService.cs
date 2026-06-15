using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SignalRProject.Interfaces;

namespace SignalRProject.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ImageService(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<string> SaveAndCompressImageAsync(IFormFile file, string subDirectory, int maxWidth, int maxHeight, bool cropToSquare = false)
        {
            if (file == null || file.Length == 0) return "";

            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot");
            var uploadsPath = Path.Combine(webRootPath, "uploads", subDirectory);
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var fileName = Guid.NewGuid().ToString() + ".jpg";
            var filePath = Path.Combine(uploadsPath, fileName);

            using var imageStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(imageStream);

            if (cropToSquare)
            {
                var minSize = Math.Min(image.Width, image.Height);
                image.Mutate(x => x.Crop(new Rectangle((image.Width - minSize) / 2, (image.Height - minSize) / 2, minSize, minSize)));
            }

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, maxHeight),
                Mode = ResizeMode.Max
            }));

            await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 80 });

            return fileName;
        }
    }
}
