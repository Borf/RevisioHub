# syntax=docker/dockerfile:1
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG TARGETARCH
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./ ./
RUN ls
RUN find /app
RUN dotnet restore

# Copy everything else and build
RUN dotnet publish -c Release RevisioHub.Web
RUN dotnet publish RevisioHub.Client /p:PublishProfile=linux-arm64
RUN dotnet publish RevisioHub.Client /p:PublishProfile=win-x64
RUN find /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/RevisioHub.Web/bin/Release/net8.0/publish  .
COPY --from=build-env /app/RevisioHub.Web/appsettings.Development.json  .
COPY --from=build-env /app/RevisioHub.Web/wwwroot wwwroot
ENTRYPOINT ["dotnet", "RevisioHub.Web.dll"]
#ENTRYPOINT "/bin/bash"

EXPOSE 2001/tcp
ENV ASPNETCORE_URLS http://+:2001
ENV ASPNETCORE_HTTP_PORTS= 
