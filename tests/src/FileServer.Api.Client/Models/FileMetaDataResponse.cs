using System;

namespace FileServer.Api.Client.Models
{
  public class FileMetaDataResponse
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