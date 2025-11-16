#!/bin/bash
# PostgreSQL Setup Script for CRM Agent
# This script sets up the PostgreSQL database for the CRM Agent application

set -e

echo "🔧 CRM Agent - PostgreSQL Setup Script"
echo "========================================"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
DB_USER="crm_user"
DB_PASSWORD="crm_password"  # CHANGE THIS IN PRODUCTION!
DB_NAME="crm_db"
DB_HOST="localhost"
DB_PORT="5432"

echo -e "${YELLOW}Configuration:${NC}"
echo "  Database User: $DB_USER"
echo "  Database Name: $DB_NAME"
echo "  Host: $DB_HOST"
echo "  Port: $DB_PORT"
echo ""

# Check if PostgreSQL is running
echo -e "${YELLOW}Checking PostgreSQL...${NC}"
if command -v psql &> /dev/null; then
    echo -e "${GREEN}✓ PostgreSQL is installed${NC}"
else
    echo -e "${RED}✗ PostgreSQL is not installed${NC}"
    echo "  Install PostgreSQL:"
    echo "  - Ubuntu/Debian: sudo apt-get install postgresql postgresql-contrib"
    echo "  - macOS: brew install postgresql@15"
    echo "  - Windows: Download from https://www.postgresql.org/download/windows/"
    exit 1
fi

# Try to connect to PostgreSQL
echo -e "${YELLOW}Testing PostgreSQL connection...${NC}"
if psql -h $DB_HOST -U postgres -c "SELECT version();" > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Connected to PostgreSQL${NC}"
else
    echo -e "${YELLOW}⚠ Could not connect as postgres user${NC}"
    echo "  Note: PostgreSQL should be running. Start it with:"
    echo "  - sudo systemctl start postgresql  (Linux)"
    echo "  - brew services start postgresql@15  (macOS)"
    echo "  - Windows: Start PostgreSQL service from Services"
fi

echo ""
echo -e "${YELLOW}Creating database user and database...${NC}"

# Create user and database
PSQL_CMD="psql -h $DB_HOST -U postgres"

# Create user
echo "  Creating user '$DB_USER'..."
$PSQL_CMD -c "CREATE USER $DB_USER WITH PASSWORD '$DB_PASSWORD';" 2>/dev/null || {
    echo "  User might already exist, updating password..."
    $PSQL_CMD -c "ALTER USER $DB_USER WITH PASSWORD '$DB_PASSWORD';"
}

# Create database
echo "  Creating database '$DB_NAME'..."
$PSQL_CMD -c "CREATE DATABASE $DB_NAME OWNER $DB_USER;" 2>/dev/null || {
    echo "  Database might already exist, skipping creation..."
}

# Grant permissions
echo "  Granting permissions..."
$PSQL_CMD -c "GRANT ALL PRIVILEGES ON DATABASE $DB_NAME TO $DB_USER;"
$PSQL_CMD -d $DB_NAME -c "GRANT USAGE ON SCHEMA public TO $DB_USER;"
$PSQL_CMD -d $DB_NAME -c "GRANT CREATE ON SCHEMA public TO $DB_USER;"

echo ""
echo -e "${GREEN}✓ Database setup completed!${NC}"
echo ""
echo -e "${YELLOW}Connection String:${NC}"
echo "  postgresql+asyncpg://$DB_USER:$DB_PASSWORD@$DB_HOST:$DB_PORT/$DB_NAME"
echo ""
echo -e "${YELLOW}Testing connection with new user...${NC}"
if PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c "SELECT 'Connection successful!' as status;" > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Successfully connected as $DB_USER${NC}"
else
    echo -e "${RED}✗ Could not connect as $DB_USER${NC}"
    echo "  Check your PostgreSQL installation and settings"
    exit 1
fi

echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "  1. Update .env file with the connection string above"
echo "  2. Run: python -m uvicorn app.main:app --reload"
echo "  3. The app will create tables automatically on startup"
echo ""
echo -e "${GREEN}✓ Setup complete!${NC}"
