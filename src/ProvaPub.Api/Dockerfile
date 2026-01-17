# ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["src/ProvaPub.Api/ProvaPub.Api.csproj", "src/ProvaPub.Api/"]
COPY ["src/ProvaPub.Application/ProvaPub.Application.csproj", "src/ProvaPub.Application/"]
COPY ["src/ProvaPub.Domain/ProvaPub.Domain.csproj", "src/ProvaPub.Domain/"]
COPY ["src/ProvaPub.Infrastructure/ProvaPub.Infrastructure.csproj", "src/ProvaPub.Infrastructure/"]

RUN dotnet restore "./src/ProvaPub.Api/ProvaPub.Api.csproj"

COPY . .

WORKDIR "/src/src/ProvaPub.Api"

RUN dotnet build "./ProvaPub.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ---
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProvaPub.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProvaPub.Api.dll"]