FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ვაკოპირებთ .csproj ფაილს შესაბამის საქაღალდეში და ვაკეთებთ restore-ს
COPY ["BookSystem/BookSystem.csproj", "BookSystem/"]
RUN dotnet restore "BookSystem/BookSystem.csproj"

# ვაკოპირებთ მთლიან პროექტს
COPY . .

# გადავდივართ პროექტის საქაღალდეში დასაბილდად
WORKDIR "/src/BookSystem"
RUN dotnet publish "BookSystem.csproj" -c Release -o /app/publish

# Runtime ეტაპი
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# პორტის კონფიგურაცია
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookSystem.dll"]