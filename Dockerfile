# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ვაკოპირებთ .csproj-ს და ვაკეთებთ restore-ს
COPY ["BookSystem.csproj", "./"]
RUN dotnet restore "BookSystem.csproj"

# ვაკოპირებთ მთლიან კოდს და ვაკეთებთ publish-ს
COPY . .
RUN dotnet publish "BookSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway მოითხოვს, რომ კონტეინერმა 8080 პორტი გამოიყენოს შიგნით
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookSystem.dll"]