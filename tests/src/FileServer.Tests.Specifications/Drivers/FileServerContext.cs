using System.Collections.Generic;
using TechTalk.SpecFlow;

namespace FileServer.Tests.Specifications.Drivers
{
  public class FileServerContext
  {
    private readonly ScenarioContext _scenarioContext;
    private const string FileIdKey = "FileId";
    public FileServerContext(ScenarioContext scenarioContext)
    {
      _scenarioContext = scenarioContext;
    }

    public string FileId
    {
      set => _scenarioContext.Set(value, FileIdKey);
      get => _scenarioContext.GetValueOrDefault(FileIdKey, null) as string;
    }
  }
}
