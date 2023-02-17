FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Sortcery/Sortcery.csproj", "Sortcery/"]
RUN dotnet restore "Sortcery/Sortcery.csproj"
COPY . .
WORKDIR "/src/Sortcery"
RUN dotnet build "Sortcery.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sortcery.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sortcery.dll"]
