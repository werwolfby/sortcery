FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Sortcery.Api/Sortcery.Api.csproj", "Sortcery.Api/"]
COPY ["Sortcery.Api.Contracts/Sortcery.Api.Contracts.csproj", "Sortcery.Api.Contracts/"]
COPY ["Sortcery.Engine/Sortcery.Engine.csproj", "Sortcery.Engine/"]
COPY ["Sortcery.Engine.Contracts/Sortcery.Engine.Contracts.csproj", "Sortcery.Engine.Contracts/"]
COPY ["Sortcery.Web/Sortcery.Web.csproj", "Sortcery.Web/"]
RUN dotnet restore "Sortcery.Api/Sortcery.Api.csproj"
COPY . .
WORKDIR "/src/Sortcery.Api"
RUN dotnet build "Sortcery.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sortcery.Api.csproj" -c Release -o /app/publish

FROM base AS final
EXPOSE 5000
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sortcery.Api.dll"]
