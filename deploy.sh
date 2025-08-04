#!/bin/bash

# Bin Buddies Deployment Script for Digital Ocean
echo "🚀 Starting Bin Buddies Deployment..."

# Update system
echo "📦 Updating system packages..."
apt update && apt upgrade -y

# Install required packages
echo "🔧 Installing required packages..."
apt install -y nginx supervisor curl apt-transport-https

# Install .NET 8 SDK and Runtime (need SDK for EF migrations)
echo "⚡ Installing .NET 8..."
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt update
apt install -y aspnetcore-runtime-8.0 dotnet-sdk-8.0

# Install EF Core tools globally
echo "🔧 Installing Entity Framework tools..."
dotnet tool install --global dotnet-ef

# Install PostgreSQL instead of SQL Server
echo "🗄️ Setting up PostgreSQL..."
apt install -y postgresql postgresql-contrib

# Start and enable PostgreSQL
systemctl start postgresql
systemctl enable postgresql

# Setup database and user
echo "� Setting up database..."
sudo -u postgres psql << 'EOSQL'
DROP DATABASE IF EXISTS binbuddiesdb;
DROP USER IF EXISTS binbuddies_user;
CREATE DATABASE binbuddiesdb;
CREATE USER binbuddies_user WITH ENCRYPTED PASSWORD 'BinBuddies2024!SecurePass';
GRANT ALL PRIVILEGES ON DATABASE binbuddiesdb TO binbuddies_user;
ALTER USER binbuddies_user CREATEDB;
\q
EOSQL

# Grant schema permissions
sudo -u postgres psql -d binbuddiesdb -c 'GRANT ALL ON SCHEMA public TO binbuddies_user;'
sudo -u postgres psql -d binbuddiesdb -c 'GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO binbuddies_user;'
sudo -u postgres psql -d binbuddiesdb -c 'GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO binbuddies_user;'

# Create application directory
echo "📁 Creating application directory..."
mkdir -p /var/www/bin-buddies
cd /var/www/bin-buddies

# Clone application from GitHub
echo "📥 Cloning application from GitHub..."
git clone https://github.com/nwbarkeriu/bin-buddies.git /var/www/bin-buddies

# Build the application
echo "� Building application..."
cd /var/www/bin-buddies
dotnet restore
dotnet build --configuration Release

# Run database migrations
echo "🗄️ Running database migrations..."
export PATH="$PATH:/root/.dotnet/tools"
ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations add InitialCreate --force 2>/dev/null || echo "Migrations already exist"
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update

# Set permissions
echo "🔐 Setting permissions..."
chown -R www-data:www-data /var/www/bin-buddies
chmod -R 755 /var/www/bin-buddies

# Create systemd service
echo "⚙️ Creating systemd service..."
cat > /etc/systemd/system/binbuddies.service << EOF
[Unit]
Description=Bin Buddies Blazor Application
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/bin-buddies/bin/Release/net8.0/BinBuddies.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=binbuddies
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
WorkingDirectory=/var/www/bin-buddies

[Install]
WantedBy=multi-user.target
EOF

# Configure Nginx
echo "🌐 Configuring Nginx..."
cat > /etc/nginx/sites-available/binbuddies << EOF
server {
    listen 80;
    server_name bin-buddies.site www.bin-buddies.site;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
        proxy_buffering off;
        proxy_read_timeout 100s;
        proxy_connect_timeout 100s;
        client_max_body_size 10m;
    }
}
EOF

# Enable the site
ln -sf /etc/nginx/sites-available/binbuddies /etc/nginx/sites-enabled/
rm -f /etc/nginx/sites-enabled/default

# Test Nginx configuration
nginx -t

# Enable and start services
echo "🔄 Starting services..."
systemctl daemon-reload
systemctl enable binbuddies
systemctl enable nginx
systemctl restart nginx
systemctl start binbuddies

echo "✅ Deployment complete!"
echo "🌐 Your application should be available at http://bin-buddies.site"
echo "📋 Optional next steps:"
echo "1. Setup SSL certificate with: certbot --nginx -d bin-buddies.site -d www.bin-buddies.site"
echo "2. Check application status: systemctl status binbuddies"
echo "3. View logs: journalctl -u binbuddies -f"
