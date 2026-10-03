FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

# NuGet source is controlled at Docker build time.
ARG NUGET_SOURCE=https://package-mirror.liara.ir/repository/nuget/index.json

WORKDIR /src


# Copy project files first so Docker can cache the restore layer.
# TaskFlow.Web references Domain, Application, and Infrastructure,
# therefore all referenced project files must exist before restore.

COPY ["TaskFlow.Domain/TaskFlow.Domain.csproj", "TaskFlow.Domain/"]
COPY ["TaskFlow.Application/TaskFlow.Application.csproj", "TaskFlow.Application/"]
COPY ["TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "TaskFlow.Infrastructure/"]
COPY ["TaskFlow.Web/TaskFlow.Web.csproj", "TaskFlow.Web/"]


# Restoring the Web project also restores all referenced projects.
RUN dotnet restore "TaskFlow.Web/TaskFlow.Web.csproj" \
    --source "$NUGET_SOURCE" \
    --disable-parallel


# Copy the remaining source code only after restore.
COPY . .


WORKDIR "/src/TaskFlow.Web"


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