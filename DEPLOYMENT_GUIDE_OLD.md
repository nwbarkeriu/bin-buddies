# 🚀 Bin Buddies - Digital Ocean Deployment Guide

## **Prerequisites**
- Digital Ocean Droplet running Ubuntu 22.04  
- Domain `bin-buddies.site` pointing to your droplet's IP
- SSH access to `root@bin-buddies.site`

### **🔍 Check Your Current Setup**

Since you mentioned you already have `/var/www/bin-buddies`, let's see what's there:

```bash
# Check what's currently in your directory
ls -la /var/www/bin-buddies/

# Check if it's a git repository
cd /var/www/bin-buddies && git status

# Check if it contains source files or published files
ls -la /var/www/bin-buddies/ | grep -E "(Pages|Models|Services|BinBuddies.dll)"
```

**Based on what you find:**
- **If you see source files** (Pages, Models, Services folders): Use **Option B** below
- **If you see published files** (BinBuddies.dll, wwwroot only): Use **Option A** below  
- **If directory is empty or minimal**: Choose either option
# Files are already in the right location, just ensure publish folder exists
# dotnet publish --configuration Release --output /var/www/bin-buddies/publish
# cp -r /var/www/bin-buddies/publish/* /var/www/bin-buddies/

# Set proper permissions (for both options)
chown -R www-data:www-data /var/www/bin-buddies
chmod -R 755 /var/www/bin-buddies

# Make sure only necessary files are in the web root
# Remove source files if they're mixed with published files
# rm -rf /var/www/bin-buddies/Pages /var/www/bin-buddies/Models /var/www/bin-buddies/Services
```unning Ubuntu 22.04
- Domain `bin-buddies.site` pointing to your droplet's IP
- SSH access to `root@bin-buddies.site`

---

## **Step 1: Connect to Your Droplet**

```bash
ssh root@bin-buddies.site
```

---

## **Step 2: Clone the Repository**

```bash
# Install Git if not already installed
apt update && apt install -y git

# Option A: If you want to keep source separate from running app (recommended)
mkdir -p /opt/binbuddies-source
cd /opt/binbuddies-source
git clone https://github.com/nwbarkeriu/bin-buddies.git
cd bin-buddies

# Option B: If you want to use your existing /var/www/bin-buddies as source
# cd /var/www/bin-buddies
# git clone https://github.com/nwbarkeriu/bin-buddies.git .
# (Note: This will put source files directly in your web directory)
```

> **📝 Directory Structure Options:**
> - **Option A (Recommended)**: Source in `/opt/binbuddies-source`, Running app in `/var/www/bin-buddies`
>   - **Pros**: Separates source from running application, cleaner updates
>   - **Cons**: Requires copying files between directories
> - **Option B**: Everything in `/var/www/bin-buddies`
>   - **Pros**: Simpler, single location
>   - **Cons**: Mixes source files with running application

---

## **Step 3: Run the Automated Deployment Script**

```bash
# Make scripts executable
chmod +x deploy.sh
chmod +x setup-database.sh

# Run the main deployment script
./deploy.sh
```

---

## **Step 4: Set Up Database**

### **Option A: PostgreSQL (Recommended for Production)**

```bash
# Run the database setup script
./setup-database.sh

# Update the database password
nano /var/www/binbuddies/appsettings.Production.json

# Change the connection string to use PostgreSQL
# Example:
# "DefaultConnection": "Host=localhost;Database=binbuddiesdb;Username=binbuddies_user;Password=YOUR_SECURE_PASSWORD"
```

### **Option B: SQL Server**

```bash
# Install SQL Server
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"
apt update
apt install -y mssql-server

# Configure SQL Server
/opt/mssql/bin/mssql-conf setup
systemctl enable mssql-server
```

---

## **Step 5: Copy Application Files**

```bash
# Copy the published application
cp -r /opt/binbuddies-source/bin-buddies/publish/* /var/www/binbuddies/

# Set proper permissions
chown -R www-data:www-data /var/www/binbuddies
chmod -R 755 /var/www/binbuddies
```

---

## **Step 6: Configure Database Connection**

```bash
# Edit production settings
nano /var/www/binbuddies/appsettings.Production.json
```

**Update the connection string with your actual database credentials:**

For PostgreSQL:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=binbuddiesdb;Username=binbuddies_user;Password=YOUR_SECURE_PASSWORD"
  }
}
```

For SQL Server:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BinBuddiesDb;User Id=sa;Password=YOUR_SA_PASSWORD;TrustServerCertificate=true;"
  }
}
```

---

## **Step 7: Install Entity Framework Tools and Run Migrations**

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Add to PATH
export PATH="$PATH:/root/.dotnet/tools"

# Navigate to application directory
cd /var/www/binbuddies

# Run database migrations
dotnet ef database update --environment Production
```

---

## **Step 8: Start the Application**

```bash
# Start the application service
systemctl start binbuddies

# Check status
systemctl status binbuddies

# Enable auto-start on boot
systemctl enable binbuddies

# Check application logs
journalctl -u binbuddies -f
```

---

## **Step 9: Configure SSL with Let's Encrypt**

```bash
# Install Certbot
apt install -y certbot python3-certbot-nginx

# Get SSL certificate
certbot --nginx -d bin-buddies.site -d www.bin-buddies.site

# Test automatic renewal
certbot renew --dry-run
```

---

## **Step 10: Final Verification**

```bash
# Check all services are running
systemctl status nginx
systemctl status binbuddies

# Test the application
curl -I http://bin-buddies.site
curl -I https://bin-buddies.site

# Check firewall (if enabled)
ufw status
ufw allow 'Nginx Full'
ufw allow OpenSSH
```

---

## **🎯 Access Your Application**

- **Main Site**: https://bin-buddies.site
- **Mobile Landing**: https://bin-buddies.site/mobile
- **Admin Panel**: Access through main navigation

---

## **🔧 Troubleshooting**

### **Application Won't Start**
```bash
# Check logs
journalctl -u binbuddies -n 50

# Check application directory permissions
ls -la /var/www/binbuddies

# Verify .NET runtime
dotnet --info
```

### **Database Connection Issues**
```bash
# Test database connection
psql -h localhost -U binbuddies_user -d binbuddiesdb

# Check database service
systemctl status postgresql
```

### **Nginx Issues**
```bash
# Test Nginx configuration
nginx -t

# Check Nginx logs
tail -f /var/log/nginx/error.log
```

---

## **🔄 Future Updates**

To update your application:

### **If using Option A (separate source directory):**
```bash
# Pull latest changes
cd /opt/binbuddies-source/bin-buddies
git pull origin main

# Rebuild application
dotnet publish --configuration Release --output ./publish

# Stop application
systemctl stop binbuddies

# Copy new files
cp -r /opt/binbuddies-source/bin-buddies/publish/* /var/www/bin-buddies/

# Set permissions
chown -R www-data:www-data /var/www/bin-buddies

# Run any new migrations
cd /var/www/bin-buddies
dotnet ef database update --environment Production

# Start application
systemctl start binbuddies
```

### **If using Option B (source in /var/www/bin-buddies):**
```bash
# Stop application first
systemctl stop binbuddies

# Pull latest changes
cd /var/www/bin-buddies
git pull origin main

# Rebuild application
dotnet publish --configuration Release --output ./publish

# Copy published files to root
cp -r ./publish/* ./

# Set permissions
chown -R www-data:www-data /var/www/bin-buddies

# Run any new migrations
dotnet ef database update --environment Production

# Start application
systemctl start binbuddies
```

---

## **📊 Monitoring**

```bash
# Monitor application performance
htop

# Check disk usage
df -h

# Monitor application logs
tail -f /var/log/nginx/access.log
journalctl -u binbuddies -f
```

---

## **🛡️ Security Checklist**

- ✅ SSL certificate installed
- ✅ Database passwords changed from defaults
- ✅ Firewall configured (UFW)
- ✅ Application running as www-data user
- ✅ Regular system updates scheduled

---

## **🎊 Success!**

Your Blazor Bin Buddies application should now be live at:
**https://bin-buddies.site**

The mobile-optimized interface is available at:
**https://bin-buddies.site/mobile**
