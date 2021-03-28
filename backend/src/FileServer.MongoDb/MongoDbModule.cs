using Autofac;
using FileServer.MongoDb.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FileServer.MongoDb
{
  public class MongoDbModule : Module
  {
    protected override void Load(ContainerBuilder builder)
    {
      builder.Register(context =>
      {
        var config = context.Resolve<IOptions<MongoDbConfigurations>>().Value;
        var c = new MongoClient(config.ConnectionString);
        return c.GetDatabase(config.DatabaseName);

      }).As<IMongoDatabase>().SingleInstance();

      //Repos
      builder.RegisterType<FileMetaDataRepository>().AsImplementedInterfaces();
    }
  }
}
