using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using FileServer.Api.Extensions;
using FileServer.Api.Middleware.Caching;
using FileServer.Api.Middleware.Logging;
using FileServer.Api.Models;
using FileServer.Core.Infrastructure;
using FileServer.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace FileServer.Api.Controllers
{
  [Authorize]
  [ApiController]
  [ApiVersion("1.0")]
  [Route("api/v{version:apiVersion}/[controller]")]
  public class FilesController : ControllerBase
  {
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;
    public FilesController(IFileService fileService)
    {
      _fileService = fileService;
      var config = new MapperConfiguration(cfg =>
      {
        cfg.CreateMap<PageRequest, PageDto>();
        cfg.CreateMap<FileMetaDataDto, FileMetaDataResponse>();
      });
      _mapper = config.CreateMapper();
    }

    [Route("")]
    [HttpPost]
    [RequestResponseLogging(LoggingTypes.Responses)]
    public async Task<string> UploadFile([FromForm] FileDataRequest fileDataRequest)
    {
      return await _fileService.UploadFile(User.GetApiKey(),fileDataRequest.File.OpenReadStream(), fileDataRequest.File.FileName, fileDataRequest.File.Length);
    }


    [Route("metaData")]
    [HttpGet]
    [RequestResponseLogging]
    public async Task<IReadOnlyCollection<FileMetaDataResponse>> GetAllFilesMetaData([FromQuery]PageRequest page, [FromServices] IFileMetaDataReadOnlyRepository fileMetaDataReadOnlyRepository)
    {
      return _mapper.Map<IReadOnlyCollection<FileMetaDataResponse>>(await fileMetaDataReadOnlyRepository.GetFileMetaDates(User.GetApiKey(), _mapper.Map<PageDto>(page)));
    }

    [Route("{id}")]
    [HttpGet]
    [ProducesResponseType(typeof(FileContentResult), (int) HttpStatusCode.OK)]
    [RequestResponseLogging(LoggingTypes.Requests)]
    [NoCache]
    public async Task<IActionResult> DownloadFile(string id)
    {
      var fileInfo = await _fileService.DownloadFile(User.GetApiKey(), id);
      return File(fileInfo.FileStream, "application/octet-stream", fileInfo.MetaData.FileName);

    }

    [Route("{id}/metaData")]
    [HttpGet]
    [RequestResponseLogging]
    public async Task<FileMetaDataResponse> GetFileMetaData(string id)
    {
      return _mapper.Map<FileMetaDataResponse>(await _fileService.GetFileInfo(User.GetApiKey(), id));
    }

    [Route("{id}")]
    [HttpDelete]
    [RequestResponseLogging]
    public async Task DeleteFile(string id)
    {
      await _fileService.DeleteFile(User.GetApiKey(), id);
    }

  }
}
