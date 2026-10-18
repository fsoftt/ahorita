# Ahorita

App Android (.NET MAUI) para crear y seguir rutinas, diseñada para personas con ADHD/TDAH.

> "Ahorita": porque empezar es lo más difícil.

- 📋 Plan del producto, diseño y arquitectura: [`docs/PLAN.md`](docs/PLAN.md)

## Requisitos

- .NET SDK 10 (ver `global.json`)
- Workload de Android: `dotnet workload install maui-android`
- JDK 17+ y Android SDK (API 36). En Linux/CI se pueden instalar con:
  `dotnet build src/Ahorita.App -t:InstallAndroidDependencies -p:AndroidSdkDirectory=<ruta> -p:AcceptAndroidSDKLicenses=True`

## Comandos

```bash
# Pruebas (dominio, aplicación e infraestructura; no requieren Android)
dotnet test --project tests/Ahorita.Domain.Tests
dotnet test --project tests/Ahorita.Application.Tests
dotnet test --project tests/Ahorita.Infrastructure.Tests

# Compilar e instalar la app en un dispositivo/emulador conectado
dotnet build src/Ahorita.App -t:Run

# Nueva migración tras cambiar el modelo
dotnet tool restore
dotnet ef migrations add <Nombre> -p src/Ahorita.Infrastructure -s src/Ahorita.Infrastructure -o Persistence/Migrations
```

## Estructura

```
src/
  Ahorita.Domain/          Entidades y reglas (sin dependencias)
  Ahorita.Application/     Casos de uso y puertos (interfaces)
  Ahorita.Infrastructure/  EF Core + SQLite, plantillas embebidas
  Ahorita.App/             MAUI: Features/ (Hoy, Rutinas, Progreso), Controls/, Theming/, Resources/
tests/                     xUnit v3 + Shouldly + NSubstitute
```

## Licencias de terceros

Fuentes Lexend y Outfit (SIL OFL 1.1) e íconos Phosphor (MIT): ver [`docs/licenses`](docs/licenses).
