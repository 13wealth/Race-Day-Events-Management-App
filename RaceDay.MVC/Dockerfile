# Stage 1: build the MVC app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RaceDay.MVC/RaceDay.MVC.csproj", "RaceDay.MVC/"]
COPY ["RaceDay.Contracts/RaceDay.Contracts.csproj", "RaceDay.Contracts/"]
RUN dotnet restore "RaceDay.MVC/RaceDay.MVC.csproj"
COPY RaceDay.MVC/ RaceDay.MVC/
COPY RaceDay.Contracts/ RaceDay.Contracts/
RUN dotnet publish "RaceDay.MVC/RaceDay.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: run the MVC app
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]