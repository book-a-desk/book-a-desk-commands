# book-a-desk-commands
Write-side API for the Book a Desk office reservation system

## Docker Setup to interact with production
To build the Docker image:

    `docker build -t book-a-desk .`
    
To run the image inside a container: 
    
    `docker run -d -p 8080:80 book-a-desk`

## Docker Setup for local development
Building the local image:

    `docker build -t book-a-desk:local .`

Running Book A Desk with a local dynamo db instance:

    `docker compose up`

This will start a local Book a Desk container listening to port 5000 and a dynamo db local instance listening on port 9000.

### One Time setup
After running `docker compose up`, you will need to create the tables required for Book a Desk by running these commands (requires aws cli):

    `aws dynamodb create-table --endpoint-url "http://localhost:9000" --table-name ReservationEvents --key-schema AttributeName=AggregateId,KeyType=HASH --attribute-definitions AttributeName=AggregateId,AttributeType=S --provisioned-throughput ReadCapacityUnits=1,WriteCapacityUnits=1`
