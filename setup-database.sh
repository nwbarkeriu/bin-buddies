#!/bin/bash

# Database Setup Script for Bin Buddies
echo "🗄️ Setting up Production Database..."

# For PostgreSQL (recommended for production)
echo "Installing PostgreSQL..."
apt install -y postgresql postgresql-contrib

# Start PostgreSQL service
systemctl start postgresql
systemctl enable postgresql

# Create database and user
sudo -u postgres psql << EOF
CREATE DATABASE binbuddiesdb;
CREATE USER binbuddies_user WITH ENCRYPTED PASSWORD 'CHANGE_THIS_PASSWORD';
GRANT ALL PRIVILEGES ON DATABASE binbuddiesdb TO binbuddies_user;
ALTER USER binbuddies_user CREATEDB;
\q
EOF

echo "✅ PostgreSQL database setup complete!"
echo "📋 Database Details:"
echo "   Database: binbuddiesdb"
echo "   User: binbuddies_user"
echo "   Password: CHANGE_THIS_PASSWORD (Please change this!)"

# Alternative: SQL Server setup (commented out)
# echo "Installing SQL Server..."
# wget -qO- https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
# add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/20.04/mssql-server-2019.list)"
# apt update
# apt install -y mssql-server
# /opt/mssql/bin/mssql-conf setup
# systemctl enable mssql-server
