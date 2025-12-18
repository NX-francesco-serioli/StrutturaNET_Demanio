FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /src

# Restore once to cache dependencies when the image is built
COPY Api.csproj .
RUN dotnet restore

COPY . .

EXPOSE 8080

CMD ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:8080"]
