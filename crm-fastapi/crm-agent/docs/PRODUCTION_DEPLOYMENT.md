# Production Deployment Guide

## Overview
This guide covers deploying the CRM Agent to production using Docker Compose.

---

## Prerequisites

- Docker Engine 20.10+
- Docker Compose 2.0+
- Git
- (Optional) Docker Hub account for image registry

---

## Pre-Deployment Checklist

- [ ] `.env` file configured with production secrets
- [ ] PostgreSQL database created and tested
- [ ] OpenAI API key verified and working
- [ ] SSL certificates obtained (for HTTPS)
- [ ] Domain name configured
- [ ] Backup strategy in place
- [ ] Monitoring/logging setup planned

---

## Step 1: Prepare Production Environment

### 1.1 Create Production .env File

```bash
# Navigate to project directory
cd /home/husain/crosscrm/crm-fastapi

# Create production .env
cp .env .env.production

# Edit with production values
nano .env.production
```

**Production .env example:**

```bash
# OpenAI Configuration (Production)
OPENAI_API_KEY=sk-proj-xxxxxxxxxxxxx
OPENAI_MODEL=gpt-4

# Database Configuration (Production)
DATABASE_URL=postgresql+asyncpg://crm_user:strong_password_here@postgres:5432/crm_db

# Application Settings
DEBUG=False
APP_NAME=CRM Agent
APP_VERSION=1.0.0

# Production Settings
ENVIRONMENT=production
LOG_LEVEL=info
```

### 1.2 Secure Credentials

```bash
# Restrict .env file permissions
chmod 600 .env.production

# Ensure it's not tracked by git
echo ".env.production" >> .gitignore
git add .gitignore
git commit -m "Add .env.production to gitignore"
```

---

## Step 2: Build Docker Images

### 2.1 Build for Production

```bash
# Build the Docker image
docker build -f docker/Dockerfile -t crm-agent:latest .

# Tag for registry (optional)
docker tag crm-agent:latest your-registry/crm-agent:latest
```

### 2.2 Verify Image

```bash
# List images
docker images | grep crm-agent

# Run health check
docker run --rm crm-agent:latest python -c "import app.main; print('✓ App loads successfully')"
```

---

## Step 3: Deploy with Docker Compose

### 3.1 Production Compose File

Use `docker/docker-compose.yml` with modifications for production:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: crm_postgres_prod
    environment:
      POSTGRES_USER: crm_user
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_DB: crm_db
    volumes:
      - postgres_data_prod:/var/lib/postgresql/data
      - ./backups:/backups
    ports:
      - "5432:5432"
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U crm_user -d crm_db"]
      interval: 10s
      timeout: 5s
      retries: 5

  app:
    image: crm-agent:latest
    container_name: crm_agent_prod
    environment:
      DATABASE_URL: postgresql+asyncpg://crm_user:${DB_PASSWORD}@postgres:5432/crm_db
      OPENAI_API_KEY: ${OPENAI_API_KEY}
      DEBUG: "False"
      ENVIRONMENT: production
    ports:
      - "8000:8000"
    depends_on:
      postgres:
        condition: service_healthy
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8000/"]
      interval: 30s
      timeout: 10s
      retries: 3

volumes:
  postgres_data_prod:
    driver: local

networks:
  default:
    name: crm_prod_network
    driver: bridge
```

### 3.2 Deploy

```bash
# Start services
docker-compose -f docker/docker-compose.yml up -d --env-file .env.production

# Verify services are running
docker-compose ps

# Check logs
docker-compose logs -f app
docker-compose logs -f postgres
```

---

## Step 4: Post-Deployment Verification

### 4.1 Health Checks

```bash
# Check API health
curl http://localhost:8000/

# Check database connection
curl http://localhost:8000/api/agents/list

# View Swagger UI
# Open browser: http://localhost:8000/docs
```

### 4.2 Database Verification

```bash
# Connect to database
docker-compose exec postgres psql -U crm_user -d crm_db

# Inside psql shell:
\dt                    # List tables
SELECT COUNT(*) FROM agent_runs;  # Check data
\q                     # Exit
```

### 4.3 Application Logs

```bash
# View application logs
docker-compose logs --tail=100 app

# Follow logs in real-time
docker-compose logs -f app
```

---

## Step 5: Backup and Recovery

### 5.1 Database Backup

```bash
# Manual backup
docker-compose exec postgres pg_dump -U crm_user crm_db > backup_$(date +%Y%m%d_%H%M%S).sql

# Scheduled backups (cron)
# Add to crontab: 0 2 * * * cd /path/to/crm-fastapi && docker-compose exec -T postgres pg_dump -U crm_user crm_db > backups/backup_$(date +\%Y\%m\%d).sql
```

### 5.2 Database Restore

```bash
# Restore from backup
cat backup_20251116_000000.sql | docker-compose exec -T postgres psql -U crm_user -d crm_db
```

---

## Step 6: Scaling and Load Balancing

### 6.1 Scale Multiple App Instances

```yaml
# In docker-compose.yml, add load balancer
services:
  nginx:
    image: nginx:latest
    container_name: crm_nginx
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
    depends_on:
      - app1
      - app2
      - app3

  app1:
    image: crm-agent:latest
    # ... configuration
  app2:
    image: crm-agent:latest
    # ... configuration
  app3:
    image: crm-agent:latest
    # ... configuration
```

---

## Step 7: Monitoring and Logging

### 7.1 Container Logs

```bash
# Aggregate logs
docker-compose logs --timestamps app postgres

# Export logs
docker-compose logs app > app_logs_$(date +%Y%m%d).txt
```

### 7.2 Metrics Collection

```bash
# View resource usage
docker stats crm_agent_prod crm_postgres_prod

# Setup monitoring with Prometheus (optional)
# See docker/monitoring/ for Prometheus setup
```

---

## Step 8: SSL/HTTPS Setup

### 8.1 Using Let's Encrypt

```bash
# Install certbot
sudo apt-get install certbot python3-certbot-nginx

# Get certificate
sudo certbot certonly --standalone -d your-domain.com

# Certificate location: /etc/letsencrypt/live/your-domain.com/
```

### 8.2 Configure Nginx for HTTPS

```nginx
server {
    listen 80;
    server_name your-domain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-domain.com;

    ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;

    location / {
        proxy_pass http://app:8000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## Troubleshooting

### Issue: Database Connection Failed

```bash
# Check PostgreSQL is running
docker-compose ps postgres

# Check logs
docker-compose logs postgres

# Verify credentials
docker-compose exec postgres psql -U crm_user -d crm_db -c "SELECT 1;"
```

### Issue: Application Won't Start

```bash
# View app logs
docker-compose logs app

# Rebuild image
docker-compose build --no-cache

# Restart
docker-compose restart app
```

### Issue: Port Already in Use

```bash
# Find process using port
sudo lsof -i :8000

# Kill process
sudo kill -9 <PID>

# Or change port in docker-compose.yml
```

---

## Maintenance

### Regular Tasks

```bash
# Daily: Check logs
docker-compose logs app

# Weekly: Database backup
docker-compose exec postgres pg_dump -U crm_user crm_db > backups/backup_$(date +%Y%m%d).sql

# Monthly: Update images
docker pull crm-agent:latest
docker-compose pull

# Quarterly: Security updates
sudo apt-get update && sudo apt-get upgrade
```

### Shutdown Procedure

```bash
# Stop services (keeps data)
docker-compose down

# Stop and remove volumes (destructive!)
docker-compose down -v

# Stop without affecting data
docker-compose stop
```

---

## Rollback Procedure

```bash
# If deployment fails, rollback:
docker-compose down

# Switch to previous image
docker tag crm-agent:previous crm-agent:latest

# Start previous version
docker-compose up -d
```

---

## Monitoring Commands

```bash
# Real-time container stats
watch -n 1 docker stats crm_agent_prod crm_postgres_prod

# Check disk usage
docker system df

# Inspect running containers
docker inspect crm_agent_prod

# Check network
docker network inspect crm_prod_network
```

---

## Security Best Practices

1. **Use strong database passwords**
2. **Enable HTTPS/SSL**
3. **Restrict network access**
4. **Regular backups**
5. **Update Docker images regularly**
6. **Use environment variables for secrets**
7. **Enable container restart policies**
8. **Monitor resource usage**
9. **Keep logs for audit trail**
10. **Regular security scans**

```bash
# Scan image for vulnerabilities
docker scan crm-agent:latest
```

---

## Support and Troubleshooting

For issues, check:
1. Docker logs: `docker-compose logs`
2. Application logs: `docker-compose logs app`
3. Database logs: `docker-compose logs postgres`
4. System logs: `journalctl -u docker -f`

---

**Deployment Completed!** 🎉

Your CRM Agent is now running in production.
