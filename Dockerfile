# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first so `dotnet restore` is cached unless dependencies change
COPY PhotographerApp.Core/PhotographerApp.Core.csproj PhotographerApp.Core/
COPY PhotographerApp.Infrastructure/PhotographerApp.Infrastructure.csproj PhotographerApp.Infrastructure/
COPY PhotographerApp.API/PhotographerApp.API.csproj PhotographerApp.API/
RUN dotnet restore PhotographerApp.API/PhotographerApp.API.csproj

# Copy the rest of the source and publish
COPY . .
RUN dotnet publish PhotographerApp.API/PhotographerApp.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render assigns the listening port via $PORT; Program.cs already binds to it.
ENTRYPOINT ["dotnet", "PhotographerApp.API.dll"]
