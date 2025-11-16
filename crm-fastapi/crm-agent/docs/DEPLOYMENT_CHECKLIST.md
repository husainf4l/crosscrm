# Production Deployment Checklist

## Pre-Deployment Phase

### Environment Setup
- [ ] PostgreSQL 13+ installed and running
- [ ] Python 3.10+ available
- [ ] Docker and Docker Compose installed (for Docker deployment)
- [ ] Git repository cloned and up to date

### Code Preparation
- [ ] All tests passing
- [ ] Code reviewed and merged to main branch
- [ ] Documentation updated
- [ ] Version bumped in `pyproject.toml`
- [ ] CHANGELOG updated

### Configuration
- [ ] `.env.production` file created
- [ ] OpenAI API key validated
- [ ] Database URL verified
- [ ] All required environment variables set
- [ ] Sensitive data not committed to git

### Database Setup
- [ ] PostgreSQL user created with strong password
- [ ] Database created and owned by CRM user
- [ ] User has necessary permissions
- [ ] Backup strategy defined
- [ ] Backup tested and verified

---

## Deployment Phase

### Pre-Deployment Testing

```bash
# 1. Run database connection test
cd crm-agent
python test_database.py
```

Expected output:
```
✅ Connected to PostgreSQL
✅ Database tables initialized
✅ Found tables: (list of tables)
✅ Database connection test PASSED
```

### Docker Deployment

#### Build Phase
```bash
# 1. Build Docker image
docker build -f docker/Dockerfile -t crm-agent:latest .

# 2. Verify image
docker images | grep crm-agent

# 3. Test image runs
docker run --rm crm-agent:latest python -c "import app.main; print('✓')"
```

#### Start Phase
```bash
# 1. Start services with Docker Compose
docker-compose -f docker/docker-compose.yml up -d --env-file .env.production

# 2. Wait for services to be healthy
sleep 10

# 3. Check status
docker-compose ps
```

### Verification Phase

```bash
# 1. Health check
curl http://localhost:8000/

# 2. Database accessible
curl http://localhost:8000/api/agents/list

# 3. Swagger UI accessible
# Open in browser: http://localhost:8000/docs

# 4. Check application logs
docker-compose logs app

# 5. Check database logs
docker-compose logs postgres

# 6. Verify no errors
docker-compose logs app | grep -i "error\|exception" || echo "No errors found"
```

### Database Verification

```bash
# 1. Connect to database
docker-compose exec postgres psql -U crm_user -d crm_db

# 2. In psql:
SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public';

# 3. Exit
\q
```

---

## Post-Deployment Phase

### Monitoring

- [ ] Application responding to requests
- [ ] Database connections stable
- [ ] No error logs
- [ ] Response times acceptable (<3s for chat)
- [ ] CPU/Memory usage normal
- [ ] Disk space sufficient

### Backup Verification

```bash
# 1. Test backup
docker-compose exec postgres pg_dump -U crm_user crm_db > test_backup.sql

# 2. Verify backup file
ls -lh test_backup.sql

# 3. Test restore (in separate database)
# Create: psql -U postgres -c "CREATE DATABASE crm_db_test OWNER crm_user;"
# Restore: cat test_backup.sql | psql -U crm_user -d crm_db_test

# 4. Cleanup
# Drop: psql -U postgres -c "DROP DATABASE crm_db_test;"
rm test_backup.sql
```

### Security Checks

- [ ] HTTPS/SSL enabled (if public)
- [ ] Firewall configured
- [ ] No default credentials exposed
- [ ] Database password is strong
- [ ] .env file permissions restricted (600)
- [ ] Database backups encrypted

### Performance Baseline

```bash
# 1. Test response time
time curl http://localhost:8000/

# 2. Load test (optional)
# Using Apache Bench: ab -n 100 -c 10 http://localhost:8000/

# 3. Monitor resources
docker stats crm_agent_prod crm_postgres_prod
```

---

## Rollback Procedure (if needed)

```bash
# 1. Stop current services
docker-compose down

# 2. Restore from backup
docker-compose exec postgres psql -U crm_user crm_db < /backups/backup_latest.sql

# 3. Checkout previous version
git checkout <previous-tag>

# 4. Rebuild and restart
docker build -f docker/Dockerfile -t crm-agent:previous .
docker-compose up -d

# 5. Verify
curl http://localhost:8000/
```

---

## Maintenance Schedule

### Daily
- [ ] Check application logs for errors
- [ ] Monitor disk usage
- [ ] Verify backup completed

### Weekly
- [ ] Full database backup and verification
- [ ] Review performance metrics
- [ ] Check for security updates

### Monthly
- [ ] Update Docker images
- [ ] Security patch deployment
- [ ] Disaster recovery drill
- [ ] Database optimization

### Quarterly
- [ ] Major version updates
- [ ] Performance tuning review
- [ ] Security audit
- [ ] Capacity planning

---

## Troubleshooting Quick Reference

### Issue: Application won't start
```bash
# Check logs
docker-compose logs app

# Rebuild
docker-compose build --no-cache

# Restart
docker-compose restart app
```

### Issue: Database connection failed
```bash
# Check PostgreSQL is running
docker-compose ps postgres

# Verify credentials
PGPASSWORD=crm_password psql -U crm_user -d crm_db -c "SELECT 1;"

# Check connection string in .env
grep DATABASE_URL .env.production
```

### Issue: Port already in use
```bash
# Find process
sudo lsof -i :8000

# Kill process
sudo kill -9 <PID>
```

### Issue: Disk space low
```bash
# Check usage
docker system df

# Prune unused images
docker system prune -a
```

---

## Sign-Off

- [ ] Deployment lead: _________________ Date: _______
- [ ] QA verification: __________________ Date: _______
- [ ] Operations acceptance: ____________ Date: _______

---

## Contact & Escalation

- **Technical Lead**: [Contact Info]
- **DevOps Team**: [Contact Info]
- **On-Call**: [Contact Info]

---

**Deployment Completed Successfully!** ✅

All critical checks passed. System is ready for production use.
