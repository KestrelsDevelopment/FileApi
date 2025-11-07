# ---- Base runtime image ----
FROM ghcr.io/kestrelsdevelopment/kestrelsdev.dotnet9.runtime:latest AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://0.0.0.0:80

FROM ghcr.io/kestrelsdevelopment/kestrelsdev.dotnet9.sdk:latest AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# switch to root to perform chown during copy (if the image isn't root already)
USER root

# Copy with ownership for the builder user
COPY --chown=builder:builder ["./KestrelsDev.FileApi.csproj", "./KestrelsDev.FileApi/"]
RUN dotnet restore "KestrelsDev.FileApi/KestrelsDev.FileApi.csproj"

COPY --chown=builder:builder [".",  "./KestrelsDev.FileApi/"]

# drop privileges for the actual build if you want
USER builder
WORKDIR "/src/KestrelsDev.FileApi"
RUN dotnet build "KestrelsDev.FileApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ---- Publish image ----
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "KestrelsDev.FileApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---- Final image ----
FROM base AS final
WORKDIR /app

# Copy the application
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "KestrelsDev.FileApi.dll"]
