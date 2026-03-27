# 1. Build Stage (აქ ხდება კოდის კომპილაცია)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ვიყენებთ wildcards (*), რომ ყველა .csproj ფაილი დაინახოს
# ეს აგვარებს "file not found" შეცდომას, თუ სტრუქტურა ოდნავ განსხვავებულია
COPY *.csproj ./
RUN dotnet restore

# ვაკოპირებთ დანარჩენ ფაილებს და ვაკეთებთ Publish-ს
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# 2. Final Stage (აქ იქმნება პატარა იმიჯი, რომელიც მხოლოდ გაშვებისთვისაა)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# უსაფრთხოების მიზნით, აპლიკაცია გაეშვება დაბალი პრივილეგიის მომხმარებლით
USER app

# Railway-სთვის საჭირო პორტის კონფიგურაცია
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Portfolio.Asp.dll"]