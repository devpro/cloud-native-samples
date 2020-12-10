FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/", "./"]
RUN dotnet restore "BusinessApi/BusinessApi.csproj"
COPY . .
WORKDIR "/src/BusinessApi"
RUN dotnet build "BusinessApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BusinessApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogzioJaegerSample.BusinessApi.dll"]
