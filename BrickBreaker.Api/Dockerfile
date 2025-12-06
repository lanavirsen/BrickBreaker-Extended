# Runtime image runs the published API artifacts
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage compiles only the API and its dependencies
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BrickBreaker.Api/BrickBreaker.Api.csproj", "BrickBreaker.Api/"]
COPY ["BrickBreaker.Core/BrickBreaker.Core.csproj", "BrickBreaker.Core/"]
COPY ["BrickBreaker.Storage/BrickBreaker.Storage.csproj", "BrickBreaker.Storage/"]
RUN dotnet restore "BrickBreaker.Api/BrickBreaker.Api.csproj"
COPY BrickBreaker.Api/ BrickBreaker.Api/
COPY BrickBreaker.Core/ BrickBreaker.Core/
COPY BrickBreaker.Storage/ BrickBreaker.Storage/
WORKDIR "/src/BrickBreaker.Api"
RUN dotnet publish "BrickBreaker.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image contains only runtime + published output
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BrickBreaker.Api.dll"]
