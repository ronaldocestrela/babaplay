FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BabaPlay.slnx", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/BabaPlay.Api/BabaPlay.Api.csproj", "src/BabaPlay.Api/"]
COPY ["src/BabaPlay.Application/BabaPlay.Application.csproj", "src/BabaPlay.Application/"]
COPY ["src/BabaPlay.Domain/BabaPlay.Domain.csproj", "src/BabaPlay.Domain/"]
COPY ["src/BabaPlay.Infrastructure/BabaPlay.Infrastructure.csproj", "src/BabaPlay.Infrastructure/"]
RUN dotnet restore "src/BabaPlay.Api/BabaPlay.Api.csproj"

COPY . .
RUN dotnet publish "src/BabaPlay.Api/BabaPlay.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
VOLUME ["/app/storage"]

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BabaPlay.Api.dll"]
