FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet publish UsedCarsPriceAPI2 -c Release -o UsedCarsPriceAPI2/out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/UsedCarsPriceAPI2/out .
ENTRYPOINT ["dotnet", "UsedCarsPriceAPI2.dll"]
ENV ASPNETCORE_URLS=http://+:80