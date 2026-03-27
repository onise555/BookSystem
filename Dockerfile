# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# სპეციალურად ვუთითებთ კონკრეტულ ფაილს, რომ ცვლილება აუცილებლად დაფიქსირდეს
COPY ["BookSystem.csproj", "./"]

# ვაკეთებთ რესტორს (აქ ხდება ყველა NuGet პაკეტის, მათ შორის ImageSharp-ის გადმოწერა)
RUN dotnet restore "BookSystem.csproj"

# ახლა ვაკოპირებთ მთლიან კოდს
COPY . .

# ვაკეთებთ ფაბლიშს
RUN dotnet publish "BookSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# ვამატებთ ლიბიბლიოთეკებს, რომლებიც ImageSharp-ს სჭირდება Linux-ზე მუშაობისთვის
RUN apt-get update && apt-get install -y libicu-dev libfontconfig1 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

USER app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookSystem.dll"]