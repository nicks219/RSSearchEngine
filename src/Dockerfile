FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# при использовании CPM файл версионирования необходимо скопировать в контейнер
COPY Directory.Packages.props .
COPY ./Rsse.Base/ ./Rsse.Base/
COPY ./Rsse.Data/ ./Rsse.Data/
# скопируем предварительно собранный фронт
COPY ./Rsse.Front/ClientApp/build/ ./Rsse.Base/ClientApp/build/

WORKDIR /app/Rsse.Base
# опубликуем в папку /app/Rsse.Base/out/
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
COPY --from=build-env /app/Rsse.Base/out/ App/
COPY --from=build-env /app/Rsse.Base/ClientApp/build/ App/ClientApp/build/
WORKDIR /App
EXPOSE 5000
ENTRYPOINT ["dotnet", "Rsse.Base.dll"]
