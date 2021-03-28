using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FileServer.Api.Client.Models;
using Refit;

namespace FileServer.Api.Client
{
  public interface IFilesApiV1
  {
    [Multipart]
    [Post("/v1/files")]
    Task<string> UploadFile([AliasAs("file")] ByteArrayPart file, [Header("Authorization")] string apiKey);

    [Get("/v1/files/metaData")]
    Task<IReadOnlyCollection<FileMetaDataResponse>> GetAllFilesMetaData([Query] PageRequest page, [Header("Authorization")] string apiKe);

    [Get("/v1/files/{id}")]
    Task<HttpContent> DownloadFile( string id, [Header("Authorization")] string apiKe);

    [Get("/v1/files/{id}/metaData")]
    Task<FileMetaDataResponse> GetFileMetaData(string id, [Header("Authorization")] string apiKe);

    [Delete("/v1/files/{id}")]
    Task DeleteFile(string id, [Header("Authorization")] string apiKe);
  }

}
