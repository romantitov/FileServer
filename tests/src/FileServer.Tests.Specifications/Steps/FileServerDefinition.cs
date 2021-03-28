using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FileServer.Api.Client;
using FileServer.Api.Client.Models;
using FileServer.Tests.Specifications.Drivers;
using FileServer.Tests.Specifications.Utils;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Refit;
using TechTalk.SpecFlow;

namespace FileServer.Tests.Specifications.Steps
{
  [Binding]
  public class FileServerDefinition
  {
    private readonly IFilesApiV1 _filesApiV1;
    private readonly IConfiguration _configuration;
    private readonly FileServerContext _fileServerContext;

    public FileServerDefinition(
      IFilesApiV1 filesApiV1, 
      IConfiguration configuration,
      FileServerContext fileServerContext)
    {
      _filesApiV1 = filesApiV1;
      _configuration = configuration;
      _fileServerContext = fileServerContext;
    }

    [Given(@"for an owner with key (\d) has no uploaded fields")]
    public async Task GivenForOwnerWithKeyHasNoUploadedFields(int keyId)
    {
      var apiKey = _configuration.GetFileServerApiKey(keyId);
      var offset = 0;
      var fileItems = 0;
      do
      {
        var metaDataItems = await _filesApiV1.GetAllFilesMetaData(new PageRequest
        {
          Limit = 100,
          Offset = offset
        }, apiKey);
        fileItems = metaDataItems.Count;

        foreach (var fileId in metaDataItems.Select(x=>x.Id))
        {
          try
          {
            await _filesApiV1.DeleteFile(fileId, apiKey);
          }
          catch (ApiException apiException)
          {
            if (apiException.StatusCode != HttpStatusCode.NotFound)
            {
              throw;
            }
          }
        }

      } while (fileItems >0);
     
    }


    [When(@"for an owner with key (\d) uploads the file '(.*)'")]
    [Given(@"for an owner with key (\d) uploads the file '(.*)'")]
    public async Task GivenForOwnerWithKeyUploadsTheFile(int keyId, string fileName)
    {
      var apiKey = _configuration.GetFileServerApiKey(keyId);
      await using var fileStream = File.OpenRead(fileName);
      await using var ms = new MemoryStream();
      await fileStream.CopyToAsync(ms);
      var fileId = await _filesApiV1.UploadFile(new ByteArrayPart(ms.ToArray(), fileName), apiKey);
      _fileServerContext.FileId = fileId;
    }


    [Then(@"for an owner with key (\d) file server contains (\d) files")]
    [Then(@"for an owner with key (\d) file server contains (\d) file")]
    public async Task ThenForKeyFileServerContainsFiles(int keyId, int expectedAmountOfItems)
    {
       var apiKey =  _configuration.GetFileServerApiKey(keyId);

      var metaDataItems = await _filesApiV1.GetAllFilesMetaData(new PageRequest
       {
         Limit = expectedAmountOfItems + 10
       }, apiKey);

      metaDataItems.Should().HaveCount(expectedAmountOfItems);
    }


    [Then(@"the file uploaded by owner with key (\d) is equal to '(.*)'")]
    public async Task ThenTheFileUploadedByOwnerWithKeyHasHashEqualTo(int keyId, string fileName)
    {
      _fileServerContext.FileId.Should().NotBeNullOrEmpty();
      var apiKey = _configuration.GetFileServerApiKey(keyId);
      var metaData = await _filesApiV1.GetFileMetaData(_fileServerContext.FileId, apiKey);
      metaData.Id.Should().Be(_fileServerContext.FileId);
      metaData.OwnerId.Should().Be(apiKey.Replace("ApiKey ", string.Empty));
      metaData.FileName.Should().Be(Path.GetFileName(fileName));


      using var md5 = MD5.Create();
      await using var testFileStream = File.OpenRead(fileName);
      metaData.FileSize.Should().Be(testFileStream.Length);
      var testFileHashString = BitConverter.ToString(await md5.ComputeHashAsync(testFileStream)).Replace("-", "").ToLowerInvariant();

      var fileRequest = await _filesApiV1.DownloadFile(_fileServerContext.FileId, apiKey);
      var uploadedFileStream = await  fileRequest.ReadAsStreamAsync();
      var uploadedFileHashString = BitConverter.ToString(await md5.ComputeHashAsync(uploadedFileStream)).Replace("-", "").ToLowerInvariant();
      testFileHashString.Should().Be(uploadedFileHashString);

    }

    [Then(@"for an owner with key (\d) requested the file meta data has '(.*)' status code")]
    public async Task ThenForAnOwnerWithKeyRequestedTheFileMetaDataHasStatusCode(int keyId, int statusCode)
    {
      _fileServerContext.FileId.Should().NotBeNullOrEmpty();
      var apiKey = _configuration.GetFileServerApiKey(keyId);
      try
      {
        await _filesApiV1.GetFileMetaData(_fileServerContext.FileId, apiKey);
      }
      catch (ApiException apiException)
      {
        apiException.StatusCode.Should().Be((HttpStatusCode) statusCode);
      }
    }

    [Then(@"for an owner with unknown key requested the file meta data has '(.*)' status code")]
    public async Task ThenForAnOwnerWithUnknownKeyRequestedTheFileMetaDataHasStatusCode(int statusCode)
    {
      var apiKey = "unknown key";
      try
      {
        await _filesApiV1.GetFileMetaData(_fileServerContext.FileId, apiKey);
      }
      catch (ApiException apiException)
      {
        apiException.StatusCode.Should().Be((HttpStatusCode)statusCode);
      }
    }



    [Then(@"for an owner with key (\d) requested the file has '(.*)' status code")]
    public async Task ThenForAnOwnerWithKeyRequestedTheFileHasStatusCode(int keyId, int statusCode)
    {
      _fileServerContext.FileId.Should().NotBeNullOrEmpty();
      var apiKey = _configuration.GetFileServerApiKey(keyId);
      try
      {
        await _filesApiV1.DownloadFile(_fileServerContext.FileId, apiKey);
      }
      catch (ApiException apiException)
      {
        apiException.StatusCode.Should().Be((HttpStatusCode)statusCode);
      }
    }

    [When(@"for an owner with key (\d) removed the file")]
    public async Task WhenForAnOwnerWithKeyRemovedTheFile(int keyId)
    {
      _fileServerContext.FileId.Should().NotBeNullOrEmpty();
      var apiKey = _configuration.GetFileServerApiKey(keyId);
      await _filesApiV1.DeleteFile(_fileServerContext.FileId, apiKey);
    }

  }
}
