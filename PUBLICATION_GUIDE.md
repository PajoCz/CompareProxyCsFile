# GitHub Publication Guide

Prùvodce publikací CompareProxyCsFile na GitHub.

## ? Pøedpublikaèní kontrola

Vše je pøipraveno! Soubory byly vytvoøeny s vašimi údaji (PajoCz).

## ?? Co bylo vytvoøeno:

### Základní dokumentace
- ? `README.md` - Hlavní dokumentace projektu
- ? `LICENSE` - MIT License (Copyright PajoCz)
- ? `CHANGELOG.md` - Historie verzí
- ? `CONTRIBUTING.md` - Prùvodce pro pøispìvatele
- ? `.gitignore` - Git ignore pravidla

### GitHub integrace
- ? `.github/workflows/build.yml` - CI/CD pipeline
- ? `.github/ISSUE_TEMPLATE/bug_report.md` - Šablona pro bug reporty
- ? `.github/ISSUE_TEMPLATE/feature_request.md` - Šablona pro feature requesty

### Konfigurace projektu
- ? `CompareProxyCsFile.csproj` - Aktualizován s metadaty (PajoCz)

## ?? Kroky k publikaci:

### 1. Ovìøení buildu

```bash
cd C:\Users\pbalas\source\repos\CompareProxyCsFile\CompareProxyCsFile
dotnet clean
dotnet build -c Release
```

### 2. Inicializace Git repository

```bash
cd C:\Users\pbalas\source\repos\CompareProxyCsFile
git init
git add .
git commit -m "Initial commit: CompareProxyCsFile v1.0.0"
```

### 3. Vytvoøení GitHub repository

1. Jdìte na https://github.com/new
2. Repository name: `CompareProxyCsFile`
3. Description: `Semantic comparison tool for auto-generated C# proxy files`
4. Public
5. **NEPOUŽÍVEJTE "Initialize with README"** (máme ho už vytvoøený)
6. Click "Create repository"

### 4. Push na GitHub

```bash
git remote add origin https://github.com/PajoCz/CompareProxyCsFile.git
git branch -M main
git push -u origin main
```

### 5. Nastavení repository

Na GitHubu:

**Settings > General:**
- Topics: `csharp`, `proxy`, `comparison`, `wcf`, `roslyn`, `code-analysis`, `semantic`, `diff`
- Description: `Semantic comparison tool for auto-generated C# proxy files`

**Settings > Features:**
- ? Enable Issues
- ? Enable Discussions (volitelné)

**Settings > Actions > General:**
- ? Allow all actions and reusable workflows

### 6. Vytvoøení první release

1. Go to **Releases** > **Create a new release**
2. Tag version: `v1.0.0`
3. Release title: `v1.0.0 - Initial Release`
4. Description:
```markdown
## Initial Release

### Features
- Semantic comparison of C# proxy files using Roslyn
- Ignores generator version differences
- Order-independent type comparison
- Timestamped TXT report generation
- Detailed difference reporting
- Error handling with error reports
- Exit codes for automation
```

5. Pøipojte zkompilované soubory:
```bash
cd CompareProxyCsFile\bin\Release\net8.0
# Zabalit všechny soubory do ZIP
```

6. Publish release

### 7. Pøidání status badges (volitelné)

Do `README.md` na zaèátek pøidejte:

```markdown
![.NET](https://github.com/PajoCz/CompareProxyCsFile/workflows/.NET%20Build%20and%20Test/badge.svg)
![License](https://img.shields.io/github/license/PajoCz/CompareProxyCsFile)
![Release](https://img.shields.io/github/v/release/PajoCz/CompareProxyCsFile)
```

## ?? Po publikaci

### Testování GitHub Actions
- Push malé zmìny pro spuštìní build workflow
- Ovìøte, že build projde na všech tøech OS

### Sdílení projektu
- LinkedIn
- Reddit (r/csharp, r/dotnet)
- Dev.to èlánek

## ?? Volitelnì: Publikace na NuGet

```bash
cd CompareProxyCsFile
dotnet pack -c Release
dotnet nuget push bin/Release/CompareProxyCsFile.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

Pak uživatelé mohou instalovat:
```bash
dotnet tool install --global CompareProxyCsFile
```

## ? Hotovo!

Váš projekt je pøipraven k publikaci na GitHub! ??

**Repository URL:** https://github.com/PajoCz/CompareProxyCsFile
