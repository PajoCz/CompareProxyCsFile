# Contributing to CompareProxyCsFile

First off, thank you for considering contributing to CompareProxyCsFile! ??

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues. When creating a bug report, include:

- **Clear title and description**
- **Steps to reproduce** the behavior
- **Expected vs actual behavior**
- **Sample files** that demonstrate the issue (if possible)
- **Environment details** (OS, .NET version)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, include:

- **Clear title and description**
- **Use case** explaining why this enhancement would be useful
- **Examples** of how the feature would work

### Pull Requests

1. Fork the repo and create your branch from `main`
2. If you've added code that should be tested, add tests
3. Ensure the code compiles without warnings
4. Follow the existing code style
5. Update the CHANGELOG.md
6. Issue the pull request

## Code Style Guidelines

### C# Coding Conventions

- Use **C# 12.0** features where appropriate
- Follow **standard C# naming conventions**:
  - PascalCase for classes, methods, properties
  - camelCase for local variables and parameters
  - Use meaningful names
- Keep methods focused and concise
- Add XML documentation comments for public APIs
- Use `var` when the type is obvious

### Example:
```csharp
/// <summary>
/// Compares two type definitions for semantic differences.
/// </summary>
/// <param name="typeName">The fully-qualified name of the type.</param>
/// <param name="type1">The first type definition.</param>
/// <param name="type2">The second type definition.</param>
/// <returns>A list of differences found between the types.</returns>
static List<string> CompareTypeInfo(string typeName, TypeInfo type1, TypeInfo type2)
{
    var differences = new List<string>();
    // Implementation...
    return differences;
}
```

## Development Setup

1. Install **.NET 8.0 SDK** or later
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/CompareProxyCsFile.git
   cd CompareProxyCsFile
   ```
3. Build the project:
   ```bash
   cd CompareProxyCsFile
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run -- file1.cs file2.cs
   ```

## Testing

Before submitting a PR, test your changes with various scenarios:

- Files with identical content
- Files with different type orders
- Files with different generator versions
- Files with actual semantic differences
- Invalid/malformed C# files
- Large files with many types

## Commit Messages

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit first line to 72 characters
- Reference issues and pull requests after the first line

### Example:
```
Add support for comparing nested types

- Extract nested class definitions
- Compare inner types recursively
- Update report format for nested differences

Fixes #123
```

## Project Structure

```
CompareProxyCsFile/
??? CompareProxyCsFile/
?   ??? Program.cs              # Main application logic
?   ??? CompareProxyCsFile.csproj  # Project file
??? README.md               # Project documentation
??? LICENSE                 # MIT License
??? CHANGELOG.md            # Version history
??? .gitignore              # Git ignore rules
```

## Questions?

Feel free to open an issue with the `question` label.

Thank you for your contribution! ??
