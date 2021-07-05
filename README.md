# book-a-desk-commands
Write-side API for the Book a Desk office reservation system

## Docker Setup
To build the Docker image:

    docker build -t book-a-desk:build-1 --build-arg AWSREGION="ca-central-1" .

To run the image inside a container: 
    
    docker run -ti --rm -e AWS_KEYID='${AWS_KEY_ID}' -e AWS_SECRETKEY='${AWS_SECRET_KEY}' -e ASPNETCORE_ENVIRONMENT=Development -e AWS_REGION=ca-central-1 -p 8000:80 book-a-desk:build-1

 