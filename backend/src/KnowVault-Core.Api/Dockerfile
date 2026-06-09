FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . ./
RUN dotnet restore KnowVault-Core.slnx
RUN dotnet publish src/KnowVault-Core.Api/KnowVault-Core.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app .

ENTRYPOINT ["dotnet", "KnowVault-Core.Api.dll"]

