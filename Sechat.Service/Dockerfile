FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY ["Sechat.Service/Sechat.Service.csproj", "Sechat.Service/"]
COPY ["Sechat.Data/Sechat.Data.csproj", "Sechat.Data/"]

RUN dotnet restore "Sechat.Data/Sechat.Data.csproj"
RUN dotnet restore "Sechat.Service/Sechat.Service.csproj"

COPY . .

WORKDIR "/app/Sechat.Service"

RUN dotnet build "Sechat.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sechat.Service.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sechat.Service.dll"]

