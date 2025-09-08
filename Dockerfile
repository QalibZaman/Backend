FROM mcr.microsoft.com/dotnet/runtime-deps:8.0 AS runtime
USER $APP_UID
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
EXPOSE 8081
COPY ["NettoRetail.Infrastructure/EmailTemplates", "/app"]
COPY ./publish/ .
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["./NettoRetail.API"]