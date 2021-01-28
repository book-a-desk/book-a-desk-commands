FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build-env
WORKDIR /app
COPY Book-A-Desk.Api/*.fsproj ./Book-A-Desk.Api/
COPY Book-A-Desk.Domain/*.fsproj ./Book-A-Desk.Domain/
COPY Book-A-Desk.Core/*.fsproj ./Book-A-Desk.Core/
RUN dotnet restore ./Book-A-Desk.Api/Book-A-Desk.Api.fsproj
COPY . ./
RUN dotnet publish \
        --configuration release \
        --output out \
        --no-restore \
        ./Book-A-Desk.Api

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
WORKDIR /app
EXPOSE 80
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Book-A-Desk.Api.dll"]
