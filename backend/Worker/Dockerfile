FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /src

COPY Directory.Packages.props ./
COPY backend/Worker/Worker.csproj backend/Worker/
COPY backend/Application/Application.csproj backend/Application/
COPY backend/Domain/Domain.csproj backend/Domain/
COPY backend/Persistence/Persistence.csproj backend/Persistence/

RUN dotnet restore backend/Worker/Worker.csproj

COPY backend ./backend

WORKDIR /src/backend/Worker

CMD ["dotnet", "watch", "run"]
