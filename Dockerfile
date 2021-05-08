FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build-env
ARG ENVIRONMENT
ARG AWSREGION
ARG AWSPROFILE
ARG AWSKEYID
ARG AWSSECRETKEY
WORKDIR /app
COPY Book-A-Desk.Api/*.fsproj ./Book-A-Desk.Api/
COPY Book-A-Desk.Domain/*.fsproj ./Book-A-Desk.Domain/
COPY Book-A-Desk.Core/*.fsproj ./Book-A-Desk.Core/
RUN dotnet restore ./Book-A-Desk.Api/Book-A-Desk.Api.fsproj
ENV HOME=/home
RUN mkdir -p /home/.aws
RUN touch $HOME/.aws/credentials
RUN echo "[default]" >> $HOME/.aws/credentials
RUN echo "aws_access_key_id = ${AWSKEYID}" >> $HOME/.aws/credentials
RUN echo "aws_secret_access_key = ${AWSSECRETKEY}" >> $HOME/.aws/credentials
COPY . ./
RUN dotnet publish \
        --configuration release \
        --output out \
        --no-restore \
        ./Book-A-Desk.Api

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
ARG ENVIRONMENT
ARG AWSREGION
ARG AWSPROFILE
ENV ENVIRONMENT=${ENVIRONMENT}
ENV AWS_REGION=${AWSREGION}
ENV AWS_PROFILE=${AWSPROFILE}
WORKDIR /app
EXPOSE 80
RUN mkdir -p /home/.aws
COPY --from=build-env /home/.aws/credentials /home/.aws/credentials
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Book-A-Desk.Api.dll"]
