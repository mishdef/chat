using Microsoft.AspNetCore.Http;

namespace SignalRProject.Interfaces
{
    public interface IImageService
    {
        Task<string> SaveAndCompressImageAsync(IFormFile file, string subDirectory, int maxWidth, int maxHeight, bool cropToSquare = false);
    }
}
