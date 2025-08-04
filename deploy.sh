#!/bin/bash

# Bin Buddies Deployment Script for Digital Ocean
echo "🚀 Starting Bin Buddies Deployment..."

# Update system
echo "📦 Updating system packages..."
apt update && apt upgrade -y

# Install required packages
echo "🔧 Installing required packages..."
apt install -y nginx supervisor curl apt-transport-https

# Install .NET 8 Runtime
echo "⚡ Installing .NET 8 Runtime..."
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt update
apt install -y aspnetcore-runtime-8.0

# Install SQL Server (if not already installed)
echo "🗄️ Setting up SQL Server..."
# Note: For production, you might want to use PostgreSQL instead
# apt install -y postgresql postgresql-contrib

# Create application directory
echo "📁 Creating application directory..."
mkdir -p /var/www/binbuddies
cd /var/www/binbuddies

# Clone or copy application files here
echo "📥 Application files should be copied to /var/www/binbuddies"

# Set permissions
echo "🔐 Setting permissions..."
chown -R www-data:www-data /var/www/binbuddies
chmod -R 755 /var/www/binbuddies

# Create systemd service
echo "⚙️ Creating systemd service..."
cat > /etc/systemd/system/binbuddies.service << EOF
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
WorkingDirectory=/var/www/binbuddies

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

echo "✅ Deployment script complete!"
echo "📋 Next steps:"
echo "1. Copy your application files to /var/www/binbuddies"
echo "2. Update database connection string in appsettings.Production.json"
echo "3. Run database migrations"
echo "4. Start the application: systemctl start binbuddies"
echo "5. Setup SSL certificate with Let's Encrypt"
