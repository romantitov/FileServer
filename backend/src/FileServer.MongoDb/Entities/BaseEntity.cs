﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileServer.MongoDb.Entities
{
  public abstract class BaseEntity
  {
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }
  }
}
