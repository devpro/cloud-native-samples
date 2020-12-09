FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/", "./"]
RUN dotnet restore "DataApi/DataApi.csproj"
COPY . .
WORKDIR "/src/DataApi"
RUN dotnet build "DataApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogzioJaegerSample.DataApi.dll"]
