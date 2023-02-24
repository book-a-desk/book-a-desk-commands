# book-a-desk-commands
Write-side API for the Book a Desk office reservation system

## Docker Setup to interact with production
To build the Docker image:

    docker build -t book-a-desk .
    
To run the image inside a container: 
    
    docker run -d -p 8080:80 book-a-desk

## Docker Setup for local development
Building the local image:

    docker build -f docker/Dockerfile -t book-a-desk:local .

Running Book A Desk with a local dynamo db instance:

    docker compose -f docker/docker-compose.yml up

Environment variables are declared in `docker/.env`.
Please create that file with this template
```
AWS_REGION="<aws-region>"
AWS_ACCESS_KEY_ID="DUMMYIDEXAMPLE"
AWS_SECRET_ACCESS_KEY="DUMMYEXAMPLEKEY"
DynamoDB__ReservationTableName="ReservationEvents"
DynamoDB__OfficeTableName="Offices"
AWS_DEVELOPMENTSTORAGE="true"
AWS_DEVELOPMENTURL="http://dynamodb-local:8000"
DOMAINNAME="broadsign.com"
FeatureFlags__BookingCancellation="true"
FeatureFlags__GetBookings="true"
Okta__OktaDomain="<your-okta-domain>"
Okta__OktaAudience="<your-okta-client-id>"
```

This will start a local Book a Desk container listening to port 5000 and a dynamo db local instance listening on port 9000.

### One Time setup
AWS Client configuration

Create two new files with the aws developer configuration:
- C:\Users\USERNAME\.aws\config
```
    [default]
    region = your_aws_region
```
Note: us-east-2 is the region of book-a-desk.dev environment.

- C:\Users\USERNAME\.aws\credentials

```
    [default]
    aws_access_key_id = your_access_key_id
    aws_secret_access_key = your_secret_access_key
    aws_session_token= your_session_token
```
Note: Get credentials from book-a-desk.dev from `Command line or programmatic access`.

Browse [this](https://docs.aws.amazon.com/sdk-for-java/v1/developer-guide/setup-credentials.html) page for more detail information about aws development configuration. It applies to different languages.

After running `docker compose up`, you will need to create the tables required for Book a Desk by running these commands (requires aws cli):

    aws dynamodb create-table --endpoint-url "http://localhost:9000" --table-name ReservationEvents --key-schema AttributeName=EventId,KeyType=HASH --attribute-definitions AttributeName=EventId,AttributeType=S --provisioned-throughput ReadCapacityUnits=1,WriteCapacityUnits=1

If you want to reset your table you can delete it using the following command:

    aws dynamodb delete-table --endpoint-url "http://localhost:9000"--table-name ReservationEvents
