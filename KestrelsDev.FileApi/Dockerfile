# ---- Base runtime image ----
FROM ghcr.io/kestrelsdevelopment/kestrelsdev.dotnet9.runtime:latest AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://0.0.0.0:80

# ---- Build image ----
FROM ghcr.io/kestrelsdevelopment/kestrelsdev.dotnet9.sdk:latest AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
USER root

# Copy in .csproj
COPY ["./KestrelsDev.FileApi.csproj", "./KestrelsDev.FileApi/"]

# Restore
RUN dotnet restore "KestrelsDev.FileApi/KestrelsDev.FileApi.csproj"

# Copy in new files
COPY [".",  "./KestrelsDev.FileApi/"]

# Build
WORKDIR "/src/KestrelsDev.FileApi"
RUN dotnet build "KestrelsDev.FileApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ---- Publish image ----
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "KestrelsDev.FileApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---- Final image ----
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ["KestrelsDev.FileApi.dll"]
