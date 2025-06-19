# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/OrderServices/Hosts/OrderServices.Api/OrderServices.Api.csproj", "OrderServices/Hosts/OrderServices.Api/"]
COPY ["src/OrderServices/Application/OrderServices.AppServices/OrderServices.AppServices.csproj", "OrderServices/Application/OrderServices.AppServices/"]
COPY ["src/OrderServices/Contracts/OrderServices.Contracts/OrderServices.Contracts.csproj", "OrderServices/Contracts/OrderServices.Contracts/"]
COPY ["src/OrderServices/Domain/OrderServices.Domain/OrderServices.Domain.csproj", "OrderServices/Domain/OrderServices.Domain/"]
COPY ["src/OrderServices/Infrastructure/OrderServices.DataAccess/OrderServices.DataAccess.csproj", "OrderServices/Infrastructure/OrderServices.DataAccess/"]
RUN dotnet restore "./OrderServices/Hosts/OrderServices.Api/OrderServices.Api.csproj"
COPY . .
WORKDIR "src/OrderServices/Hosts/OrderServices.Api"
RUN dotnet build "./OrderServices.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OrderServices.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderServices.Api.dll"]