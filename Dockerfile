# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["BaseConocimiento.API/BaseConocimiento.API.csproj", "BaseConocimiento.API/"]
COPY ["BaseConocimiento.Infraestructure/BaseConocimiento.Infrastructure.csproj", "BaseConocimiento.Infraestructure/"]
RUN dotnet restore "BaseConocimiento.API/BaseConocimiento.API.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/BaseConocimiento.API"
RUN dotnet build "BaseConocimiento.API.csproj" -c Release -o /app/build

# Publicar
FROM build AS publish
RUN dotnet publish "BaseConocimiento.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
# Ajusta el nombre del .dll si es diferente
ENTRYPOINT ["dotnet", "BaseConocimiento.API.dll"]