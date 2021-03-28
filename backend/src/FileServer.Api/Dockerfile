#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["FileServer.Api/FileServer.Api.csproj", "FileServer.Api/"]
COPY ["FileServer.Binary/FileServer.Binary.csproj", "FileServer.Binary/"]
COPY ["FileServer.Core/FileServer.Core.csproj", "FileServer.Core/"]
COPY ["FileServer.MongoDb/FileServer.MongoDb.csproj", "FileServer.MongoDb/"]
RUN dotnet restore "FileServer.Api/FileServer.Api.csproj"
COPY . .
WORKDIR "/src/FileServer.Api"
RUN dotnet build "FileServer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileServer.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileServer.Api.dll"]