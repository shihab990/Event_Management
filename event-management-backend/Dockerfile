FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore event-management-backend.sln
RUN dotnet dev-certs https -ep /src/aspnetapp.pfx -p "aspnetcoredev"
RUN dotnet publish WebApi/WebApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/aspnetapp.pfx /app/aspnetapp.pfx
RUN mkdir -p /app/Data

ENV ASPNETCORE_URLS=http://+:8080;https://+:8443
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=aspnetcoredev
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/aspnetapp.pfx
EXPOSE 8080 8443

ENTRYPOINT ["dotnet", "WebApi.dll"]
