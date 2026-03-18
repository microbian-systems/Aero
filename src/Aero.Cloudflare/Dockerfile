# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Aero/src/Directory.Build.props", "Aero/src/"]
COPY ["Directory.build.props", "."]
COPY ["Aero/src/NuGet.Config", "Aero/src/"]
COPY ["Aero/src/Aero.Cloudflare/Aero.Cloudflare.csproj", "Aero/src/Aero.Cloudflare/"]
RUN dotnet restore "./Aero/src/Aero.Cloudflare/Aero.Cloudflare.csproj"
COPY . .
WORKDIR "/src/Aero/src/Aero.Cloudflare"
RUN dotnet build "./Aero.Cloudflare.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aero.Cloudflare.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aero.Cloudflare.dll"]