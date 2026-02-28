# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution, build config, and project files for layer-cached restore
COPY AppTemplate.slnx .
COPY nuget.config .
COPY global.json .
COPY Directory.Build.props .
COPY src/AppTemplate.Api/AppTemplate.Api.csproj src/AppTemplate.Api/
COPY src/AppTemplate.Web/AppTemplate.Web.csproj src/AppTemplate.Web/
RUN dotnet restore

# Copy source code and publish
COPY src/ src/
RUN dotnet publish src/AppTemplate.Api/AppTemplate.Api.csproj -c Release -o /app/api --no-restore
RUN dotnet publish src/AppTemplate.Web/AppTemplate.Web.csproj -c Release -o /app/web --no-restore

# API runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app
COPY --from=build /app/api .
EXPOSE 5050
ENV ASPNETCORE_URLS=http://+:5050
ENTRYPOINT ["dotnet", "AppTemplate.Api.dll"]

# Web runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS web
WORKDIR /app
COPY --from=build /app/web .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AppTemplate.Web.dll"]
