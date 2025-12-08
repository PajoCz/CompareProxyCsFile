# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-12-08

### Added
- Initial release
- Semantic comparison of C# proxy files using Roslyn
- Ignores generator version differences in `GeneratedCodeAttribute`
- Order-independent type comparison
- Timestamped TXT report generation
- Detailed difference reporting for:
  - Type definitions (classes, interfaces, enums)
  - Members (methods, properties, fields)
  - Attributes
  - Base types and interfaces
  - Enum members
- Error handling with error report generation
- Exit codes for automation (0 = identical, 1 = differences/error)
- Support for .NET 8.0

### Features
- Compares two C# source files
- Generates comparison reports with timestamp
- Console output with summary
- Full path resolution in reports
- UTF-8 encoding support

[1.0.0]: https://github.com/PajoCz/CompareProxyCsFile/releases/tag/v1.0.0
