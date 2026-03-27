FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# აკოპირებს პროექტს და აინსტალირებს ბიბლიოთეკებს
COPY *.csproj ./
RUN dotnet restore

# აკოპირებს კოდს და აკეთებს ფაბლიშს
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# ImageSharp-ისთვის ხანდახან საჭიროა დამატებითი ბიბლიოთეკები ლინუქსზე
RUN apt-get update && apt-get install -y libicu-dev libfontconfig1 && rm -rf /var/lib/apt/lists/*

USER app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookSystem.dll"]