FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /app
COPY LineBackend/LineBackend.csproj .
RUN dotnet restore
COPY LineBackend LineBackend
RUN dotnet build LineBackend


FROM build as publish
WORKDIR /app
RUN dotnet publish LineBackend -c Release -o out -r linux-x64


FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
# ENV PORT=80
WORKDIR /app
COPY --from=publish /app/out .
CMD dotnet LineBackend.dll