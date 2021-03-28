using System;

namespace FileServer.MongoDb.Entities
{
  public class FileMetaDataEntity : BaseEntity
  {
    public const string CollectionName = "FileMetaDatas";
    public string DocumentId { get; set; }
    public string FileName { get; set; }
    public DateTimeOffset UploadDate { get; set; }
    public DateTimeOffset LastAccessedDate { get; set; }
    public string OwnerId { get; set; }
    public long FileSize { get; set; }
  }
}