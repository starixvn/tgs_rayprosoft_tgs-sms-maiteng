#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.Sms.MaiTeng/Tgs.Sms.MaiTeng.csproj", "Tgs.Sms.MaiTeng/"]
COPY ["Tgs.Sms.MaiTeng.Core/Tgs.Sms.MaiTeng.Core.csproj", "Tgs.Sms.MaiTeng.Core/"]
COPY ["Tgs.Sms.MaiTeng.Repositories/Tgs.Sms.MaiTeng.Repositories.csproj", "Tgs.Sms.MaiTeng.Repositories/"]
RUN dotnet restore "Tgs.Sms.MaiTeng/Tgs.Sms.MaiTeng.csproj"
COPY . .
WORKDIR "/src/Tgs.Sms.MaiTeng"
RUN dotnet build "Tgs.Sms.MaiTeng.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.Sms.MaiTeng.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Sms.MaiTeng.dll"]