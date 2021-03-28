using System.IO;
using System.Threading.Tasks;

namespace FileServer.Core.Infrastructure
{
  public interface IFileStorage
  {
    Task UploadFile(string documentId, Stream fileStream);

    Task<Stream> DownloadFile(string documentId);

    Task<bool> IsFileExist(string documentId);
    Task DeleteFile(string documentId);
  }
}
