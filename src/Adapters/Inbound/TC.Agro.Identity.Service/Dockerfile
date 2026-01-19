# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy Directory.Build.props, Directory.Build.targets and Directory.Packages.props first (build optimization)
# From common
COPY common/Directory.Build.props ./common/
COPY common/Directory.Build.targets ./common/
COPY common/Directory.Packages.props ./common/

# From identity-service
COPY services/identity-service/Directory.Build.props ./services/identity-service/
COPY services/identity-service/Directory.Build.targets ./services/identity-service/
COPY services/identity-service/Directory.Packages.props ./services/identity-service/

# Copy shared common projects (required for ProjectReferences)
COPY ["common/src/TC.Agro.SharedKernel/TC.Agro.SharedKernel.csproj", "common/src/TC.Agro.SharedKernel/"]
COPY ["common/src/TC.Agro.Contracts/TC.Agro.Contracts.csproj", "common/src/TC.Agro.Contracts/"]

# Copy identity-service projects maintaining directory structure (required for ProjectReferences)
COPY ["services/identity-service/src/Adapters/Inbound/TC.Agro.Identity.Service/TC.Agro.Identity.Service.csproj", "services/identity-service/src/Adapters/Inbound/TC.Agro.Identity.Service/"]
COPY ["services/identity-service/src/Core/TC.Agro.Identity.Application/TC.Agro.Identity.Application.csproj", "services/identity-service/src/Core/TC.Agro.Identity.Application/"]
COPY ["services/identity-service/src/Core/TC.Agro.Identity.Domain/TC.Agro.Identity.Domain.csproj", "services/identity-service/src/Core/TC.Agro.Identity.Domain/"]
COPY ["services/identity-service/src/Adapters/Outbound/TC.Agro.Identity.Infrastructure/TC.Agro.Identity.Infrastructure.csproj", "services/identity-service/src/Adapters/Outbound/TC.Agro.Identity.Infrastructure/"]

# Restore packages
RUN dotnet restore "services/identity-service/src/Adapters/Inbound/TC.Agro.Identity.Service/TC.Agro.Identity.Service.csproj"

# Copy source code maintaining structure
COPY . .

# Build the application
WORKDIR "/src/services/identity-service/src/Adapters/Inbound/TC.Agro.Identity.Service"
RUN dotnet build "./TC.Agro.Identity.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TC.Agro.Identity.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TC.Agro.Identity.Service.dll"]