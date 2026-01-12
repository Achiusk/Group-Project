# Deployment Guide - Mijn Energie

## Option 1: Docker (Recommended for Local Development)

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

## Option 2: IIS Deployment (Educloud)

### Prerequisites
- .NET 9 SDK installed
- Access to Educloud server
- MySQL database on Educloud

### Step 1: Publish the Application

**Using Visual Studio:**
1. Right-click on `urban_city_power_managment.Web` project
2. Select **Publish**
3. Choose **Folder** as target
4. Set path to `./publish`
5. Click **Publish**

**Using Command Line:**
```bash
cd Group-Project
dotnet publish urban_city_power_managment.Web -c Release -o ./publish
```

### Step 2: Configure Database Connection

Edit `publish/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_EDUCLOUD_MYSQL_HOST;Port=3306;Database=EindhovenEnergy;User=YOUR_USER;Password=YOUR_PASSWORD;"
  }
}
```

### Step 3: Upload to Educloud

1. Connect to Educloud via **FileZilla** or **SFTP**
2. Upload entire `publish` folder contents to your web directory
3. Make sure these files are included:
   - `urban_city_power_managment.Web.dll`
   - `urban_city_power_managment.Web.exe`
   - `web.config`
   - `appsettings.json`
   - `wwwroot/` folder

### Step 4: Configure IIS

1. Open **IIS Manager** on Educloud
2. Create new website or application
3. Set **Physical Path** to your upload directory
4. Set **Application Pool** to `.NET CLR Version: No Managed Code`
5. Ensure **ASP.NET Core Hosting Bundle** is installed

### Step 5: Initialize Database

Run the SQL script on your Educloud MySQL:
```bash
mysql -u your_user -p your_database < docker/mysql-init/01-init.sql
```

Or import via phpMyAdmin on Educloud.

---

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment mode | Production |
| `ConnectionStrings__DefaultConnection` | MySQL connection string | See appsettings.json |

---

## Database Credentials

### Docker (Local)
- **Host:** localhost (or `mysql` from within containers)
- **Port:** 3306
- **Database:** EindhovenEnergy
- **User:** energie_user
- **Password:** Energie123!

### Educloud
Use credentials provided by your Educloud administrator.

---

## Useful Commands

### Docker
```bash
# View running containers
docker ps

# View logs
docker-compose logs -f web

# Rebuild after code changes
docker-compose up -d --build

# Remove everything
docker-compose down -v
```

### IIS Troubleshooting
```bash
# Check if .NET 9 is installed
dotnet --list-runtimes

# Test database connection
dotnet run --project urban_city_power_managment.Web
```

---

## Quick Reference

| Deployment | URL | Database |
|------------|-----|----------|
| Docker Local | http://localhost:8080 | Included (MySQL container) |
| Educloud IIS | http://your-educloud-url | Educloud MySQL |

---

## Files Structure for IIS

```
publish/
|-- urban_city_power_managment.Web.dll
|-- urban_city_power_managment.Web.exe
|-- web.config
|-- appsettings.json
|-- wwwroot/
|   |-- css/
|   |-- lib/
|   +-- favicon.png
+-- (other DLL files)
```

---

## Team Checklist

- [ ] Clone repository
- [ ] Choose deployment method (Docker or IIS)
- [ ] Configure database connection
- [ ] Run/Deploy application
- [ ] Test all pages work
- [ ] Verify database connection

Questions? Check the logs or contact the team!
