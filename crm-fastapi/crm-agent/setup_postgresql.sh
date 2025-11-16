#!/bin/bash

# PostgreSQL Configuration and Setup Script
# This script sets up PostgreSQL for the CRM Agent application
# Run this after installing PostgreSQL on your system

set -e

echo "========================================="
echo "CRM Agent - PostgreSQL Setup"
echo "========================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
DB_USER="crm_user"
DB_NAME="crm_db"
DB_HOST="localhost"
DB_PORT="5432"

# Check if PostgreSQL is installed
if ! command -v psql &> /dev/null; then
    echo -e "${RED}❌ PostgreSQL is not installed${NC}"
    echo "Install PostgreSQL and try again:"
    echo "  Ubuntu/Debian: sudo apt-get install postgresql postgresql-contrib"
    echo "  macOS: brew install postgresql"
    echo "  Windows: https://www.postgresql.org/download/windows/"
    exit 1
fi

echo -e "${GREEN}✓ PostgreSQL found${NC}"
echo ""

# Check if PostgreSQL service is running
if ! sudo service postgresql status &> /dev/null; then
    echo -e "${YELLOW}⚠ PostgreSQL service appears to be down${NC}"
    echo "Starting PostgreSQL service..."
    sudo service postgresql start || sudo systemctl start postgresql
    echo -e "${GREEN}✓ PostgreSQL service started${NC}"
else
    echo -e "${GREEN}✓ PostgreSQL service is running${NC}"
fi

echo ""
echo "Creating database user and database..."
echo ""

# Check if user exists
if sudo -u postgres psql -tAc "SELECT 1 FROM pg_user WHERE usename = '$DB_USER'" | grep -q 1; then
    echo -e "${YELLOW}⚠ User '$DB_USER' already exists${NC}"
    echo "Do you want to:"
    echo "  1) Reset the user (drop and recreate)"
    echo "  2) Keep existing user"
    echo "  3) Exit"
    read -p "Enter choice (1-3): " choice
    
    case $choice in
        1)
            echo "Dropping existing user and database..."
            sudo -u postgres psql -c "DROP DATABASE IF EXISTS $DB_NAME;" || true
            sudo -u postgres psql -c "DROP USER IF EXISTS $DB_USER;" || true
            echo -e "${GREEN}✓ Cleaned up${NC}"
            ;;
        2)
            echo -e "${GREEN}✓ Keeping existing user${NC}"
            ;;
        3)
            echo "Exiting..."
            exit 0
            ;;
        *)
            echo -e "${RED}Invalid choice${NC}"
            exit 1
            ;;
    esac
fi

echo ""

# Prompt for password
read -sp "Enter password for database user '$DB_USER': " DB_PASSWORD
echo ""
read -sp "Confirm password: " DB_PASSWORD_CONFIRM
echo ""

if [ "$DB_PASSWORD" != "$DB_PASSWORD_CONFIRM" ]; then
    echo -e "${RED}❌ Passwords do not match${NC}"
    exit 1
fi

if [ -z "$DB_PASSWORD" ]; then
    echo -e "${RED}❌ Password cannot be empty${NC}"
    exit 1
fi

echo ""
echo "Setting up database..."

# Create user if it doesn't exist
if ! sudo -u postgres psql -tAc "SELECT 1 FROM pg_user WHERE usename = '$DB_USER'" | grep -q 1; then
    sudo -u postgres psql -c "CREATE USER $DB_USER WITH PASSWORD '$DB_PASSWORD';" || true
    echo -e "${GREEN}✓ Created user '$DB_USER'${NC}"
else
    echo -e "${GREEN}✓ User '$DB_USER' exists${NC}"
fi

# Create database
sudo -u postgres psql -c "CREATE DATABASE $DB_NAME OWNER $DB_USER;" || true
echo -e "${GREEN}✓ Created database '$DB_NAME'${NC}"

# Grant privileges
sudo -u postgres psql -c "GRANT CONNECT ON DATABASE $DB_NAME TO $DB_USER;"
sudo -u postgres psql -c "GRANT USAGE ON SCHEMA public TO $DB_USER;"
sudo -u postgres psql -c "GRANT CREATE ON SCHEMA public TO $DB_USER;"
sudo -u postgres psql -c "ALTER ROLE $DB_USER WITH CREATEDB CREATEROLE;" || true
echo -e "${GREEN}✓ Granted privileges${NC}"

echo ""
echo "Verifying connection..."

# Test connection
PGPASSWORD="$DB_PASSWORD" psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c "SELECT 1;" > /dev/null 2>&1

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Connection successful${NC}"
else
    echo -e "${RED}❌ Connection failed${NC}"
    exit 1
fi

echo ""
echo "========================================="
echo -e "${GREEN}✓ Setup Complete!${NC}"
echo "========================================="
echo ""
echo "Update your .env file with:"
echo ""
echo "DATABASE_URL=postgresql+asyncpg://$DB_USER:$DB_PASSWORD@$DB_HOST:$DB_PORT/$DB_NAME"
echo ""
echo "For production, ensure:"
echo "  - Use a strong, unique password (not shown above)"
echo "  - Store credentials securely (not in version control)"
echo "  - Configure PostgreSQL for remote access if needed"
echo "  - Set up regular backups"
echo ""
echo "Next steps:"
echo "  1. Update .env file with the connection string above"
echo "  2. Run: python test_database.py"
echo "  3. Run: docker-compose up -d (for containerized deployment)"
echo ""
