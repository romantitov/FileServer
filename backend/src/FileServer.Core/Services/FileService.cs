using System;
using System.IO;
using System.Threading.Tasks;
using FileServer.Core.Exceptions;
using FileServer.Core.Infrastructure;

namespace FileServer.Core.Services
{
  public interface IFileService
  {
    Task<string> UploadFile(string userId, Stream fileStream, string fileName, long fileLength);
    Task<FileMetaDataDto> GetFileInfo(string userId, string id);
    Task<FileDataResponse> DownloadFile(string userId, string id);
    Task DeleteFile(string userId, string id);
  }

  public class FileService : IFileService 
  {
    private readonly IFileMetaDataRepository _fileMetaDataRepository;
    private readonly IFileStorage _fileStorage;

    public FileService(
      IFileMetaDataRepository fileMetaDataRepository, 
      IFileStorage fileStorage)
    {
      _fileMetaDataRepository = fileMetaDataRepository;
      _fileStorage = fileStorage;
    }


    public async Task<string> UploadFile(string userId, Stream fileStream, string fileName, long fileLength)
    {
      fileName = Path.GetFileName(fileName);
      var documentId = Guid.NewGuid().ToString("N");
      await _fileStorage.UploadFile(documentId, fileStream);
      return await _fileMetaDataRepository.CreateFileMetaData(new FileMetaDataDto
      {
        FileName = fileName,
        FileSize = fileLength,
        OwnerId = userId,
        LastAccessedDate = DateTimeOffset.UtcNow,
        DocumentId = documentId,
        UploadDate = DateTimeOffset.UtcNow,
      });
    }

    public async Task<FileMetaDataDto> GetFileInfo(string userId, string id)
    {
      var fileInfo = await _fileMetaDataRepository.GetFileMeteData(id);
      if (fileInfo == null)
      {
        throw new ResourceNotFoundException(ErrorCodes.NoMetaDataFound, "Metadata for requested file was not found");
      }

      if (fileInfo.OwnerId != userId)
      {
        throw new AccessNotAllowedException(ErrorCodes.AccessDoesNotAllowed, "Access to requested resource does not allowed");
      }

      return fileInfo;
    }

    public async Task<FileDataResponse> DownloadFile(string userId, string id)
    {
      var fileInfo = await GetFileInfo(userId, id);
      if (!await _fileStorage.IsFileExist(fileInfo.DocumentId))
      {
        throw new ResourceNotFoundException(ErrorCodes.NoFileFound, "Requested file was not found");
      }

      var stream = await _fileStorage.DownloadFile(fileInfo.DocumentId);
      fileInfo.LastAccessedDate = DateTimeOffset.UtcNow;
      await _fileMetaDataRepository.UpdateFileMetaData(fileInfo);
      return new FileDataResponse
      {
        MetaData = fileInfo,
        FileStream = stream
      };
    }

    public async Task DeleteFile(string userId, string id)
    {
      var fileInfo = await GetFileInfo(userId, id);
      if (!await _fileStorage.IsFileExist(fileInfo.DocumentId))
      {
        throw new ResourceNotFoundException(ErrorCodes.NoFileFound, "Requested file was not found");
      }

      await _fileMetaDataRepository.DeleteFileMetaData(id);
      await _fileStorage.DeleteFile(fileInfo.DocumentId);
    }
  }

  public class FileDataResponse
  {
    public FileMetaDataDto MetaData { get; set; }

    public Stream FileStream { get; set; }
  }
}
