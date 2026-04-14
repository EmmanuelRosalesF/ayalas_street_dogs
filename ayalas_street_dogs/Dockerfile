FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ayalas_street_dogs/ayalas_street_dogs.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish ayalas_street_dogs.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ayalas_street_dogs.dll"]
