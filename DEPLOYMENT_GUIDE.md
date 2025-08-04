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

---

## **Step 1: Connect to Your Droplet**

```bash
ssh root@bin-buddies.site
```

---

## **Step 2: Set Up Repository**

### **Option A: Separate Source Directory (Recommended)**
```bash
# Install Git if not already installed
apt update && apt install -y git

# Create a dedicated directory for the source code
mkdir -p /opt/binbuddies-source
cd /opt/binbuddies-source

# Clone your repository
git clone https://github.com/nwbarkeriu/bin-buddies.git
cd bin-buddies
```

### **Option B: Use Existing /var/www/bin-buddies**
```bash
# Install Git if not already installed
apt update && apt install -y git

# Navigate to your existing directory
cd /var/www/bin-buddies

# If it's not a git repo yet, initialize and clone
git init
git remote add origin https://github.com/nwbarkeriu/bin-buddies.git
git pull origin main

# Or if it's empty, clone directly
# rm -rf /var/www/bin-buddies/*
# git clone https://github.com/nwbarkeriu/bin-buddies.git .
```

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

### **PostgreSQL (Recommended for Production)**

```bash
# Run the database setup script
./setup-database.sh

# Update the database password
nano /var/www/bin-buddies/appsettings.Production.json
```

**Example PostgreSQL connection string:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=binbuddiesdb;Username=binbuddies_user;Password=YOUR_SECURE_PASSWORD;Port=5432;Pooling=true;"
  }
}
```

---

## **Step 5: Build and Deploy Application**

### **Option A: If using separate source directory**
```bash
# Build the application
cd /opt/binbuddies-source/bin-buddies
dotnet publish --configuration Release --output ./publish

# Copy published files to web directory
cp -r ./publish/* /var/www/bin-buddies/

# Set proper permissions
chown -R www-data:www-data /var/www/bin-buddies
chmod -R 755 /var/www/bin-buddies
```

### **Option B: If source is in /var/www/bin-buddies**
```bash
# Build the application in place
cd /var/www/bin-buddies
dotnet publish --configuration Release --output ./publish

# Copy published files to root (overwriting source files)
cp -r ./publish/* ./

# Clean up source files that shouldn't be in web root
rm -rf ./Pages ./Models ./Services ./Data ./Migrations ./Properties
rm -f ./*.cs ./*.csproj ./Program.cs

# Set proper permissions
chown -R www-data:www-data /var/www/bin-buddies
chmod -R 755 /var/www/bin-buddies
```

---

## **Step 6: Configure Database Connection**

```bash
# Edit production settings
nano /var/www/bin-buddies/appsettings.Production.json
```

**Update with your actual database credentials**

---

## **Step 7: Install Entity Framework Tools and Run Migrations**

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Add to PATH
export PATH="$PATH:/root/.dotnet/tools"

# Navigate to application directory
cd /var/www/bin-buddies

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

## **🔄 Future Updates**

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
cp -r ./publish/* /var/www/bin-buddies/

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

# Clean up source files
rm -rf ./Pages ./Models ./Services ./Data ./Migrations ./Properties
rm -f ./*.cs ./*.csproj ./Program.cs

# Set permissions
chown -R www-data:www-data /var/www/bin-buddies

# Run any new migrations
dotnet ef database update --environment Production

# Start application
systemctl start binbuddies
```

---

## **🎯 Access Your Application**

- **Main Site**: https://bin-buddies.site
- **Mobile Landing**: https://bin-buddies.site/mobile

---

## **🔧 Troubleshooting**

### **Application Won't Start**
```bash
# Check logs
journalctl -u binbuddies -n 50

# Check application directory permissions
ls -la /var/www/bin-buddies

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

---

## **🎊 Success!**

Your Blazor Bin Buddies application should now be live at **https://bin-buddies.site** with the mobile interface at **https://bin-buddies.site/mobile**!
