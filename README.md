# CompareProxyCsFile

A C# console application for semantic comparison of auto-generated proxy files (e.g., WCF service references). This tool intelligently compares C# source files while ignoring irrelevant differences like generator version numbers and type ordering.

## ?? Features

- **Semantic Comparison**: Compares types, members, and attributes based on their meaning, not text position
- **Generator Version Agnostic**: Ignores differences in `GeneratedCodeAttribute` version numbers
- **Order Independent**: Detects changes regardless of class/interface ordering in files
- **Detailed Reports**: Generates timestamped TXT reports with comprehensive comparison results
- **Exit Codes**: Returns appropriate exit codes for automation (0 = identical, 1 = differences/error)

## ?? Installation

### Prerequisites

- .NET 8.0 SDK or later
- Windows, Linux, or macOS

### Build from Source

```bash
git clone https://github.com/PajoCz/CompareProxyCsFile.git
cd CompareProxyCsFile/CompareProxyCsFile
dotnet build -c Release
```

The compiled executable will be in `bin/Release/net8.0/`

## ?? Usage

### Basic Usage

```bash
CompareProxyCsFile <file1.cs> <file2.cs>
```

### Example

```bash
CompareProxyCsFile "D:\Project\OldProxy\Reference.cs" "D:\Project\NewProxy\Reference.cs"
```

### Output

The tool generates a timestamped report file: `ProxyComparison_yyyyMMdd_HHmmss.txt`

**Example report structure:**
```
================================================================================
PROXY FILES COMPARISON REPORT
================================================================================

Date and Time: 2025-12-08 14:30:25

File 1: D:\Project\OldProxy\Reference.cs
File 2: D:\Project\NewProxy\Reference.cs

================================================================================

RESULT: Files are semantically identical.

No differences found in:
  - Type definitions (classes, interfaces, enums)
  - Type members (methods, properties, fields)
  - Attributes (excluding generator version numbers)
  - Base types and interfaces

================================================================================
END OF REPORT
================================================================================
```

## ?? What It Compares

### Detects Changes In:
- ? Type additions/removals (classes, interfaces, enums)
- ? Member additions/removals (methods, properties, fields)
- ? Method signatures (return types, parameters)
- ? Attributes (except generator versions)
- ? Base types and implemented interfaces
- ? Enum member changes

### Ignores:
- ? Different generator version numbers in `GeneratedCodeAttribute`
- ? Different ordering of types in files
- ? Auto-generated comments
- ? Whitespace and formatting

## ?? Use Cases

- **CI/CD Pipelines**: Verify that service reference updates don't introduce breaking changes
- **Code Reviews**: Quickly identify meaningful changes in auto-generated proxy files
- **Version Control**: Detect actual API changes vs. regeneration noise
- **Migration Projects**: Compare old vs. new service implementations

## ??? Technical Details

### Technology Stack
- .NET 8.0
- Microsoft.CodeAnalysis.CSharp (Roslyn) 5.0.0
- C# 12.0

### How It Works
1. Parses both C# files using Roslyn syntax trees
2. Extracts semantic information (types, members, attributes)
3. Normalizes generator attributes by removing version parameters
4. Compares types by fully-qualified name (order-independent)
5. Generates detailed difference report

## ?? Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Files are semantically identical |
| 1 | Differences found or error occurred |

Perfect for automation and scripting!

## ?? Error Handling

If an error occurs, the tool generates an error report: `ProxyComparison_ERROR_yyyyMMdd_HHmmss.txt`

Example errors:
- File not found
- Invalid C# syntax
- Permission issues

## ?? Example Scenarios

### Scenario 1: Identical Files with Different Generator Versions
```csharp
// File1.cs
[GeneratedCodeAttribute("System.Xml", "4.8.9221.0")]
public class MyClass { }

// File2.cs
[GeneratedCodeAttribute("System.Xml", "4.8.9032.0")]
public class MyClass { }
```
**Result**: Files are semantically identical ?

### Scenario 2: Different Type Order
```csharp
// File1.cs
public class ClassA { }
public class ClassB { }

// File2.cs
public class ClassB { }
public class ClassA { }
```
**Result**: Files are semantically identical ?

### Scenario 3: New Method Added
```csharp
// File1.cs
public interface IService {
    void MethodA();
}

// File2.cs
public interface IService {
    void MethodA();
    void MethodB(); // New method
}
```
**Result**: Differences found
```
+ Member added in IService: Method void MethodB()
```

## ?? Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ?? Acknowledgments

- Built with [Roslyn](https://github.com/dotnet/roslyn) - the .NET Compiler Platform
- Inspired by the need to efficiently compare WCF service proxy regenerations

## ?? Contact

PajoCz - [GitHub](https://github.com/PajoCz)

Project Link: [https://github.com/PajoCz/CompareProxyCsFile](https://github.com/PajoCz/CompareProxyCsFile)

---

Made with ?? for developers tired of reviewing auto-generated code changes
