FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copia os arquivos de projeto e restaura as dependências
COPY *.csproj ./
RUN dotnet restore

# Copia o restante do código e gera o build de publicação
COPY . ./
RUN dotnet publish -c Release -o out

# Estágio final de execução usando o runtime do .NET 10
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

# Expõe a porta padrão da API
EXPOSE 5292
ENV ASPNETCORE_URLS=http://+:5292

ENTRYPOINT ["dotnet", "EduqPlus.API.dll"]