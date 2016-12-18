# Get images
docker run hello-world

#To try something more ambitious, you can run an Ubuntu container with:
docker run -it ubuntu bash

# download docker image
docker pull nginx

docker rm -f webserver1

#Create a container to run nginx 
docker run -d -p 80:80 --name webserver1 nginx


docker ps
 
# remove the container
docker rm -f webserver

docker start webserver
docker stop webserver

# remove the image 
docker rmi -f nginx 

docker images

# Remove dangling volume

docker volume rm $(docker volume ls -f dangling=true -q)

# List volume
docker volume ls

docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=hardtoguesspassword' -p 1433:1433 -v e:/docker/mssql -d microsoft/mssql-server-linux

#mounting volume in windows host
docker run -v e:\docker:/mssql

# mount host volume to container volume 
# e:/docker/mssql is the host volume
#:/data is container volume
docker run --rm -v e:/docker/mssql:/data alpine /bin/sh

# write to host volume e:/docker/mssql
/# /s > data/file3.txt


#save image 
docker commit 38e4cbb8afed my_ms_sql_1
docker save -o e:/docker/mssql/my_ms_sql_1.tar my_ms_sql_1

#reload image from host file
docker load -i e:/docker/my_mssql_1.tar
docker images
docker 

# run the ms sql images again , data should be present
docker run -p 1433:1433 -d my_mssql_1

