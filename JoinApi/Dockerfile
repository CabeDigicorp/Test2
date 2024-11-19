#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
COPY ./JoinApi/Deployment/Staging/certificate.pem /usr/local/share/ca-certificates/
COPY ./JoinApi/Deployment/Staging/cabundle.pem /usr/local/share/ca-certificates/
COPY ./JoinApi/Deployment/Staging/fullchain.pem /usr/local/share/ca-certificates/
COPY ./JoinApi/Deployment/Staging/star.digicorp.it.key /usr/local/share/ca-certificates/
RUN chmod 644 /usr/local/share/ca-certificates/certificate.pem && update-ca-certificates
RUN chmod 644 /usr/local/share/ca-certificates/cabundle.pem && update-ca-certificates
RUN chmod 644 /usr/local/share/ca-certificates/fullchain.pem && update-ca-certificates
RUN chmod 644 /usr/local/share/ca-certificates/star.digicorp.it.key && update-ca-certificates
WORKDIR /app

EXPOSE 80
EXPOSE 8080
EXPOSE 5100
EXPOSE 5000
EXPOSE 44300

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=JoinApiDockerStaging
#RUN dotnet dev-certs https
#RUN dotnet dev-certs https --clean
#RUN dotnet dev-certs https --trust
#RUN dotnet dev-certs https -ep /root/.dotnet/corefx/cryptography/x509stores/my/joinwebkestrel.pfx -p pierantonella #per certificato development
WORKDIR /src
COPY ["./JoinApi/JoinApi.csproj", "JoinApi/"]
COPY ["./ModelData/ModelData.csproj", "ModelData/"]
RUN dotnet restore "./JoinApi/JoinApi.csproj"
RUN dotnet restore "./ModelData/ModelData.csproj"
RUN apt-get update && apt-get install -y nodejs

COPY . .
WORKDIR "/src/JoinApi"
RUN dotnet build "JoinApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "JoinApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN apt-get update && \
    apt-get install -y \
    iputils-ping \
    netcat-traditional \
    procps && \
    rm -rf /var/lib/apt/lists/*
RUN apt-get clean

#COPY --from=publish /root/.dotnet/corefx/cryptography/x509stores/my/* /root/.dotnet/corefx/cryptography/x509stores/my/ #per certificato development
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JoinApi.dll"]