# book-a-desk-commands
Write-side API for the Book a Desk office reservation system

## Docker Setup
To build the Docker image:

    docker build -t book-a-desk .
    
To run the image inside a container: 
    
    docker run -d -p 8080:80 book-a-desk
 