FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

WORKDIR /app

COPY itlab-salary-build .

ENV ASPNETCORE_URLS=http://*:5508
CMD dotnet Backend.dll
