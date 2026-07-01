FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/HotelSearch.Domain/HotelSearch.Domain.csproj src/HotelSearch.Domain/
COPY src/HotelSearch.Application/HotelSearch.Application.csproj src/HotelSearch.Application/
COPY src/HotelSearch.Infrastructure/HotelSearch.Infrastructure.csproj src/HotelSearch.Infrastructure/
COPY src/HotelSearch.Api/HotelSearch.Api.csproj src/HotelSearch.Api/

RUN dotnet restore src/HotelSearch.Api/HotelSearch.Api.csproj

COPY src/ src/
RUN dotnet publish src/HotelSearch.Api/HotelSearch.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HotelSearch.Api.dll"]
