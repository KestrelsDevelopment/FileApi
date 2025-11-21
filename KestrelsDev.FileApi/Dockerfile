# Define the non-root user used for security in the final runtime image
# We assume 'appuser' is available in your runtime base image.
ARG APP_USER=nonroot
# Define the non-root user used in the build stage (you used 'builder' in your chown attempts)
ARG BUILD_USER=builder

# ---- Base runtime image (Runs as non-root) ----
FROM ghcr.io/kestrelsdevelopment/kestrelsdev.dotnet09.runtime-curl:latest AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://0.0.0.0:80
# CRITICAL: Set the non-root user for security in the final image
USER ${APP_USER}

# ---- Build image (Uses non-root for commands) ----
FROM ghcr.io/kestrelsdevelopment/kestrelsdev.dotnet9.sdk:latest AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Use 'root' briefly to set up directory ownership for the output
# This resolves the "Access to the path '/app' is denied" error from the 'dotnet build' step.
USER root
RUN chown -R ${BUILD_USER}:${BUILD_USER} /app
RUN mkdir -p /app/build /app/publish && chown -R ${BUILD_USER}:${BUILD_USER} /app/build /app/publish

# 2. Use 'root' for COPY operations so we can apply --chown
# COPY 1: .csproj (FIXED: Converted to shell form)
COPY --chown=${BUILD_USER}:${BUILD_USER} ./KestrelsDev.FileApi.csproj ./KestrelsDev.FileApi/

# Switch to the non-root user for security during execution of .NET CLI commands
USER ${BUILD_USER}

# Restore (runs as non-root user)
RUN dotnet restore "KestrelsDev.FileApi/KestrelsDev.FileApi.csproj"

# Switch back to 'root' for the second COPY operation
USER root
# COPY 2: All other files (FIXED: Converted to shell form)
COPY --chown=${BUILD_USER}:${BUILD_USER} . ./KestrelsDev.FileApi/
# Switch back to the non-root user
USER ${BUILD_USER}

# Build (runs as non-root user)
WORKDIR "/src/KestrelsDev.FileApi"
RUN dotnet build "KestrelsDev.FileApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ---- Publish image (Runs as non-root) ----
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/KestrelsDev.FileApi"
USER ${BUILD_USER}
# The /app/publish directory is already owned by ${BUILD_USER}
RUN dotnet publish "KestrelsDev.FileApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---- Final image (Runs as non-root) ----
FROM base AS final
ARG APP_USER=appuser
WORKDIR /app

# The published files are owned by the 'builder' user. We must switch to root 
# one last time to copy them and change their ownership to the runtime 'appuser'.
USER root
COPY --from=publish /app/publish .

# Change ownership to the final runtime user (ensuring the user can access the files)
RUN chown -R ${APP_USER}:${APP_USER} /app

# Switch back to the non-root user for safe runtime execution
USER ${APP_USER}

CMD ["KestrelsDev.FileApi.dll"]
