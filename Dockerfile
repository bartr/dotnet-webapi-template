### Build and Test the App
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

### copy the source and tests
COPY src /src

WORKDIR /src

# build the app
RUN dotnet publish -c Release -o /app

###########################################################


### Build the runtime container
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS release

### if port is changed, also update value in Config
EXPOSE 8080
WORKDIR /app

### create a user
RUN addgroup -S kl && \
    adduser -S kl -G kl && \
    mkdir -p /home/kl && \
    chown -R kl:kl /home/kl

### run as kl user
USER kl

### copy the app
COPY --from=build /app .

ENTRYPOINT [ "dotnet",  "app.dll" ]
