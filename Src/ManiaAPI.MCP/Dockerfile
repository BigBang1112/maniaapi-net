# syntax=docker/dockerfile:1

# Build from the repository root, as the MCP references the other projects and generators:
#   docker build -f Src/ManiaAPI.MCP/Dockerfile -t maniaapi-mcp .

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0-alpine AS build
WORKDIR /src

# Restore first, so the packages stay cached until a project file changes
COPY Src/Directory.Build.targets Src/
COPY Generators/ManiaAPI.MCP.Generators/ManiaAPI.MCP.Generators.csproj Generators/ManiaAPI.MCP.Generators/
COPY Generators/ManiaAPI.TMX.Generators/ManiaAPI.TMX.Generators.csproj Generators/ManiaAPI.TMX.Generators/
COPY Src/ManiaAPI.MCP/ManiaAPI.MCP.csproj Src/ManiaAPI.MCP/
COPY Src/ManiaAPI.ManiaPlanetAPI/ManiaAPI.ManiaPlanetAPI.csproj Src/ManiaAPI.ManiaPlanetAPI/
COPY Src/ManiaAPI.ManiaPlanetAPI.Extensions.Hosting/ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.csproj Src/ManiaAPI.ManiaPlanetAPI.Extensions.Hosting/
COPY Src/ManiaAPI.MasterServer/ManiaAPI.MasterServer.csproj Src/ManiaAPI.MasterServer/
COPY Src/ManiaAPI.MasterServer.Extensions.Hosting/ManiaAPI.MasterServer.Extensions.Hosting.csproj Src/ManiaAPI.MasterServer.Extensions.Hosting/
COPY Src/ManiaAPI.NadeoAPI/ManiaAPI.NadeoAPI.csproj Src/ManiaAPI.NadeoAPI/
COPY Src/ManiaAPI.NadeoAPI.Extensions.Gbx/ManiaAPI.NadeoAPI.Extensions.Gbx.csproj Src/ManiaAPI.NadeoAPI.Extensions.Gbx/
COPY Src/ManiaAPI.NadeoAPI.Extensions.Hosting/ManiaAPI.NadeoAPI.Extensions.Hosting.csproj Src/ManiaAPI.NadeoAPI.Extensions.Hosting/
COPY Src/ManiaAPI.TMX/ManiaAPI.TMX.csproj Src/ManiaAPI.TMX/
COPY Src/ManiaAPI.TMX.Extensions.Gbx/ManiaAPI.TMX.Extensions.Gbx.csproj Src/ManiaAPI.TMX.Extensions.Gbx/
COPY Src/ManiaAPI.TMX.Extensions.Hosting/ManiaAPI.TMX.Extensions.Hosting.csproj Src/ManiaAPI.TMX.Extensions.Hosting/
COPY Src/ManiaAPI.TrackmaniaAPI/ManiaAPI.TrackmaniaAPI.csproj Src/ManiaAPI.TrackmaniaAPI/
COPY Src/ManiaAPI.TrackmaniaAPI.Extensions.Hosting/ManiaAPI.TrackmaniaAPI.Extensions.Hosting.csproj Src/ManiaAPI.TrackmaniaAPI.Extensions.Hosting/
COPY Src/ManiaAPI.TrackmaniaWS/ManiaAPI.TrackmaniaWS.csproj Src/ManiaAPI.TrackmaniaWS/
COPY Src/ManiaAPI.TrackmaniaWS.Extensions.Hosting/ManiaAPI.TrackmaniaWS.Extensions.Hosting.csproj Src/ManiaAPI.TrackmaniaWS.Extensions.Hosting/
COPY Src/ManiaAPI.UnitedLadder/ManiaAPI.UnitedLadder.csproj Src/ManiaAPI.UnitedLadder/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore Src/ManiaAPI.MCP/ManiaAPI.MCP.csproj

COPY Generators/ Generators/
COPY Src/ Src/

# The libraries pack themselves on build, which the image doesn't need
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish Src/ManiaAPI.MCP/ManiaAPI.MCP.csproj -c Release -o /app --no-restore \
        -p:GeneratePackageOnBuild=false -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:11.0-alpine
WORKDIR /app
COPY --from=build /app .

# Listen on all interfaces through a Kestrel endpoint rather than ASPNETCORE_HTTP_PORTS, as the startup check
# that keeps writes on loopback only sees addresses from the configuration.
ENV ASPNETCORE_HTTP_PORTS= \
    Kestrel__Endpoints__Http__Url=http://0.0.0.0:8080
EXPOSE 8080

USER $APP_UID

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget -q -O /dev/null http://127.0.0.1:8080/healthz || exit 1

# HTTP by default. Pass other arguments to replace it, like --toolsets tmnf,exchange; without --http, it runs over stdio.
ENTRYPOINT ["dotnet", "ManiaAPI.MCP.dll"]
CMD ["--http"]
