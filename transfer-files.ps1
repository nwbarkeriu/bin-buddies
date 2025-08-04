# Transfer files to server using SCP
$server = "root@bin-buddies.site"

# Copy the entire project
scp -o "KexAlgorithms=diffie-hellman-group14-sha256" -r . ${server}:/var/www/bin-buddies/

Write-Host "Files transferred to server"
