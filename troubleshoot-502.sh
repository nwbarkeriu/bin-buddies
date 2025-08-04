#!/bin/bash

# Bin Buddies 502 Error Troubleshooting Script
# Run this to diagnose and fix common 502 errors

echo "🔍 Bin Buddies 502 Error Troubleshooting"
echo "========================================"

echo ""
echo "1️⃣ Checking service status..."
echo "Bin Buddies Service: $(sudo systemctl is-active binbuddies.service 2>/dev/null || echo 'inactive')"
echo "Nginx Service: $(sudo systemctl is-active nginx 2>/dev/null || echo 'inactive')"

echo ""
echo "2️⃣ Checking if application is listening on port 5000..."
if sudo netstat -tlnp | grep -q ":5000"; then
    echo "✅ Something is listening on port 5000"
    sudo netstat -tlnp | grep ":5000"
else
    echo "❌ Nothing is listening on port 5000"
fi

echo ""
echo "3️⃣ Checking recent application logs..."
echo "Last 20 lines from binbuddies service:"
sudo journalctl -u binbuddies.service --no-pager -n 20

echo ""
echo "4️⃣ Checking nginx error logs..."
echo "Recent nginx errors:"
sudo tail -20 /var/log/nginx/error.log 2>/dev/null || echo "No nginx error log found"

echo ""
echo "5️⃣ Testing local application response..."
if curl -f -s -m 5 http://localhost:5000 > /dev/null; then
    echo "✅ Application responds to localhost:5000"
else
    echo "❌ Application does not respond to localhost:5000"
    echo "Testing response with verbose output:"
    curl -I -m 5 http://localhost:5000 2>&1 || echo "No response"
fi

echo ""
echo "6️⃣ Checking configuration files..."
echo "Production config exists: $(test -f /var/www/bin-buddies/publish/appsettings.Production.json && echo 'Yes' || echo 'No')"
echo "Main DLL exists: $(test -f /var/www/bin-buddies/publish/BinBuddies.dll && echo 'Yes' || echo 'No')"

echo ""
echo "7️⃣ Quick fixes to try:"
echo "🔧 Fix 1: Restart the application service"
echo "   sudo systemctl restart binbuddies.service"
echo ""
echo "🔧 Fix 2: Check database connection (if using PostgreSQL)"
echo "   sudo -u postgres psql -d binbuddiesdb -c 'SELECT version();'"
echo ""
echo "🔧 Fix 3: Rebuild and restart everything"
echo "   cd /var/www/bin-buddies && bash update-deploy.sh"
echo ""
echo "🔧 Fix 4: Check file permissions"
echo "   sudo chown -R www-data:www-data /var/www/bin-buddies"
echo "   sudo chmod -R 755 /var/www/bin-buddies"
echo ""

echo "8️⃣ If still having issues, run the enhanced update script:"
echo "   cd /var/www/bin-buddies && bash update-deploy.sh"
