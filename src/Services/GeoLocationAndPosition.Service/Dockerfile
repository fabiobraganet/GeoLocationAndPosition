#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Services/GeoLocationAndPosition.Service/GeoLocationAndPosition.Service.csproj", "src/Services/GeoLocationAndPosition.Service/"]
COPY ["src/Libraries/GeoLocationAndPosition.Application/GeoLocationAndPosition.Application.csproj", "src/Libraries/GeoLocationAndPosition.Application/"]
COPY ["src/Libraries/GeoLocationAndPosition.DTO/GeoLocationAndPosition.DTO.csproj", "src/Libraries/GeoLocationAndPosition.DTO/"]
COPY ["src/Libraries/GeoLocationAndPosition.Data/GeoLocationAndPosition.Data.csproj", "src/Libraries/GeoLocationAndPosition.Data/"]
COPY ["src/Libraries/GeoLocationAndPosition.Domain/GeoLocationAndPosition.Domain.csproj", "src/Libraries/GeoLocationAndPosition.Domain/"]
RUN dotnet restore "src/Services/GeoLocationAndPosition.Service/GeoLocationAndPosition.Service.csproj"
COPY . .
WORKDIR "/src/src/Services/GeoLocationAndPosition.Service"
RUN dotnet build "GeoLocationAndPosition.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GeoLocationAndPosition.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GeoLocationAndPosition.Service.dll"]