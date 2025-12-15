# Azure Deployment Guide for Stedelijk Energiebeheer Eindhoven

## Prerequisites
- Azure CLI installed: https://docs.microsoft.com/cli/azure/install-azure-cli
- .NET 9 SDK installed
- Azure subscription
- AccuWeather API key

## Step-by-Step Deployment

### 1. Login to Azure
```bash
az login
az account set --subscription "YOUR_SUBSCRIPTION_ID"
```

### 2. Set Variables
```bash
RESOURCE_GROUP="rg-eindhoven-energy"
LOCATION="westeurope"
APP_NAME="eindhoven-energy-mgmt"
PLAN_NAME="plan-eindhoven-energy"
KEYVAULT_NAME="kv-eindhoven-$(openssl rand -hex 4)"
APPINSIGHTS_NAME="ai-eindhoven-energy"
```

### 3. Create Resource Group
```bash
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

### 4. Create Key Vault
```bash
az keyvault create \
  --name $KEYVAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --enable-soft-delete true \
  --enable-purge-protection true
```

### 5. Add Secrets to Key Vault
```bash
# AccuWeather API Key
az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "AccuWeatherApiKey" \
  --value "YOUR_ACCUWEATHER_API_KEY"

# Azure AD Client ID
az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "AzureAdClientId" \
  --value "YOUR_AZURE_AD_CLIENT_ID"

# Azure AD Tenant ID
az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "AzureAdTenantId" \
  --value "YOUR_TENANT_ID"
```

### 6. Create Application Insights
```bash
az monitor app-insights component create \
  --app $APPINSIGHTS_NAME \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP \
  --application-type web

# Get instrumentation key
APPINSIGHTS_KEY=$(az monitor app-insights component show \
  --app $APPINSIGHTS_NAME \
  --resource-group $RESOURCE_GROUP \
  --query instrumentationKey -o tsv)

echo "Application Insights Key: $APPINSIGHTS_KEY"
```

### 7. Create App Service Plan
```bash
az appservice plan create \
  --name $PLAN_NAME \
  --resource-group $RESOURCE_GROUP \
  --sku B1 \
  --is-linux
```

### 8. Create Web App
```bash
az webapp create \
  --name $APP_NAME \
--resource-group $RESOURCE_GROUP \
  --plan $PLAN_NAME \
  --runtime "DOTNET|9.0"
```

### 9. Enable Managed Identity
```bash
az webapp identity assign \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP

# Get the identity principal ID
IDENTITY_ID=$(az webapp identity show \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query principalId -o tsv)

echo "Managed Identity ID: $IDENTITY_ID"
```

### 10. Grant Key Vault Access to Web App
```bash
az keyvault set-policy \
  --name $KEYVAULT_NAME \
  --object-id $IDENTITY_ID \
  --secret-permissions get list
```

### 11. Configure App Settings
```bash
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    "KeyVault__VaultUri=https://$KEYVAULT_NAME.vault.azure.net/" \
    "ApplicationInsights__InstrumentationKey=$APPINSIGHTS_KEY" \
    "AccuWeather__LocationKey=2754454" \
    "AccuWeather__BaseUrl=http://dataservice.accuweather.com"
```

### 12. Enable HTTPS Only
```bash
az webapp update \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --https-only true
```

### 13. Build and Publish Application
```bash
# From project directory
dotnet publish -c Release -o ./publish

# Create zip file
cd publish
zip -r ../publish.zip .
cd ..
```

### 14. Deploy to Azure
```bash
az webapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --src publish.zip
```

### 15. Configure Azure AD for 2FA

#### Create App Registration
```bash
# This requires manual steps in Azure Portal:
# 1. Go to portal.azure.com
# 2. Navigate to "Azure Active Directory" ? "App registrations"
# 3. Click "New registration"
# 4. Fill in:
#    - Name: "Stedelijk Energiebeheer Eindhoven"
#- Supported account types: "Accounts in this organizational directory only"
#    - Redirect URI: https://$APP_NAME.azurewebsites.net/signin-oidc
# 5. After creation, go to "Authentication"
# 6. Enable "ID tokens"
# 7. Go to "API permissions"
# 8. Add "Microsoft Graph" ? "User.Read"
# 9. Grant admin consent
# 10. Copy "Application (client) ID" and "Directory (tenant) ID"
```

#### Add Azure AD Secrets to Key Vault
```bash
# After getting values from portal
az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "AzureAdClientId" \
  --value "COPIED_CLIENT_ID"

az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "AzureAdTenantId" \
  --value "COPIED_TENANT_ID"
```

#### Configure MFA in Azure AD
```bash
# Manual steps in Azure Portal:
# 1. Go to "Azure Active Directory" ? "Security" ? "Multifactor authentication"
# 2. Click "Additional cloud-based MFA settings"
# 3. Select "Microsoft Authenticator app" as verification method
# 4. (Optional) Create Conditional Access policy:
#    - Azure AD ? Security ? Conditional Access ? New policy
#    - Name: "Require MFA for Energy Management App"
#    - Users: Select appropriate users/groups
#    - Cloud apps: Select your app registration
#    - Grant: Require multi-factor authentication
```

### 16. Verify Deployment
```bash
# Get the app URL
APP_URL=$(az webapp show \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query defaultHostName -o tsv)

echo "Your application is deployed at: https://$APP_URL"

# Open in browser
open "https://$APP_URL"
```

### 17. Configure Custom Domain (Optional)
```bash
# If you have a custom domain:
az webapp config hostname add \
  --webapp-name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --hostname "energy.eindhoven.nl"

# Bind SSL certificate
az webapp config ssl bind \
  --certificate-thumbprint THUMBPRINT \
  --ssl-type SNI \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP
```

### 18. Set up Continuous Deployment (Optional)
```bash
# If using GitHub:
az webapp deployment source config \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --repo-url https://github.com/YOUR_USERNAME/YOUR_REPO \
  --branch main \
  --manual-integration
```

## Monitoring and Maintenance

### View Application Logs
```bash
az webapp log tail \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP
```

### View Application Insights
```bash
# Go to Azure Portal
# Navigate to Application Insights ? $APPINSIGHTS_NAME
# View:
# - Live Metrics
# - Failures
# - Performance
# - Users
```

### Scale Application
```bash
# Scale up (more powerful instance)
az appservice plan update \
  --name $PLAN_NAME \
  --resource-group $RESOURCE_GROUP \
  --sku P1V2

# Scale out (more instances)
az appservice plan update \
--name $PLAN_NAME \
  --resource-group $RESOURCE_GROUP \
  --number-of-workers 3
```

### Backup and Restore
```bash
# Create backup
az webapp config backup create \
  --resource-group $RESOURCE_GROUP \
  --webapp-name $APP_NAME \
  --backup-name "backup-$(date +%Y%m%d)" \
  --container-url "STORAGE_CONTAINER_SAS_URL"
```

## Cost Optimization

### Estimated Monthly Costs (EUR)
- App Service (B1): €10-15
- Key Vault: €0.03 per 10,000 operations
- Application Insights: Free tier (1 GB/month)
- **Total**: ~€10-20/month for development

### Production Recommendations
- Use App Service P1V2 or higher: €60-150/month
- Enable auto-scaling
- Use Azure Front Door for global distribution
- Implement Azure CDN for static assets

## Security Checklist

- [ ] All secrets stored in Azure Key Vault
- [ ] Managed Identity enabled for app
- [ ] HTTPS-only enforced
- [ ] MFA configured in Azure AD
- [ ] Application Insights enabled
- [ ] Diagnostic logs enabled
- [ ] Backup configured
- [ ] Custom domain with SSL (production)
- [ ] Network security groups configured (if using VNet)
- [ ] Regular security updates scheduled

## Troubleshooting

### Application won't start
```bash
# Check logs
az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP

# Check App Service diagnostics
az webapp browse --name $APP_NAME --resource-group $RESOURCE_GROUP
```

### Can't access Key Vault
```bash
# Verify managed identity
az webapp identity show --name $APP_NAME --resource-group $RESOURCE_GROUP

# Verify Key Vault access policy
az keyvault show --name $KEYVAULT_NAME
```

### 2FA not working
- Verify App Registration redirect URI matches deployment URL
- Check Azure AD logs in portal
- Ensure Microsoft Authenticator is properly configured

## Clean Up Resources
```bash
# WARNING: This deletes everything!
az group delete \
  --name $RESOURCE_GROUP \
  --yes \
  --no-wait
```

## Support

For issues or questions:
- Azure Documentation: https://docs.microsoft.com/azure
- Azure Support: https://azure.microsoft.com/support
- AccuWeather API: https://developer.accuweather.com/support
