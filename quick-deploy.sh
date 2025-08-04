#!/bin/bash

# Quick Deployment Script for Bin Buddies
# Run this on your Digital Ocean droplet: bash <(curl -s https://raw.githubusercontent.com/nwbarkeriu/bin-buddies/main/quick-deploy.sh)

set -e  # Exit on any error

echo "🚀 Starting Bin Buddies Quick Deployment..."

# Update system
echo "📦 Updating system..."
apt update && apt upgrade -y

# Install essential packages
echo "🔧 Installing essential packages..."
apt install -y nginx curl wget apt-transport-https software-properties-common

# Install .NET 8 Runtime
echo "⚡ Installing .NET 8..."
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt update
apt install -y aspnetcore-runtime-8.0 dotnet-sdk-8.0

# Install PostgreSQL
echo "🗄️ Installing PostgreSQL..."
apt install -y postgresql postgresql-contrib

# Start and enable PostgreSQL
systemctl start postgresql
systemctl enable postgresql

# Create database and user
echo "🔐 Setting up database..."
sudo -u postgres psql << 'EOF'
DROP DATABASE IF EXISTS binbuddiesdb;
DROP USER IF EXISTS binbuddies_user;
CREATE DATABASE binbuddiesdb;
CREATE USER binbuddies_user WITH ENCRYPTED PASSWORD 'BinBuddies2024!SecurePass';
GRANT ALL PRIVILEGES ON DATABASE binbuddiesdb TO binbuddies_user;
ALTER USER binbuddies_user CREATEDB;
\q
EOF

# Clone repository
echo "📥 Cloning repository..."
mkdir -p /opt/binbuddies-source
cd /opt/binbuddies-source
rm -rf bin-buddies
git clone https://github.com/nwbarkeriu/bin-buddies.git
cd bin-buddies

# Create application directory
echo "📁 Setting up application directory..."
mkdir -p /var/www/binbuddies
cp -r publish/* /var/www/binbuddies/

# Update connection string for PostgreSQL
echo "🔧 Configuring database connection..."
cat > /var/www/binbuddies/appsettings.Production.json << 'EOF'
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=binbuddiesdb;Username=binbuddies_user;Password=BinBuddies2024!SecurePass;Port=5432;Pooling=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "bin-buddies.site,www.bin-buddies.site"
}
EOF

# Install EF Core tools and update packages for PostgreSQL
echo "🔄 Installing Entity Framework tools..."
dotnet tool install --global dotnet-ef
export PATH="$PATH:/root/.dotnet/tools"

# Add PostgreSQL provider to the application
cd /var/www/binbuddies
# Note: This would require modifying the .csproj file to add Npgsql.EntityFrameworkCore.PostgreSQL

# Set permissions
echo "🔐 Setting permissions..."
chown -R www-data:www-data /var/www/binbuddies
chmod -R 755 /var/www/binbuddies

# Create systemd service
echo "⚙️ Creating systemd service..."
cat > /etc/systemd/system/binbuddies.service << 'EOF'
[Unit]
Description=Bin Buddies Blazor Application
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/binbuddies/BinBuddies.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=binbuddies
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
WorkingDirectory=/var/www/binbuddies

[Install]
WantedBy=multi-user.target
EOF

# Configure Nginx
echo "🌐 Configuring Nginx..."
cat > /etc/nginx/sites-available/binbuddies << 'EOF'
server {
    listen 80;
    server_name bin-buddies.site www.bin-buddies.site;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_buffering off;
        proxy_read_timeout 100s;
        proxy_connect_timeout 100s;
        client_max_body_size 10m;
    }
}
EOF

# Enable the site
rm -f /etc/nginx/sites-enabled/default
ln -sf /etc/nginx/sites-available/binbuddies /etc/nginx/sites-enabled/

# Test Nginx configuration
nginx -t

# Enable and start services
echo "🔄 Starting services..."
systemctl daemon-reload
systemctl enable binbuddies
systemctl enable nginx
systemctl restart nginx

# Note: Database migrations would need to be run manually due to PostgreSQL vs SQL Server differences

echo "✅ Deployment complete!"
echo ""
echo "🎯 Next Steps:"
echo "1. Your application should be running at http://bin-buddies.site"
echo "2. Install SSL certificate: certbot --nginx -d bin-buddies.site -d www.bin-buddies.site"
echo "3. Check application status: systemctl status binbuddies"
echo "4. View logs: journalctl -u binbuddies -f"
echo ""
echo "⚠️  Note: You may need to manually run database migrations if using PostgreSQL"
echo "   The application was built for SQL Server, so some adjustments may be needed."
echo ""
echo "🔐 Database credentials:"
echo "   Database: binbuddiesdb"
echo "   User: binbuddies_user"
echo "   Password: BinBuddies2024!SecurePass"
