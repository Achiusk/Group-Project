# Testing Instructions - Language & Theme Issues

## ? **WHAT'S FIXED:**

1. ? **Sidebar Navigation** - Now uses localized bindings
2. ? **Language Switching** - All ViewModels recreate when language changes
3. ? **Comprehensive Debug Output** - To diagnose theme issues

---

## ?? **TESTING PROCEDURE:**

### **Step 1: Run the Application**
```powershell
dotnet run --project "C:\Users\LAURE\source\repos\urban city power managment\urban city power managment.csproj"
```

### **Step 2: Check Visual Studio Debug Output**

1. Open **View ? Output** (`Ctrl+Alt+O`)
2. Select **"Debug"** from the dropdown
3. You should see on startup:
```
MainWindowViewModel: Setting initial theme to Light...
===== ThemeService.SetTheme START =====
Requested theme: Light
Application.Current is null: False
BEFORE: Application.Current.RequestedThemeVariant = Light
AFTER: Application.Current.RequestedThemeVariant = Light
Theme applied successfully
Firing ThemeChanged event...
ThemeChanged event fired. Subscriber count: 1
===== ThemeService.SetTheme END =====
```

### **Step 3: Test Language Switching**

1. Navigate to **Settings** (Instellingen)
2. Click **"English" (EN)** button
3. **Check Debug Output:**
```
English button clicked! ViewModel is not null
Calling SetLanguage('en')...
LocalizationService.SetLanguage called with: en
Language changed to: en
Firing LanguageChanged event...
LanguageChanged event fired. Subscriber count: 2
SetLanguage('en') completed
=== MainWindowViewModel.OnLanguageChanged fired ===
Current view type: SettingsViewModel
Recreating SettingsViewModel...
=== Language change handling complete ===
```

4. **Expected Results:**
   - ? Sidebar navigation changes to English
   - ? Settings view changes to English
   - ? App title changes to "Urban Energy Management Eindhoven"

5. Navigate to **Dashboard** and check if title is in English

### **Step 4: Test Dark Theme**

1. In Settings, click **"Donkere Modus"** (Dark Mode) button
2. **Check Debug Output:**
```
Dark theme button clicked! ViewModel is not null
Calling SetDarkTheme()...
===== ThemeService.SetTheme START =====
Requested theme: Dark
Application.Current is null: False
BEFORE: Application.Current.RequestedThemeVariant = Light
AFTER: Application.Current.RequestedThemeVariant = Dark
Theme applied successfully
Firing ThemeChanged event...
ThemeChanged event fired. Subscriber count: 1
===== ThemeService.SetTheme END =====
SetDarkTheme() completed
Theme changed to: Dark
```

3. **Expected Results:**
   - ? **ENTIRE APP** should turn dark (not just title bar)
   - ? Background should be dark gray/black
   - ? Text should be white/light
   - ? Cards should have dark backgrounds

---

## ?? **IF THEME DOESN'T WORK:**

### **Possible Issues:**

**Issue 1: Application.Current is null**
- Debug output shows: `Application.Current is null: True`
- **Solution:** Theme is being set too early
- Need to delay theme setting until after window is shown

**Issue 2: Theme sets but doesn't apply visually**
- Debug output shows theme set successfully
- But app still looks light
- **Possible causes:**
  1. FluentTheme not properly configured
  2. Custom styles (ButtonStyles.axaml) overriding theme
  3. Explicit colors in XAML overriding theme

**Issue 3: Only title bar changes**
- This means Windows theme is changing, but Avalonia theme isn't
- **Solution:** Need to ensure `Application.Current.RequestedThemeVariant` is actually being applied

---

## ?? **PLEASE PROVIDE THIS INFO:**

After testing, copy and paste from Debug Output window:

### **1. On App Startup:**
(The theme initialization messages)

### **2. When Clicking "English":**
(The language change messages)

### **3. When Clicking "Dark Mode":**
(The theme change messages - MOST IMPORTANT!)

### **4. Visual Results:**
- Does sidebar change language? (Yes/No)
- Does entire app turn dark? (Yes/No/Only title bar)
- Any views still showing "?{Binding...}"? (Which ones?)

---

## ?? **REMAINING 10% LANGUAGE ISSUES:**

If some views still show hardcoded text or "?{Binding...}", it's because:

1. **Hardcoded text in XAML** - Views like DashboardView have "Totale Opwekking" hardcoded
2. **Missing ViewModel properties** - Some ViewModels don't have localized properties
3. **DataContext not set** - Some nested controls might not inherit DataContext

**To fix remaining 10%:** Need to identify which specific labels aren't changing and add bindings for them.

---

## ? **EXPECTED FINAL STATE:**

- **Language:** 100% of text changes when switching language
- **Theme:** Entire app (sidebar, header, content) changes from light to dark
- **No binding errors:** No "?{Binding...}" anywhere

