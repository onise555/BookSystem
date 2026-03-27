# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ჯერ მხოლოდ პროექტის ფაილს ვაკოპირებთ ქეშირებისთვის
COPY ["Portfolio.Asp.csproj", "./"]
RUN dotnet restore "Portfolio.Asp.csproj"

# ახლა ვაკოპირებთ მთლიან კოდს
COPY . .
# ვაკეთებთ ფაბლიშს (Optimization: --no-restore რადგან ზემოთ უკვე ვქენით)
RUN dotnet publish "Portfolio.Asp.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# 2. Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# უსაფრთხოებისთვის: ვიყენებთ დაბალი პრივილეგიის მომხმარებელს
USER app

# Railway-სთვის და .NET 8-სთვის ეს პორტი სტანდარტია
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Portfolio.Asp.dll"]