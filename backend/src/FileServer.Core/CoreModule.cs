using Autofac;
using FileServer.Core.Services;

namespace FileServer.Core
{
  public class CoreModule: Module
  {
    protected override void Load(ContainerBuilder builder)
    {
      builder.RegisterType<FileService>().As<IFileService>();
    }
  }
}
