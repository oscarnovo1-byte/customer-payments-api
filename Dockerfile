# =========================
# Compilation Stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["src/CustomerPayments.Api/CustomerPayments.Api.csproj", \
      "src/CustomerPayments.Api/"]

RUN dotnet restore \
    "src/CustomerPayments.Api/CustomerPayments.Api.csproj"

COPY . .

WORKDIR "/src/src/CustomerPayments.Api"

RUN dotnet publish \
    "CustomerPayments.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false


# =========================
# Execution Stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=build /app/publish .

USER root

# Directory where SQLite will be persisted
RUN mkdir -p /app/data \
    && chown -R app:app /app/data

USER app

ENTRYPOINT ["dotnet", "CustomerPayments.Api.dll"]