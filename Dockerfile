# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# აკოპირებს ყველა .csproj ფაილს, რაც კი საქაღალდეშია
COPY *.csproj ./
RUN dotnet restore

# აკოპირებს კოდს და აკეთებს ფაბლიშს
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# 2. Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# .NET 8-ის უსაფრთხოების სტანდარტი
USER app

# Railway პორტის კონფიგურაცია
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# აქ ჩავწერე შენი პროექტის სახელი სურათის მიხედვით
ENTRYPOINT ["dotnet", "BookSystem.dll"]