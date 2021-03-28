using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileServer.Core.Infrastructure
{
  public interface IFileMetaDataRepository
  {
    Task<FileMetaDataDto> GetFileMeteData(string id);
    Task<string> CreateFileMetaData(FileMetaDataDto fileMetaData);
    Task DeleteFileMetaData(string id);
    Task UpdateFileMetaData(FileMetaDataDto fileMetaData);
  }

  public interface IFileMetaDataReadOnlyRepository
  {
    Task<IReadOnlyCollection<FileMetaDataDto>> GetFileMetaDates(string ownerId, PageDto pageDto);
  }

  public class PageDto
  {
    public int Limit { get; set; }

    public int Offset { get; set; }
  }
  public class FileMetaDataDto
  {
    public string Id { get; set; }
    public string DocumentId { get; set; }
    public string FileName { get; set; }
    public DateTimeOffset UploadDate { get; set; }
    public DateTimeOffset LastAccessedDate { get; set; }
    public string OwnerId { get; set; }
    public long FileSize { get; set; }
  }
}
