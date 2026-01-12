# Azure Deployment Guide - Mijn Energie

## Prerequisites

- Azure account (free tier available)
- GitHub repository
- Azure CLI (optional, for command line deployment)

---

## Option 1: Deploy via Azure Portal (Easiest)

### Step 1: Create Azure Resources

1. Go to [Azure Portal](https://portal.azure.com)
2. Click **Create a resource**
3. Search for **Web App**
4. Configure:
   - **Name**: `mijn-energie` (or your preferred name)
   - **Runtime stack**: `.NET 9`
   - **Region**: `West Europe`
   - **Pricing**: `Free F1` (for testing)
5. Click **Review + Create** ? **Create**

### Step 2: Create MySQL Database

1. In Azure Portal, click **Create a resource**
2. Search for **Azure Database for MySQL**
3. Choose **Flexible Server**
4. Configure:
   - **Server name**: `mijn-energie-db`
   - **Region**: Same as Web App
   - **MySQL version**: `8.0`
   - **Compute**: `Burstable B1ms` (cheapest)
   - **Admin username**: `energie_admin`
   - **Password**: (create a strong password)
5. Under **Networking**, allow Azure services
6. Click **Review + Create** ? **Create**

### Step 3: Configure Connection String

1. Go to your **Web App** in Azure Portal
2. Click **Configuration** ? **Connection strings**
3. Add new connection string:
   - **Name**: `DefaultConnection`
   - **Value**: 
  ```
     Server=mijn-energie-db.mysql.database.azure.com;Port=3306;Database=EindhovenEnergy;User=energie_admin;Password=YOUR_PASSWORD;SslMode=Required;
     ```
   - **Type**: `MySQL`
4. Click **Save**

### Step 4: Deploy from GitHub

1. In your Web App, go to **Deployment Center**
2. Choose **GitHub** as source
3. Authorize and select:
   - **Organization**: Your GitHub username
   - **Repository**: `Group-Project`
   - **Branch**: `main`
4. Click **Save**

Azure will automatically deploy on every push to main!

---

## Option 2: Deploy via GitHub Actions

### Step 1: Get Publish Profile

1. In Azure Portal, go to your Web App
2. Click **Download publish profile**
3. Save the file

### Step 2: Add Secret to GitHub

1. Go to your GitHub repository
2. Click **Settings** ? **Secrets and variables** ? **Actions**
3. Click **New repository secret**
4. Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
5. Value: Paste the entire contents of the publish profile file
6. Click **Add secret**

### Step 3: Update Workflow (if needed)

Edit `.github/workflows/azure-deploy.yml`:
```yaml
env:
  AZURE_WEBAPP_NAME: your-actual-app-name  # Change this!
```

### Step 4: Trigger Deployment

Push to main branch or manually trigger:
```bash
git push origin main
```

---

## Option 3: Deploy via Azure CLI

### Install Azure CLI
```bash
# Windows (PowerShell)
winget install Microsoft.AzureCLI

# Or download from: https://aka.ms/installazurecliwindows
```

### Login and Deploy
```bash
# Login to Azure
az login

# Create resource group
az group create --name mijn-energie-rg --location westeurope

# Create App Service plan
az appservice plan create --name mijn-energie-plan --resource-group mijn-energie-rg --sku F1 --is-linux

# Create Web App
az webapp create --name mijn-energie --resource-group mijn-energie-rg --plan mijn-energie-plan --runtime "DOTNET|9.0"

# Deploy from local folder
cd Group-Project
dotnet publish -c Release -o ./publish
az webapp deploy --resource-group mijn-energie-rg --name mijn-energie --src-path ./publish --type zip
```

---

## Environment Variables for Azure

Set these in **Configuration** ? **Application settings**:

| Name | Value | Description |
|------|-------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Environment mode |
| `ConnectionStrings__DefaultConnection` | (see above) | MySQL connection |

---

## Database Setup

### Option A: Run SQL Script
Connect to Azure MySQL and run:
```sql
-- Contents of docker/mysql-init/01-init.sql
```

### Option B: Use Migrations
The app will automatically create tables on first run.

---

## Monitoring

### Application Insights (Optional)

1. In Azure Portal, create **Application Insights** resource
2. Copy the **Instrumentation Key**
3. Add to Web App configuration:
   - Name: `Azure__ApplicationInsights__InstrumentationKey`
   - Value: Your instrumentation key

### View Logs
```bash
az webapp log tail --name mijn-energie --resource-group mijn-energie-rg
```

---

## Costs (Estimated)

| Resource | Free Tier | Production |
|----------|-----------|------------|
| App Service | F1 Free | B1 ~€12/month |
| MySQL | Burstable B1ms ~€12/month | B2s ~€25/month |
| **Total** | ~€12/month | ~€37/month |

---

## Troubleshooting

### App won't start
```bash
# Check logs
az webapp log tail --name mijn-energie --resource-group mijn-energie-rg

# Check health
curl https://mijn-energie.azurewebsites.net/health
```

### Database connection failed
1. Check connection string in Configuration
2. Verify MySQL firewall allows Azure services
3. Check SSL mode is set to `Required`

### Deployment failed
1. Check GitHub Actions logs
2. Verify publish profile secret is correct
3. Check .NET version matches (9.0)

---

## Quick Commands

```bash
# View app in browser
az webapp browse --name mijn-energie --resource-group mijn-energie-rg

# Restart app
az webapp restart --name mijn-energie --resource-group mijn-energie-rg

# View deployment status
az webapp deployment list-publishing-profiles --name mijn-energie --resource-group mijn-energie-rg

# Scale up (if needed)
az appservice plan update --name mijn-energie-plan --resource-group mijn-energie-rg --sku B1
```

---

## URLs

After deployment, your app will be available at:
- **Web App**: `https://mijn-energie.azurewebsites.net`
- **Health Check**: `https://mijn-energie.azurewebsites.net/health`

Replace `mijn-energie` with your actual app name.
