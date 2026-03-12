# Use the official .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the csproj file and restore dependencies
COPY ["invex-api.csproj", "./"]
RUN dotnet restore "./invex-api.csproj"

# Copy the rest of the application files
COPY . .

# Build and publish the application
RUN dotnet publish "invex-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Ensure the application listens on the port provided by Render
# Render typically sets the PORT environment variable.
# We also expose 8080 as the default .NET 8+ container port.
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Set the entrypoint
ENTRYPOINT ["dotnet", "invex-api.dll"]
