# Reusable Dockerfile for OrderFlow .NET API, Gateway, and microservice projects.
# docker-compose passes PROJECT_PATH and DLL_NAME so one Dockerfile can build
# each backend project without duplicating the same container instructions.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
ARG PROJECT_PATH
RUN dotnet restore "$PROJECT_PATH"
RUN dotnet publish "$PROJECT_PATH" -c Release -o /app/publish /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
ARG DLL_NAME
ENV ORDERFLOW_DLL_NAME=$DLL_NAME
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=build /app/publish .
ENTRYPOINT ["sh", "-c", "dotnet \"$ORDERFLOW_DLL_NAME\""]
