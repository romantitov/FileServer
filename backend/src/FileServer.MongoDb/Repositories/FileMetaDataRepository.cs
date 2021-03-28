using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FileServer.Core.Infrastructure;
using FileServer.MongoDb.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FileServer.MongoDb.Repositories
{
  public class FileMetaDataRepository : IFileMetaDataRepository, IFileMetaDataReadOnlyRepository
  {
    private readonly IMongoCollection<FileMetaDataEntity> _collection;
    private readonly IMapper _mapper;
    public FileMetaDataRepository(IMongoDatabase mongoDatabase)
    {
      _collection = mongoDatabase.GetCollection<FileMetaDataEntity>(FileMetaDataEntity.CollectionName);
      var config = new MapperConfiguration(cfg =>
      {
        cfg.CreateMap<FileMetaDataEntity, FileMetaDataDto>()
          .ForMember(x=>x.Id, opt=>opt.MapFrom(y=>y.Id)).ReverseMap();
      });
      _mapper = config.CreateMapper();
    }

    public async Task<FileMetaDataDto> GetFileMeteData(string id)
    {
      var cursor = await _collection.FindAsync(x => x.Id == id);
      var entity = await cursor.FirstOrDefaultAsync();
      return _mapper.Map<FileMetaDataDto>(entity);
    }

    public async Task<string> CreateFileMetaData(FileMetaDataDto fileMetaData)
    {
      var entity = _mapper.Map<FileMetaDataEntity>(fileMetaData);
      entity.Id = ObjectId.GenerateNewId().ToString();
      await _collection.InsertOneAsync(entity);
      return entity.Id;
    }

    public async Task UpdateFileMetaData(FileMetaDataDto fileMetaData)
    {
      await _collection.ReplaceOneAsync(x => x.Id == fileMetaData.Id, _mapper.Map<FileMetaDataEntity>(fileMetaData));
    }

    public async Task<IReadOnlyCollection<FileMetaDataDto>> GetFileMetaDates(string ownerId, PageDto pageDto)
    {
      var query =  _collection.Find(x => x.OwnerId == ownerId);
      var entities = await query.Skip(pageDto.Offset).Limit(pageDto.Limit).ToListAsync();
      return _mapper.Map<IReadOnlyCollection<FileMetaDataDto>>(entities);
    }

    public async Task DeleteFileMetaData(string id)
    {
      await _collection.DeleteOneAsync(x => x.Id == id);
    }
  }
}
