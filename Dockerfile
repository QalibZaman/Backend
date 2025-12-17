# ---------- BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish NettoRetail.API/NettoRetail.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------- RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 8080

COPY --from=build /app/publish .
COPY NettoRetail.Infrastructure/EmailTemplates ./EmailTemplates

ENTRYPOINT ["dotnet", "NettoRetail.API.dll"]
