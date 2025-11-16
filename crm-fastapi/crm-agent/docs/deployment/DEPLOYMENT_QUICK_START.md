# 🚀 Production Deployment Guide - Quick Start

## Prerequisites Checklist

Before you start, ensure you have:

```bash
✓ PostgreSQL 13+ installed and running
✓ Docker and Docker Compose installed
✓ Git repository cloned
✓ OpenAI API key available
✓ Python 3.10+ installed
```

**Verify prerequisites:**
```bash
psql --version          # PostgreSQL
docker --version        # Docker
docker-compose --version  # Docker Compose
python --version        # Python
git --version          # Git
```

---

## 🔧 Phase 1: Database Setup (5 minutes)

### Step 1: Run PostgreSQL Setup Script

```bash
cd /home/husain/crosscrm/crm-fastapi/crm-agent

# Make script executable (if not already)
chmod +x setup_postgresql.sh

# Run the setup script
./setup_postgresql.sh
```

**What it does:**
- ✓ Checks PostgreSQL installation
- ✓ Creates database user: `crm_user`
- ✓ Creates database: `crm_db`
- ✓ Sets up permissions
- ✓ Verifies connection

**Expected output:**
```
PostgreSQL found
PostgreSQL service is running
✓ Created user 'crm_user'
✓ Created database 'crm_db'
✓ Granted privileges
✓ Connection successful
```

### Step 2: Update .env File

Edit `.env` file in the project root:

```bash
nano /home/husain/crosscrm/crm-fastapi/crm-agent/.env
```

Replace the DATABASE_URL line with the connection string provided by the setup script:

```env
# From setup script output:
DATABASE_URL=postgresql+asyncpg://crm_user:YOUR_PASSWORD@localhost:5432/crm_db
```

**Save and close** (Ctrl+X, then Y, then Enter)

---

## ✅ Phase 2: Connection Verification (2 minutes)

### Test Database Connection

```bash
cd /home/husain/crosscrm/crm-fastapi/crm-agent

# Set Python path
export PYTHONPATH=/home/husain/crosscrm/crm-fastapi/crm-agent

# Run database test
python test_database.py
```

**Expected output:**
```
Testing database connection...
✅ Connected to PostgreSQL
✅ Version: PostgreSQL 13.0
✅ Database tables initialized
✅ Found tables: alembic_version, business_profiles, chat_messages, tasks
✅ Database connection test PASSED
Ready for deployment!
```

### If Test Fails:

**Error: "could not connect to server"**
```bash
# Check PostgreSQL is running
sudo service postgresql status

# If not running, start it
sudo service postgresql start
```

**Error: "password authentication failed"**
```bash
# Verify DATABASE_URL matches setup script output
grep DATABASE_URL .env

# Verify the password in .env matches what you set during setup
```

**Error: "database does not exist"**
```bash
# Run setup script again
./setup_postgresql.sh
```

---

## 🐳 Phase 3: Docker Deployment (5 minutes)

### Option A: Using Docker Compose (Recommended)

```bash
cd /home/husain/crosscrm/crm-fastapi/crm-agent

# Start all services
docker-compose -f docker/docker-compose.yml up -d

# Wait for services to be healthy
sleep 10

# Verify services are running
docker-compose ps
```

**Expected output:**
```
NAME                    STATUS              PORTS
crm_postgres_prod       Up (healthy)        5432/tcp
crm_agent_prod          Up (healthy)        0.0.0.0:8000->8000/tcp
```

### Option B: Manual Docker Build

```bash
cd /home/husain/crosscrm/crm-fastapi/crm-agent

# Build image
docker build -f docker/Dockerfile -t crm-agent:latest .

# Run container
docker run -d \
  --name crm-agent \
  -p 8000:8000 \
  --env-file .env \
  crm-agent:latest

# Verify it's running
docker ps | grep crm-agent
```

---

## 🔍 Phase 4: Verification (3 minutes)

### Health Checks

```bash
# 1. Application is responding
curl http://localhost:8000/

# Expected: 
# {"message":"CRM Agent API is running"}

# 2. API Documentation available
open http://localhost:8000/docs
# Or: curl http://localhost:8000/openapi.json

# 3. Chat endpoint works
curl -X POST http://localhost:8000/api/chat/message \
  -H "Content-Type: application/json" \
  -d '{
    "user_id": "test-user",
    "message": "Hello, test message"
  }'

# 4. View logs
docker-compose logs -f app

# 5. Database queries
docker-compose exec postgres psql -U crm_user -d crm_db -c "SELECT COUNT(*) FROM chat_messages;"
```

### Monitoring Resources

```bash
# Real-time resource usage
docker stats crm_agent_prod crm_postgres_prod

# Application logs
docker-compose logs app

# Database logs
docker-compose logs postgres

# Check for errors
docker-compose logs app | grep -i error
```

---

## 📊 Access Points

Once deployed, access the application at:

| Service | URL | Purpose |
|---------|-----|---------|
| **API** | http://localhost:8000 | REST API endpoints |
| **Docs** | http://localhost:8000/docs | Swagger UI documentation |
| **Chat** | http://localhost:8000/chat | Chat interface |
| **OpenAPI** | http://localhost:8000/openapi.json | OpenAPI specification |

---

## 🛑 Stopping Services

```bash
# Stop Docker Compose services
docker-compose down

# Stop specific container
docker stop crm_agent_prod
docker stop crm_postgres_prod

# Remove containers and volumes
docker-compose down -v
```

---

## 🔄 Restarting Services

```bash
# Restart all services
docker-compose restart

# Restart specific service
docker-compose restart app
docker-compose restart postgres

# Full restart (stop and start)
docker-compose down
docker-compose up -d
```

---

## 📦 Production Checklist

- [ ] PostgreSQL 13+ installed and tested
- [ ] Database created and verified with `test_database.py`
- [ ] .env file updated with correct DATABASE_URL
- [ ] All environment variables set (OPENAI_API_KEY, etc.)
- [ ] Docker images built successfully
- [ ] Services started with Docker Compose
- [ ] Health checks passing
- [ ] Chat interface accessible
- [ ] API endpoints responding
- [ ] No error messages in logs
- [ ] Database backups configured
- [ ] Firewall rules configured (if needed)
- [ ] SSL/HTTPS configured (if production)
- [ ] Monitoring configured (optional)

---

## 🐛 Troubleshooting

### Issue: "Address already in use"

```bash
# Find what's using port 8000
sudo lsof -i :8000

# Kill the process
sudo kill -9 <PID>

# Or use different port in docker-compose.yml
```

### Issue: "Connection refused"

```bash
# Check if database is running
docker-compose ps postgres

# Check database logs
docker-compose logs postgres

# Restart database
docker-compose restart postgres
```

### Issue: "out of memory"

```bash
# Check Docker resources
docker stats

# Increase Docker memory limit in Docker Desktop settings
# Or deploy on a server with more resources
```

### Issue: "permission denied"

```bash
# Check file permissions
ls -la setup_postgresql.sh
ls -la docker/

# Make executable
chmod +x setup_postgresql.sh
chmod +x docker/Dockerfile
```

---

## 📚 Additional Resources

- **Full Documentation**: `/docs/PRODUCTION_DEPLOYMENT.md`
- **Deployment Checklist**: `/docs/DEPLOYMENT_CHECKLIST.md`
- **API Reference**: `/docs/API.md`
- **Architecture Guide**: `/docs/ARCHITECTURE.md`
- **Setup Guide**: `/docs/SETUP.md`

---

## ✨ What's Next?

1. **Monitor the Application**
   - Check logs regularly
   - Monitor resource usage
   - Set up alerts

2. **Configure Backups**
   - Daily database backups
   - Store offsite
   - Test restore procedures

3. **Scale if Needed**
   - Use load balancer for multiple instances
   - Configure connection pooling
   - Monitor performance metrics

4. **Security Hardening**
   - Enable HTTPS/SSL
   - Configure firewall rules
   - Set up VPN access
   - Rotate credentials regularly

---

## 🎉 Deployment Complete!

Your CRM Agent is now running in production!

**Quick Summary:**
- ✅ Database: PostgreSQL running
- ✅ Application: FastAPI running on port 8000
- ✅ API Docs: Available at /docs
- ✅ Chat Interface: Available at /chat
- ✅ Agents: All 5 agents active and scheduled

For support, refer to the troubleshooting section or check the logs:
```bash
docker-compose logs -f app
```

---

**Last Updated**: 2024
**Version**: 1.0.0
**Status**: Production Ready ✅
