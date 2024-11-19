#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
FROM nginx AS base
EXPOSE 80
EXPOSE 5101 
EXPOSE 5002
#EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=JoinWebUIDockerStaging
WORKDIR /src
COPY ["JoinWebUI/JoinWebUI.csproj", "JoinWebUI/"]
COPY ["ModelData/ModelData.csproj", "ModelData/"]
RUN dotnet restore "JoinWebUI/JoinWebUI.csproj"
RUN dotnet restore "ModelData/ModelData.csproj"
RUN apt-get update && apt-get install -y nodejs

COPY . .
WORKDIR "/src/JoinWebUI"
RUN dotnet build "JoinWebUI.csproj" -c JoinWebUIDockerStaging -o /app/build
 
FROM build AS publish
RUN dotnet publish JoinWebUI.csproj -c JoinWebUIDockerStaging -o /app/publish /p:UseAppHost=false
 
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
RUN mkdir -p /usr/share/nginx/html/.well-known/
RUN mkdir -p /usr/share/nginx/html/.well-known/acme-challenge/
COPY --from=publish /app/publish/wwwroot .
COPY --from=publish /app/publish/nginx.conf /etc/nginx/nginx.conf
COPY ./JoinWebUI/Configuration/mime.types /etc/nginx/mime.types
COPY ./JoinWebUI/Deployment/Staging/zy7i0MJSTC_NojMO1hG5ueKda589A0talQgqbnEZdo4 /usr/share/nginx/html/.well-known/acme-challenge/
COPY ./JoinWebUI/Deployment/Staging/certificate.pem /usr/local/share/ca-certificates/
COPY ./JoinWebUI/Deployment/Staging/star.digicorp.it.key /usr/local/share/ca-certificates/
COPY ./JoinWebUI/Deployment/Staging/cabundle.pem /usr/local/share/ca-certificates/
COPY ./JoinWebUI/Deployment/Staging/fullchain.pem /usr/local/share/ca-certificates/
RUN chmod 644 /usr/local/share/ca-certificates/certificate.pem && update-ca-certificates
RUN chmod 644 /usr/local/share/ca-certificates/star.digicorp.it.key && update-ca-certificates
RUN chmod 644 /usr/local/share/ca-certificates/cabundle.pem && update-ca-certificates
RUN chmod 644 /usr/local/share/ca-certificates/fullchain.pem && update-ca-certificates

#COPY ./JoinWebUI/Deployment/Staging/nginx-selfsigned.crt /usr/local/share/ca-certificates/   #per sviluppo
#COPY ./JoinWebUI/Deployment/Staging/nginx-selfsigned.key /usr/local/share/ca-certificates/   #per sviluppo
#RUN chmod 644 /usr/local/share/ca-certificates/nginx-selfsigned.crt && update-ca-certificates  #per sviluppo
#RUN chmod 644 /usr/local/share/ca-certificates/nginx-selfsigned.key && update-ca-certificates	#per sviluppo
#COPY --from=publish /app/publish/wwwroot .
#COPY JoinWebUI/nginx.conf /etc/nginx/nginx.conf
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "JoinWebUI.dll"]