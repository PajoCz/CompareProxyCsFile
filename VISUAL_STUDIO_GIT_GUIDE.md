# Publikace pomocí Visual Studio Git UI

Pokud máte problémy se skriptem, mùžete použít Visual Studio's Git rozhraní:

## ?? Krok za krokem:

### 1. Otevøete Solution ve Visual Studiu
```
File > Open > Project/Solution
Vyberte: C:\Users\pbalas\source\repos\CompareProxyCsFile\CompareProxyCsFile.slnx
```

### 2. Otevøete Git Changes panel
```
View > Git Changes
(nebo Ctrl+0, Ctrl+G)
```

### 3. Stage všechny zmìny
V Git Changes panelu:
- Zkontrolujte seznam zmìn
- Kliknìte na **"+"** u "Changes" pro stage všech souborù
- Nebo kliknìte pravým tlaèítkem > "Stage All"

### 4. Commit zmìny
V Git Changes panelu:
- Napište commit message do textového pole nahoøe:
```
Initial commit: CompareProxyCsFile v1.0.0

Complete project with:
- Semantic proxy comparison using Roslyn
- Timestamped TXT reports
- Comprehensive documentation
- GitHub workflows and templates
- MIT License
```
- Kliknìte **"Commit All"** tlaèítko

### 5. Push na GitHub
V Git Changes panelu:
- Kliknìte na šipku nahoru (?) **"Push"**
- Nebo: kliknìte na dropdown u "Commit All" > **"Commit All and Push"**

### 6. Ovìøení
- Otevøete https://github.com/PajoCz/CompareProxyCsFile
- Ovìøte, že všechny soubory jsou nahrány

## ?? Alternativa: Git Repository panel

Pokud Git Changes nefunguje:

### Team Explorer (starší VS verze):
```
View > Team Explorer
```
1. Kliknìte na "Changes"
2. Zadejte commit message
3. Kliknìte "Commit All"
4. Kliknìte "Sync"
5. Kliknìte "Push"

### Git Repository Window (novìjší VS verze):
```
View > Git Repository
```
1. Vyberte "Outgoing Commits"
2. Push zmìny

## ? Rychlý pøístup

**Tlaèítka ve Visual Studio:**
- Na dolní lištì najdìte Git informace
- Kliknìte na branch name ("master" nebo "main")
- Kliknìte na "?" ikonu pro push

## ?? Troubleshooting

### Problém: "Authentication failed"
**Øešení:**
1. Tools > Options > Source Control > Git Global Settings
2. Zkontrolujte pøihlašovací údaje
3. Nebo použijte GitHub Desktop / Git Credential Manager

### Problém: Branch se jmenuje "master" místo "main"
**Øešení:**
1. V Git Changes: kliknìte na "master" (branch name)
2. Vyberte "Manage Branches"
3. Pravý klik na "master" > Rename > zadejte "main"
4. Push

### Problém: Konflikty nebo chyby
**Øešení:**
1. V Team Explorer > Sync > Pull
2. Vyøešte konflikty pokud jsou
3. Commit a push znovu

## ?? Po úspìšném push

Otevøete `POST_PUBLICATION_CHECKLIST.md` a postupujte podle dalších krokù:
- Pøidat topics na GitHubu
- Vytvoøit první release
- Povolit GitHub Actions

---

**Tip:** Pokud stále máte problémy, použijte GitHub Desktop (https://desktop.github.com/)
