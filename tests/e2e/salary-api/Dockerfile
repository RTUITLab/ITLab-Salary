FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app

COPY ./itlab-salary-build /app
COPY ./appsettings.Production.json /app

CMD [ "dotnet", "Backend.dll" ]