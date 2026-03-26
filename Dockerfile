# Estágio de Build (Compilação)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# 1. Copia e restaura dependências primeiro (aproveita o cache do Docker)
COPY RinhaIngressos.csproj ./
RUN dotnet restore RinhaIngressos.csproj

# 2. Copia o restante dos arquivos e compila em modo Release
COPY . .
RUN dotnet publish RinhaIngressos.csproj -c Release -o out

# Estágio de Runtime (Execução Final)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# 3. Instala a lib de rede necessária para o PostgreSQL no Linux enxuto
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

# 4. Configurações de performance embutidas (já que o compose é fixo)
ENV DOTNET_gcServer=0
ENV DOTNET_GCHighMemPercent=80
ENV ASPNETCORE_URLS=http://+:8080

# 5. Copia os arquivos compilados do estágio anterior
COPY --from=build /app/out .

# 6. Ponto de entrada (Note o uso obrigatório de aspas duplas)
ENTRYPOINT ["dotnet", "RinhaIngressos.dll"]