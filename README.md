# ecommerce_microservices
A practices dotnet microservices for a shopping service - Designed using .NET 5

### To run project locally
Ensure .NET is installed on your local machine
Install Docker on your machine for the local database or setup locally for the various storage medium

For Mongo: pull the image and run
docker pull mongo
docker run -d --name catalog-mongo -p 27017:27017 -v /mylocalstorage:/data/db
docker exec -it catalog-mongo bin/bash
mongo

Change Directory into one of the local project
Run the command "dotnet restore"
dotnet watch run || dotnet run

Or build the project using the following docker compose command
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
docker-compose -f docker-compose.yml -f docker-compose.override.yml down
