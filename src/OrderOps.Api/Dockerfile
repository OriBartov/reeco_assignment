FROM node:22-alpine AS frontend
WORKDIR /frontend

COPY src/OrderOps.Web/package.json src/OrderOps.Web/package-lock.json ./
RUN npm ci

COPY src/OrderOps.Web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/OrderOps.sln ./
COPY src/OrderOps.Api/OrderOps.Api.csproj OrderOps.Api/
COPY src/OrderOps.Infrastructure/OrderOps.Infrastructure.csproj OrderOps.Infrastructure/
COPY src/OrderOps.Import/OrderOps.Import.csproj OrderOps.Import/

RUN dotnet restore OrderOps.Api/OrderOps.Api.csproj

COPY src/OrderOps.Api/ OrderOps.Api/
COPY src/OrderOps.Infrastructure/ OrderOps.Infrastructure/
COPY --from=frontend /frontend/dist OrderOps.Api/wwwroot/
COPY data/ /app/data/

RUN dotnet publish OrderOps.Api/OrderOps.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
COPY --from=build /app/data ./data

ENV ASPNETCORE_URLS=http://+:3000
ENV ASPNETCORE_ENVIRONMENT=Docker

EXPOSE 3000

ENTRYPOINT ["dotnet", "OrderOps.Api.dll"]
