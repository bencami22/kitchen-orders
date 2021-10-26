FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["KitchenOrders/KitchenOrders.csproj", "KitchenOrders/"]
RUN dotnet restore "KitchenOrders/KitchenOrders.csproj"
COPY . .
WORKDIR "/src/KitchenOrders"
RUN dotnet build "KitchenOrders.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KitchenOrders.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KitchenOrders.dll"]
