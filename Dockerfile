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
RUN addgroup -S csapp && \
    adduser -S csapp -G csapp && \
    mkdir -p /home/csapp && \
    chown -R csapp:csapp /home/csapp

### run as csapp user
USER csapp

### copy the app
COPY --from=build /app .

ENTRYPOINT [ "dotnet",  "csapp.dll" ]
