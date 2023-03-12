FROM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SeniorProj.Server.dll"]