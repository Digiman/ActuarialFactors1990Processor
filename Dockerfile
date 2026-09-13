# Multi-stage build: SDK for compile, ASP.NET runtime as the final image.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /workspace
COPY Directory.Packages.props ./
COPY .editorconfig ./
COPY src/ ./src/
RUN dotnet build src/DataProcessingApp.WebApi/DataProcessingApp.WebApi.csproj -c Release --nologo

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /workspace/src/DataProcessingApp.WebApi/bin/Release/net10.0/ ./
COPY --from=build /workspace/src/DataProcessingApp.WebApi/wwwroot/ ./wwwroot/
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "DataProcessingApp.WebApi.dll"]