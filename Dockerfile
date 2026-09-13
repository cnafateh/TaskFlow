FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

# NuGet source is controlled at Docker build time.
ARG NUGET_SOURCE=https://package-mirror.liara.ir/repository/nuget/index.json

WORKDIR /src

COPY ["TaskFlow.Web/TaskFlow.Web.csproj", "TaskFlow.Web/"]


# Restore only from the configured mirror.
# --source overrides the NuGet sources for this restore.
RUN dotnet restore "TaskFlow.Web/TaskFlow.Web.csproj" \
    --source "$NUGET_SOURCE" \
    --disable-parallel


COPY . .

WORKDIR "/src/TaskFlow.Web"


# Restore has already been completed above.
RUN dotnet publish "TaskFlow.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore


FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .


USER root

RUN mkdir -p /app/data /app/keys \
    && chown -R $APP_UID /app


USER $APP_UID


ENTRYPOINT ["dotnet", "TaskFlow.Web.dll"]