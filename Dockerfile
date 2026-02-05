# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# რადგან .csproj და Dockerfile ერთად დევს, პირდაპირ ვაკოპირებთ ფაილს
COPY ["BookSystem.csproj", "./"]
RUN dotnet restore "BookSystem.csproj"

# ვაკოპირებთ დანარჩენ ყველა ფაილს
COPY . .

# ვაკეთებთ Publish-ს
RUN dotnet publish "BookSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway მოითხოვს პორტის მითითებას
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookSystem.dll"]