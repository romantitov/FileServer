using Autofac;
using FileServer.Core.Infrastructure;

namespace FileServer.Binary
{
  public class BinaryModule: Module
  {
    protected override void Load(ContainerBuilder builder)
    {
      builder.RegisterType<FileStorage>().As<IFileStorage>();
    }
  }
}
