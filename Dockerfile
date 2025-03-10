# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and restore dependencies
COPY *.sln .
COPY Domain/*.csproj ./Domain/
COPY Applicaction/*.csproj ./Applicaction/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY WebApi/*.csproj ./WebApi/

RUN dotnet restore

# Copy the rest of the code and build the application
COPY Domain/. ./Domain/
COPY Applicaction/. ./Applicaction/
COPY Infrastructure/. ./Infrastructure/
COPY WebApi/. ./WebApi/
WORKDIR /app/WebApi
RUN dotnet publish -c Release -o /app/out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out ./

# Verify that the DLL exists (for debugging)
RUN ls -la /app

# Expose the API port
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "WebApi.dll"]
