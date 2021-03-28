using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FileServer.Core.Infrastructure;

namespace FileServer.Binary
{
  public class FileStorage: IFileStorage
  {

    private const string FileDirectory = "Files";

    public FileStorage()
    {
      if (!Directory.Exists(FileDirectory))
      {
        Directory.CreateDirectory(FileDirectory);
      }
    }

    public async Task UploadFile(string documentId, Stream fileStream)
    {
      using (var file = File.Create(Path.Combine(FileDirectory, documentId)))
      {
        await fileStream.CopyToAsync(file);
      }
    }

    public Task<Stream> DownloadFile(string documentId)
    {
      return Task.FromResult(File.OpenRead(Path.Combine(FileDirectory, documentId)) as Stream);
    }

    public Task<bool> IsFileExist(string documentId)
    {
      return Task.FromResult(File.Exists(Path.Combine(FileDirectory, documentId)));
    }

    public Task DeleteFile(string documentId)
    {
      File.Delete(Path.Combine(FileDirectory, documentId));
      return Task.CompletedTask;
    }
  }
}
