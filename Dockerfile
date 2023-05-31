FROM mcr.microsoft.com/dotnet/sdk:6.0.301 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet build -c Release
RUN dotnet publish --configuration Release --no-build --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "MassWhatsAppSite.dll"]
