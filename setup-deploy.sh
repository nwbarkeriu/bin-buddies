#!/bin/bash

# Bin Buddies Complete Setup Script for Digital Ocean Droplet
# Run this once: bash <(curl -s https://raw.githubusercontent.com/nwbarkeriu/bin-buddies/main/setup-deploy.sh)

set -e  # Exit on any error

echo "🚀 Starting Bin Buddies Complete Setup..."

# Update system
echo "📦 Updating system packages..."
sudo apt update && sudo apt upgrade -y

# Install essential packages
echo "🔧 Installing essential packages..."
sudo apt install -y nginx curl wget apt-transport-https software-properties-common git

# Install .NET 8 SDK and Runtime
echo "⚡ Installing .NET 8..."
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0 dotnet-sdk-8.0

# Install Entity Framework Core tools globally
echo "🔧 Installing Entity Framework tools..."
dotnet tool install --global dotnet-ef
export PATH="$PATH:/root/.dotnet/tools"

# Install PostgreSQL (better for production than SQL Server on Linux)
echo "🗄️ Installing PostgreSQL..."
sudo apt install -y postgresql postgresql-contrib

# Start and enable PostgreSQL
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Create database and user
echo "🔐 Setting up database..."
sudo -u postgres psql << 'EOF'
DROP DATABASE IF EXISTS binbuddiesdb;
DROP USER IF EXISTS binbuddies_user;
CREATE DATABASE binbuddiesdb;
CREATE USER binbuddies_user WITH ENCRYPTED PASSWORD 'BinBuddies2024!SecurePass';
GRANT ALL PRIVILEGES ON DATABASE binbuddiesdb TO binbuddies_user;
ALTER USER binbuddies_user CREATEDB;
GRANT ALL ON SCHEMA public TO binbuddies_user;
\q
EOF

# Grant additional permissions
sudo -u postgres psql -d binbuddiesdb -c 'GRANT ALL ON SCHEMA public TO binbuddies_user;'
sudo -u postgres psql -d binbuddiesdb -c 'ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO binbuddies_user;'
sudo -u postgres psql -d binbuddiesdb -c 'ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO binbuddies_user;'

# Create application directory and clone repository
echo "📁 Setting up application directory..."
sudo mkdir -p /var/www/bin-buddies
cd /var/www
sudo chown $(whoami):$(whoami) bin-buddies
cd bin-buddies

echo "📥 Cloning repository..."
git clone https://github.com/nwbarkeriu/bin-buddies.git .

# Create production configuration for PostgreSQL
echo "🔧 Creating production configuration..."
cat > appsettings.Production.json << 'EOF'
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=binbuddiesdb;Username=binbuddies_user;Password=BinBuddies2024!SecurePass;Port=5432;Pooling=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "bin-buddies.site,www.bin-buddies.site,localhost"
}
EOF

# Build and publish the application
echo "🛠 Building and publishing application..."
dotnet restore
dotnet build --configuration Release
dotnet publish -c Release -o ./publish

# Run database migrations
echo "🗄️ Setting up database schema..."
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update

# Seed initial data
echo "📊 Seeding database with initial data..."
cd ./publish
ASPNETCORE_ENVIRONMENT=Production timeout 60s dotnet BinBuddies.dll --seed || echo "⚠️ Seeding completed or timed out"
cd ..

# Set proper permissions
echo "🔐 Setting permissions..."
sudo chown -R www-data:www-data /var/www/bin-buddies
sudo chmod -R 755 /var/www/bin-buddies

# Create systemd service
echo "⚙️ Creating systemd service..."
sudo tee /etc/systemd/system/binbuddies.service > /dev/null << 'EOF'
[Unit]
Description=Bin Buddies Blazor Application
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/bin-buddies/publish/BinBuddies.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=binbuddies
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
WorkingDirectory=/var/www/bin-buddies/publish

[Install]
WantedBy=multi-user.target
EOF

# Configure Nginx
echo "🌐 Configuring Nginx..."
sudo tee /etc/nginx/sites-available/binbuddies > /dev/null << 'EOF'
server {
    listen 80;
    server_name bin-buddies.site www.bin-buddies.site _;
    
    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header X-Content-Type-Options "nosniff" always;
    
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
    
    # Handle Blazor SignalR connections
    location /_blazor {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
EOF

# Enable the site and remove default
sudo rm -f /etc/nginx/sites-enabled/default
sudo ln -sf /etc/nginx/sites-available/binbuddies /etc/nginx/sites-enabled/

# Test Nginx configuration
sudo nginx -t

# Enable and start services
echo "🔄 Starting all services..."
sudo systemctl daemon-reload
sudo systemctl enable binbuddies
sudo systemctl enable nginx
sudo systemctl restart nginx
sudo systemctl start binbuddies

# Wait for services to start
sleep 10

# Check service status
echo "🔍 Checking service status..."
if sudo systemctl is-active --quiet binbuddies.service; then
    echo "✅ Bin Buddies service is running"
else
    echo "❌ Service failed to start. Checking logs..."
    sudo journalctl -u binbuddies.service --no-pager -n 20
fi

if sudo systemctl is-active --quiet nginx; then
    echo "✅ Nginx is running"
else
    echo "❌ Nginx failed to start"
fi

echo ""
echo "✅ Complete setup finished!"
echo ""
echo "🎯 Your Bin Buddies application is now available at:"
echo "   • http://bin-buddies.site (if domain is configured)"
echo "   • http://$(curl -s ifconfig.me) (your droplet's IP)"
echo ""
echo "🔐 Database Information:"
echo "   • Database: binbuddiesdb"
echo "   • User: binbuddies_user"
echo "   • Password: BinBuddies2024!SecurePass"
echo ""
echo "📋 Next Steps:"
echo "1. Configure your domain DNS to point to this server"
echo "2. Install SSL certificate: sudo certbot --nginx -d bin-buddies.site -d www.bin-buddies.site"
echo "3. For updates, run: bash /var/www/bin-buddies/update-deploy.sh"
echo ""
echo "🔧 Useful Commands:"
echo "   • Check logs: sudo journalctl -u binbuddies.service -f"
echo "   • Restart service: sudo systemctl restart binbuddies.service"
echo "   • Check status: sudo systemctl status binbuddies.service"
echo "   • Update deployment: cd /var/www/bin-buddies && bash update-deploy.sh"
