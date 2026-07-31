FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BabaPlay.slnx", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/BabaPlay.Web/BabaPlay.Web.csproj", "src/BabaPlay.Web/"]
RUN dotnet restore "src/BabaPlay.Web/BabaPlay.Web.csproj"

COPY . .
RUN dotnet publish "src/BabaPlay.Web/BabaPlay.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/BabaPlay.Web/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
