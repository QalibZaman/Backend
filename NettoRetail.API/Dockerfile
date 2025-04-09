FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NettoRetail.API/NettoRetail.API.csproj", "NettoRetail.API/"]
COPY ["NettoRetail.Application/NettoRetail.Application.csproj", "NettoRetail.Application/"]
COPY ["NettoRetail.DataAccess/NettoRetail.DataAccess.csproj", "NettoRetail.DataAccess/"]
COPY ["NettoRetail.Shared/NettoRetail.Shared.csproj", "NettoRetail.Shared/"]
COPY ["NettoRetail.Domain/NettoRetail.Domain.csproj", "NettoRetail.Domain/"]
COPY ["NettoRetail.Infrastructure/NettoRetail.Infrastructure.csproj", "NettoRetail.Infrastructure/"]
RUN dotnet restore "./NettoRetail.API/NettoRetail.API.csproj"
COPY . .
WORKDIR "/src/NettoRetail.API"
RUN dotnet build "./NettoRetail.API.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NettoRetail.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
#Copies email templates that are sent when recovering password
COPY ["NettoRetail.Infrastructure/EmailTemplates", "/app"]
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NettoRetail.API.dll"]
