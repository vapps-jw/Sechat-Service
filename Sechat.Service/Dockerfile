FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["Sechat/Sechat.Service/MasterService.csproj", "Sechat/Sechat.Service/"]
COPY ["Sechat/Sechat.Data/Sechat.Data.csproj", "Sechat/Sechat.Data/"]
RUN dotnet restore "Sechat/Sechat.Service/Sechat.Service.csproj"
COPY . .
WORKDIR "/src/Sechat/MasterService/"
RUN dotnet build "Sechat.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sechat.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sechat.Service.dll"]