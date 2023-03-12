FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /src
COPY ["Server/SeniorProj.Server.csproj", "Server/"]
RUN dotnet restore "Server/SeniorProj.Server.csproj"
COPY . .
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
ENTRYPOINT ["dotnet", "SeniorProj.Server.dll"]