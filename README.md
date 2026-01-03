# AstroComputationEngine

A cross-platform .NET MAUI application for astronomical computation and data visualization. This app demonstrates advanced astronomical calculations using the Swiss Ephemeris library and showcases AI integration for data analysis and pattern recognition.

## Features

- **Astronomical Data Analysis**: Generate detailed astronomical position reports
- **Multi-Object Calculations**: Analyze relationships between celestial bodies
- **Temporal Analysis**: Track celestial movements over time periods
- **AI-Powered Pattern Recognition**: Leverage machine learning for data interpretation
- **Swiss Ephemeris Integration**: High-precision planetary position calculations
- **Cross-Platform**: Runs on Android, iOS, macOS, and Windows

## Technologies Used

- **.NET MAUI**: Cross-platform UI framework
- **SwissEphNet**: Swiss Ephemeris library for precise astronomical calculations
- **OpenRouter API**: AI-powered data analysis and pattern recognition
- **C# 12**: Modern C# features with nullable reference types

## Project Structure

```
AstroComputationEngine/
├── Interfaces/          # Service contracts
├── Models/             # Data models for AI, astronomical data, and locations
├── Services/           # Business logic implementations
├── Utility/            # Helper classes and extensions
├── Views/              # XAML pages and UI
├── Resources/          # App resources (fonts, images, ephemeris data)
└── Platforms/          # Platform-specific code
```

## Getting Started

### Prerequisites

- .NET 8.0 or later
- Visual Studio 2022 with MAUI workload
- For mobile development: Android SDK and/or Xcode

### Configuration

1. Clone the repository
2. Open `AstroComputationEngine.sln` in Visual Studio
3. Configure your AI service API key (see Configuration section below)
4. Build and run the project

### Configuration

**Important**: Before running the application, you need to configure the AI service:

1. Create an account at [OpenRouter](https://openrouter.ai/)
2. Get your API key
3. Replace the hardcoded API key in `Services/AIService.cs` with your own key, or better yet, move it to configuration

**Security Note**: The current implementation has a hardcoded API key that should be moved to a secure configuration method before production use.

## Core Services

### ChartService
Handles all astronomical calculations including:
- Planetary position computations
- Coordinate system transformations
- Angular relationship analysis
- Data generation for different calculation types

### AIService
Integrates with OpenRouter API to provide:
- Data pattern analysis
- Computational insights
- Automated report generation

### CityService
Manages geographic location data for accurate positional calculations

## Usage

The app provides three main computational modes:

1. **Daily Analyzer**: Calculate celestial positions for specific dates and locations
2. **Composite Analyzer**: Compare astronomical data between two different time periods
3. **Davison Analyzer**: Generate midpoint-based calculations between datasets

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is open source. Please add an appropriate license file.

## Disclaimer

This application is designed for educational and computational demonstration purposes. It showcases astronomical calculation techniques and AI integration patterns for data analysis applications.

## Acknowledgments

- Swiss Ephemeris for high-precision astronomical calculations
- OpenRouter for AI-powered data analysis capabilities
- .NET MAUI team for the cross-platform framework