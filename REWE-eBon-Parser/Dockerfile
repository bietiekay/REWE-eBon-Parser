#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["REWE-eBon-Parser/REWE-eBon-Parser.csproj", "REWE-eBon-Parser/"]
COPY ["REWEeBonParserLibrary/REWEeBonParserLibrary.csproj", "REWEeBonParserLibrary/"]
RUN dotnet restore "REWE-eBon-Parser/REWE-eBon-Parser.csproj"
COPY . .
WORKDIR "/src/REWE-eBon-Parser"
RUN dotnet build "REWE-eBon-Parser.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "REWE-eBon-Parser.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "REWE-eBon-Parser.dll"]