FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Labratory/Labratory.csproj", "Labratory/"]
RUN dotnet restore "Labratory/Labratory.csproj"
COPY . .
WORKDIR "/src/Labratory"
RUN dotnet build "Labratory.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Labratory.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Labratory.dll"]
