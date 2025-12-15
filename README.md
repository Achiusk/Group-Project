# Stedelijk Energiebeheer Eindhoven

Een admin panel applicatie voor energie management gebouwd met Avalonia UI (.NET 9) in de stijl van de Rijksoverheid.

## Functies

### ? Energie Management
- **Energieopwekking Overzicht**: Realtime monitoring van wind (20%), water (20%), en kolen (60%) energie productie
- **Consumenten Dashboard**: Energie verbruik en zonnepanelen teruglevering voor huishoudens
- **Bedrijven Dashboard**: Energie verbruik tracking voor bedrijven en organisaties
- **Meldingen Systeem**: Realtime notificaties voor incidenten en ongewoon verbruik
- **Taken Beheer**: Task management voor energie infrastructuur onderhoud

### ?? Talen
- **Nederlands** (Primair)
- **Engels** (Optioneel)
- Eenvoudig schakelen tussen talen via instellingen

### ?? Thema's
- **Lichte Modus** (Rijksoverheid stijl: blauw/wit/teal)
- **Donkere Modus**

### ??? Weather Integration
- AccuWeather API integratie
- Realtime weer informatie voor Eindhoven
- Temperatuur en weersomstandigheden op hoofdscherm

### ?? Beveiliging

#### Two-Factor Authentication (2FA)
De applicatie ondersteunt **Microsoft Authenticator** voor twee-factor authenticatie:

1. **Azure AD/Entra ID Integration**
   - Gebruik Microsoft Identity Platform
  - Automatische MFA prompt als geconfigureerd in Azure AD
   - Ondersteunt alle Microsoft Authenticator methoden

2. **Setup Instructies voor 2FA**:
   ```bash
   # 1. Ga naar Azure Portal (portal.azure.com)
 # 2. Navigeer naar "Azure Active Directory" (nu "Microsoft Entra ID")
   # 3. Ga naar "App registrations" ? "New registration"
   # 4. Vul in:
   # - Name: "Stedelijk Energiebeheer Eindhoven"
   #    - Supported account types: "Accounts in this organizational directory only"
   #    - Redirect URI: Platform: Public client/native, URI: http://localhost
   # 5. Kopieer "Application (client) ID" en "Directory (tenant) ID"
   # 6. Update appsettings.json met deze waarden
   ```

3. **MFA Configuratie in Azure AD**:
   ```bash
   # 1. In Azure Portal ? Azure AD ? Security ? MFA
   # 2. Kies "Additional cloud-based MFA settings"
   # 3. Selecteer "Microsoft Authenticator app" als verificatie methode
   # 4. Optioneel: Maak Conditional Access policies voor verplichte MFA
   ```

## ?? Azure Hosting

### Voorbereiding voor Azure Deployment

#### 1. Azure App Service
De applicatie is ontworpen voor deployment naar Azure App Service:

```bash
# Azure CLI commando's voor deployment
az login

# Maak resource group
az group create --name rg-eindhoven-energy --location westeurope

# Maak App Service plan
az appservice plan create \
  --name plan-eindhoven-energy \
  --resource-group rg-eindhoven-energy \
  --sku B1 \
  --is-linux

# Maak Web App
az webapp create \
  --name app-eindhoven-energy \
  --resource-group rg-eindhoven-energy \
  --plan plan-eindhoven-energy \
 --runtime "DOTNET|9.0"
```

#### 2. Azure Key Vault voor Geheimen
Gebruik Azure Key Vault om API keys en geheimen veilig op te slaan:

```bash
# Maak Key Vault
az keyvault create \
  --name kv-eindhoven-energy \
  --resource-group rg-eindhoven-energy \
  --location westeurope

# Voeg geheimen toe
az keyvault secret set \
  --vault-name kv-eindhoven-energy \
--name AccuWeatherApiKey \
  --value "YOUR_API_KEY"

az keyvault secret set \
  --vault-name kv-eindhoven-energy \
--name AzureAdClientId \
  --value "YOUR_CLIENT_ID"

# Geef App Service toegang tot Key Vault
az webapp identity assign \
  --name app-eindhoven-energy \
  --resource-group rg-eindhoven-energy

# Zet access policy
az keyvault set-policy \
  --name kv-eindhoven-energy \
  --object-id <webapp-identity-principal-id> \
  --secret-permissions get list
```

#### 3. Application Insights voor Monitoring

```bash
# Maak Application Insights
az monitor app-insights component create \
  --app ai-eindhoven-energy \
  --location westeurope \
  --resource-group rg-eindhoven-energy

# Verkrijg connection string
az monitor app-insights component show \
  --app ai-eindhoven-energy \
  --resource-group rg-eindhoven-energy \
 --query connectionString
```

#### 4. Azure SQL Database (Optioneel voor toekomstige data opslag)

```bash
# Maak SQL Server
az sql server create \
  --name sql-eindhoven-energy \
  --resource-group rg-eindhoven-energy \
  --location westeurope \
  --admin-user sqladmin \
  --admin-password <SecurePassword>

# Maak Database
az sql db create \
  --resource-group rg-eindhoven-energy \
  --server sql-eindhoven-energy \
  --name db-energy-management \
  --service-objective S0
```

### Publish naar Azure

```bash
# Build en publish
dotnet publish -c Release -r win-x64 --self-contained false

# Deploy naar Azure (optie 1: via Azure CLI)
az webapp deployment source config-zip \
  --resource-group rg-eindhoven-energy \
  --name app-eindhoven-energy \
  --src publish.zip

# Deploy naar Azure (optie 2: via Visual Studio)
# Rechtsklik op project ? Publish ? Azure ? Azure App Service
```

## ?? Installatie & Development

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 of JetBrains Rider
- Azure subscription (voor deployment)

### Local Development

```bash
# Clone repository
git clone <repository-url>
cd "urban city power managment"

# Restore packages
dotnet restore

# Run applicatie
dotnet run
```

### Configuratie

1. **AccuWeather API Key**:
   - Registreer op https://developer.accuweather.com/
   - Verkrijg gratis API key
   - Update `appsettings.Development.json`

2. **Azure AD/2FA** (optioneel voor development):
   - Volg de 2FA Setup Instructies hierboven
   - Update `appsettings.json` met Azure AD credentials

## ??? Architectuur

### Technology Stack
- **Framework**: .NET 9
- **UI**: Avalonia UI 11.3.8
- **MVVM**: ReactiveUI
- **Authentication**: Microsoft.Identity.Client
- **HTTP**: Microsoft.Extensions.Http
- **Cloud**: Azure (App Service, Key Vault, Application Insights)

### Project Structure
```
??? Services/
?   ??? LocalizationService.cs      # Meertalige ondersteuning
?   ??? ThemeService.cs     # Light/Dark mode
?   ??? AccuWeatherService.cs       # Weather API
?   ??? MicrosoftAuthenticationService.cs  # 2FA/Azure AD
??? ViewModels/
?   ??? MainWindowViewModel.cs
?   ??? DashboardViewModel.cs
?   ??? PowerGenerationViewModel.cs
?   ??? ConsumersViewModel.cs
?   ??? BusinessesViewModel.cs
?   ??? NotificationsViewModel.cs
?   ??? TasksViewModel.cs
?   ??? SettingsViewModel.cs
??? Views/
? ??? [Corresponding AXAML views]
??? Styles/
?   ??? RijksoverheidStyles.axaml  # Custom Rijksoverheid styling
??? appsettings.json   # Configuration
```

## ?? Security Best Practices

1. **Never commit secrets**: Use `.gitignore` voor `appsettings.json`
2. **Use Azure Key Vault**: Alle API keys en secrets in Key Vault
3. **Enable MFA**: Verplicht voor productie omgeving
4. **Use Managed Identities**: Voor Azure service authenticatie
5. **HTTPS Only**: Force HTTPS in productie
6. **Regular Updates**: Houd dependencies up-to-date

## ?? Monitoring & Logging

### Application Insights
De applicatie is voorbereid voor Azure Application Insights:
- Performance monitoring
- Exception tracking
- Custom telemetry
- User analytics

### Logging implementeren
```csharp
// Add to services
services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:ConnectionString"]);
```

## ?? Localization

Talen worden beheerd in `LocalizationService.cs`. Om een nieuwe taal toe te voegen:

```csharp
["fr"] = new Dictionary<string, string>
{
    ["AppTitle"] = "Gestion de l'énergie urbaine Eindhoven",
    // ... meer vertalingen
}
```

## ?? License

© 2024 Gemeente Eindhoven - Voor educatieve doeleinden

## ?? Contributing

Voor bijdragen aan dit project:
1. Fork de repository
2. Maak een feature branch
3. Commit je wijzigingen
4. Push naar de branch
5. Open een Pull Request

## ?? Support

Voor vragen of problemen, contacteer het development team.

---

**Versie**: 1.0.0  
**Laatst bijgewerkt**: 2024  
**Ontwikkeld voor**: FHICT Smart Urban Energy Challenge
