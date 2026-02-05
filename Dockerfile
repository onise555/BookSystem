# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ვაკოპირებთ .csproj ფაილს (რადგან Dockerfile-ის გვერდითაა)
COPY ["BookSystem.csproj", "./"]
RUN dotnet restore "BookSystem.csproj"

# ვაკოპირებთ დანარჩენ კოდს
COPY . .

# ვაკეთებთ Release ბილდს
RUN dotnet publish "BookSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# პორტის კონფიგურაცია
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookSystem.dll"]