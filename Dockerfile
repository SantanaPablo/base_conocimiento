FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["BaseConocimiento.API/BaseConocimiento.API.csproj", "BaseConocimiento.API/"]
COPY ["BaseConocimiento.Infraestructure/BaseConocimiento.Infrastructure.csproj", "BaseConocimiento.Infraestructure/"]

# Restaurar
RUN dotnet restore "BaseConocimiento.API/BaseConocimiento.API.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/BaseConocimiento.API"
RUN dotnet publish "BaseConocimiento.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BaseConocimiento.API.dll"]