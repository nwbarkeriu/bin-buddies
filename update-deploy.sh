#!/bin/bash

# Bin Buddies Enhanced Update Deployment Script
# Run this on your droplet after initial setup to deploy updates with new subscription system

set -e  # Exit on any error

echo "🔁 Pulling latest code from Git..."
cd /var/www/bin-buddies
git pull origin main || { echo "❌ Git pull failed"; exit 1; }

echo "🔧 Restoring .NET packages..."
dotnet restore || { echo "❌ Package restore failed"; exit 1; }

echo "🛠 Publishing app to production..."
dotnet publish -c Release -o ./publish || { echo "❌ Publish failed"; exit 1; }

echo "🗄️ Running database migrations..."
export PATH="$PATH:/root/.dotnet/tools"

# Stop the service before running migrations to avoid conflicts
echo "🛑 Stopping application service for migration..."
sudo systemctl stop binbuddies.service || echo "Service was not running"

# Run migrations with proper environment
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update || { 
    echo "❌ Database migration failed"; 
    echo "🔍 Checking migration status...";
    dotnet ef migrations list --environment Production;
    exit 1; 
}

echo "📊 Seeding database with subscription plans and roles..."
# Note: This will only add new data, won't duplicate existing records
cd ./publish

# Seed with longer timeout for new subscription data
ASPNETCORE_ENVIRONMENT=Production timeout 60s dotnet BinBuddies.dll --seed || echo "⚠️ Seeding completed or timed out"
cd ..

echo "🔐 Setting correct permissions..."
sudo chown -R www-data:www-data /var/www/bin-buddies
sudo chmod -R 755 /var/www/bin-buddies

echo "🚀 Starting Bin Buddies service..."
sudo systemctl start binbuddies.service
sudo systemctl enable binbuddies.service

echo "🔄 Reloading Nginx..."
sudo nginx -t || { echo "❌ Nginx configuration test failed"; exit 1; }
sudo systemctl reload nginx

echo "⏳ Waiting for service to start..."
sleep 10

echo "🔍 Checking service status..."
if sudo systemctl is-active --quiet binbuddies.service; then
    echo "✅ Bin Buddies service is running"
else
    echo "❌ Service failed to start. Checking logs..."
    sudo journalctl -u binbuddies.service --no-pager -n 30
    echo "🔧 Troubleshooting steps:"
    echo "   • Check database connection: sudo journalctl -u binbuddies.service | grep -i 'connection\\|database'"
    echo "   • Check port conflicts: sudo netstat -tlnp | grep :5000"
    echo "   • Check file permissions: ls -la /var/www/bin-buddies/publish/"
    exit 1
fi

echo "🌐 Testing application response..."
sleep 5  # Give the app more time to fully start
if curl -f -s http://localhost:5000 > /dev/null; then
    echo "✅ Application is responding"
else
    echo "⚠️ Application may not be responding on port 5000"
    echo "🔍 Testing alternative endpoints..."
    curl -I http://localhost:5000 || echo "No response from application"
fi

echo ""
echo "✅ Deployment complete!"
echo "🎯 Application Status:"
echo "   • Service: $(sudo systemctl is-active binbuddies.service)"
echo "   • Nginx: $(sudo systemctl is-active nginx)"
echo "   • Local URL: http://localhost:5000"
echo "   • Public URL: http://bin-buddies.site"
echo ""
echo "🆕 New Features Available:"
echo "   • Enhanced Landing Page: http://bin-buddies.site/"
echo "   • Customer Registration: http://bin-buddies.site/register"
echo "   • Employee Dashboard: http://bin-buddies.site/employee-dashboard"
echo "   • Manager Dashboard: http://bin-buddies.site/manager-dashboard"
echo ""
echo "📋 Useful Commands:"
echo "   • Check logs: sudo journalctl -u binbuddies.service -f"
echo "   • Restart service: sudo systemctl restart binbuddies.service"
echo "   • Check status: sudo systemctl status binbuddies.service"
echo "   • Check status: sudo systemctl status binbuddies.service"
