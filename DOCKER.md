# Docker Deployment Guide - Mijn Energie

## Quick Start (Local Development)

### Prerequisites
- Docker Desktop installed
- Git installed

### Steps

1. **Clone the repository:**
```bash
git clone https://github.com/Achiusk/Group-Project.git
cd Group-Project
```

2. **Start all services:**
```bash
docker-compose up -d
```

3. **Access the application:**
- Web App: http://localhost:8080
- phpMyAdmin: http://localhost:8081 (optional)

4. **Stop all services:**
```bash
docker-compose down
```

---

## Educloud Deployment

### Option 1: Using Docker Compose

1. **Upload files to Educloud:**
   - `Dockerfile`
   - `docker-compose.yml`
   - `docker/` folder
   - `urban_city_power_managment.Web/` folder

2. **SSH into Educloud and run:**
```bash
docker-compose up -d --build
```

### Option 2: Build and Push to Registry

1. **Build the image locally:**
```bash
docker build -t mijn-energie:latest .
```

2. **Tag for your registry:**
```bash
docker tag mijn-energie:latest your-registry/mijn-energie:latest
```

3. **Push to registry:**
```bash
docker push your-registry/mijn-energie:latest
```

4. **On Educloud, pull and run:**
```bash
docker pull your-registry/mijn-energie:latest
docker run -d -p 8080:8080 --name mijn-energie your-registry/mijn-energie:latest
```

---

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment mode | Production |
| `ConnectionStrings__DefaultConnection` | MySQL connection string | See docker-compose.yml |

### Custom Database Connection

```bash
docker run -d -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=Server=your-host;Port=3306;Database=EindhovenEnergy;User=user;Password=pass;" \
  mijn-energie:latest
```

---

## Database Access

### MySQL Credentials (Docker)
- **Host:** localhost (or `mysql` from within containers)
- **Port:** 3306
- **Database:** EindhovenEnergy
- **User:** energie_user
- **Password:** Energie123!
- **Root Password:** rootpassword123

### phpMyAdmin
- URL: http://localhost:8081
- Login with above credentials

---

## Useful Commands

```bash
# View running containers
docker ps

# View logs
docker-compose logs -f web
docker-compose logs -f mysql

# Restart services
docker-compose restart

# Rebuild after code changes
docker-compose up -d --build

# Remove everything (including data)
docker-compose down -v

# Access MySQL CLI
docker exec -it mijn-energie-mysql mysql -u energie_user -pEnergie123! EindhovenEnergy

# Access container shell
docker exec -it mijn-energie-web /bin/bash
```

---

## Troubleshooting

### App won't start
```bash
# Check logs
docker-compose logs web

# Ensure MySQL is healthy
docker-compose ps
```

### Database connection failed
```bash
# Wait for MySQL to be ready (takes ~30 seconds on first start)
docker-compose logs mysql

# Verify connection string in environment
docker exec mijn-energie-web printenv | grep Connection
```

### Port already in use
```bash
# Change ports in docker-compose.yml
ports:
  - "9090:8080"  # Use 9090 instead of 8080
```

---

## Architecture

```
+-------------------+  +-------------------+
|   Web Browser     |   |    phpMyAdmin     |
|Port: 8080      |     |    Port: 8081     |
+--------+----------+     +--------+----------+
         |        |
     v     v
+--------+-------------------------+----------+
|           Docker Network         |
|            |
|  +---------------+    +------------------+  |
|  | Blazor Web    |  | MySQL Database   |  |
|  | Container     +--->| Container |  |
|  | Port: 8080    |    | Port: 3306       |  |
|  +---------------++------------------+  |
|     |              |
|            v      |
|               +------+------+       |
|  | mysql-data  | |
|         | (Volume)    |       |
|       +-------------+       |
+---------------------------------------------+
```

---

## Team Members

Share these files with your team:
- `Dockerfile`
- `docker-compose.yml`
- `.dockerignore`
- `docker/mysql-init/01-init.sql`
- `DOCKER.md` (this file)

Everyone can run `docker-compose up -d` to get the same environment!
