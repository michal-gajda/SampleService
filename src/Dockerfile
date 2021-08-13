FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY CompanyName.SampleService.sln CompanyName.SampleService.sln
COPY CompanyName.SampleService.Application/CompanyName.SampleService.Application.csproj CompanyName.SampleService.Application/CompanyName.SampleService.Application.csproj
COPY CompanyName.SampleService.Domain/CompanyName.SampleService.Domain.csproj CompanyName.SampleService.Domain/CompanyName.SampleService.Domain.csproj
COPY CompanyName.SampleService.Infrastructure/CompanyName.SampleService.Infrastructure.csproj CompanyName.SampleService.Infrastructure/CompanyName.SampleService.Infrastructure.csproj
COPY CompanyName.SampleService.WebApi/CompanyName.SampleService.WebApi.csproj CompanyName.SampleService.WebApi/CompanyName.SampleService.WebApi.csproj
RUN dotnet restore

COPY . .

RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app

RUN apt-get update && apt-get install -y curl=7.64.0-4+deb10u2 --no-install-recommends && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

RUN groupadd -g 10000 dotnet && useradd -u 10000 -g dotnet dotnet && chown -R dotnet:dotnet /app
USER dotnet:dotnet

ENV ASPNETCORE_URLS http://*:5080
EXPOSE 5080

HEALTHCHECK --interval=30s --timeout=10s --retries=3 CMD curl --fail http://localhost:5080/health || exit 1

ENTRYPOINT ["dotnet", "CompanyName.SampleService.WebApi.dll"]