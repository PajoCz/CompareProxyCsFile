# Post-Publication Checklist

Po úspìšném nahrání kódu na GitHub proveïte tyto kroky:

## ? Na GitHubu (https://github.com/PajoCz/CompareProxyCsFile)

### 1. Nastavení Repository
**Settings > General:**
- [ ] Pøidat topics: 
  - `csharp`
  - `proxy`
  - `comparison`
  - `wcf`
  - `roslyn`
  - `code-analysis`
  - `semantic`
  - `diff`
- [ ] Ovìøit description: "Semantic comparison tool for auto-generated C# proxy files"

**Settings > Features:**
- [ ] Povolit Issues
- [ ] Povolit Discussions (volitelné)

**Settings > Actions:**
- [ ] Povolit GitHub Actions
- [ ] Ovìøit, že workflow bìží

### 2. Vytvoøení První Release

**Releases > Create a new release:**

1. [ ] Tag version: `v1.0.0`
2. [ ] Release title: `v1.0.0 - Initial Release`
3. [ ] Description:
```markdown
## ?? Initial Release

### Features
? Semantic comparison of C# proxy files using Roslyn
? Ignores generator version differences in `GeneratedCodeAttribute`
? Order-independent type comparison
? Timestamped TXT report generation
? Detailed difference reporting for types, members, and attributes
? Error handling with comprehensive error reports
? Exit codes for automation (0 = identical, 1 = differences)
? Cross-platform support (.NET 8.0)

### Installation
```bash
git clone https://github.com/PajoCz/CompareProxyCsFile.git
cd CompareProxyCsFile/CompareProxyCsFile
dotnet build -c Release
```

### Usage
```bash
CompareProxyCsFile <file1.cs> <file2.cs>
```

See [README.md](https://github.com/PajoCz/CompareProxyCsFile#readme) for detailed documentation.
```

4. [ ] Pøipojit zkompilované soubory:
```powershell
# V PowerShell:
cd C:\Users\pbalas\source\repos\CompareProxyCsFile\CompareProxyCsFile\bin\Release\net8.0
Compress-Archive -Path * -DestinationPath ..\CompareProxyCsFile-v1.0.0-win-x64.zip
```

5. [ ] Publish release

### 3. Ovìøení GitHub Actions

- [ ] Zkontrolovat **Actions** tab
- [ ] Ovìøit, že build workflow probìhl úspìšnì
- [ ] Pokud ne, opravit chyby a push fix

### 4. README Vylepšení (volitelné)

Pøidat status badges na zaèátek README.md:

```markdown
![.NET](https://github.com/PajoCz/CompareProxyCsFile/workflows/.NET%20Build%20and%20Test/badge.svg)
![License](https://img.shields.io/github/license/PajoCz/CompareProxyCsFile)
![Release](https://img.shields.io/github/v/release/PajoCz/CompareProxyCsFile)
![GitHub last commit](https://img.shields.io/github/last-commit/PajoCz/CompareProxyCsFile)
```

## ?? Propagace (volitelné)

### Social Media
- [ ] LinkedIn post o novém projektu
- [ ] Tweet (pokud máte Twitter)

### Dev Communities
- [ ] Reddit: r/csharp
- [ ] Reddit: r/dotnet
- [ ] Dev.to èlánek
- [ ] Hackernews (Show HN)

### Awesome Lists
- [ ] Pull request do [awesome-dotnet](https://github.com/quozd/awesome-dotnet)
- [ ] Pull request do [awesome-csharp](https://github.com/uhub/awesome-csharp)

## ?? NuGet Package (pokroèilé, volitelné)

Pokud chcete publikovat jako .NET global tool:

```bash
# Vytvoøit NuGet package
cd CompareProxyCsFile
dotnet pack -c Release

# Publikovat na NuGet.org (vyžaduje API key z https://www.nuget.org/)
dotnet nuget push bin/Release/CompareProxyCsFile.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

Pak uživatelé mohou instalovat:
```bash
dotnet tool install --global CompareProxyCsFile
```

## ?? Monitoring

Po nìkolika dnech zkontrolujte:
- [ ] Poèet stars
- [ ] Issues od uživatelù
- [ ] Pull requests
- [ ] GitHub Insights (traffic)

## ?? Další Kroky

### Možná vylepšení pro verzi 1.1.0:
- [ ] Unit testy
- [ ] Podpora více souborù najednou
- [ ] JSON output formát
- [ ] Verbose/quiet režim
- [ ] Konfigurovatelné ignorování atributù
- [ ] Podpora nested typù
- [ ] Performance optimalizace pro velké soubory

---

**? Gratulace! Váš projekt je nyní live na GitHubu!**

Repository: https://github.com/PajoCz/CompareProxyCsFile
