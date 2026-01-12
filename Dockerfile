# Mijn Energie - Blazor Server App
# Multi-stage build for .NET 9

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY urban_city_power_managment.Web/urban_city_power_managment.Web.csproj ./urban_city_power_managment.Web/
RUN dotnet restore urban_city_power_managment.Web/urban_city_power_managment.Web.csproj

# Copy all source files
COPY . .

# Build the application
WORKDIR /src/urban_city_power_managment.Web
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Expose port
EXPOSE 8080

# Copy published files
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "urban_city_power_managment.Web.dll"]
