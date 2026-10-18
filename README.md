# Ahorita

App Android (.NET MAUI) para crear y seguir rutinas, diseñada para personas con ADHD/TDAH.

> "Ahorita": porque empezar es lo más difícil.

- 📋 Plan del producto, diseño y arquitectura: [`docs/PLAN.md`](docs/PLAN.md)
- ✅ Guía de pruebas del MVP: [`docs/PRUEBAS.md`](docs/PRUEBAS.md)

## Qué hace

- **Hoy**: la próxima rutina como protagonista y un botón gigante "Empezar ahorita". Lo que ya pasó no aparece: cero culpa.
- **Modo Ahora**: un paso a la vez, anillo que se vacía, "Terminas a las…", saltar, posponer, pausar o terminar antes.
- **Celebración** al terminar (también con progreso parcial).
- **Rutinas**: editor con vista previa en vivo, emojis, color, días, hora, aviso previo y pasos reordenables; 6 plantillas.
- **Recordatorios**: aviso previo y "es hora" con Empezar / En 5 min / Hoy no; notificación de la rutina en curso con cuenta regresiva y botón Listo.
- **Progreso amable**: "5 de 7 días" (sin rachas rotas), mapa de 4 semanas y sugerencias de duración basadas en lo que realmente tardas.
- **Ajustes**: bajo estímulo, celebraciones, vibración, lectura en voz alta, tema claro/oscuro.
- Offline y privada: sin cuentas, sin internet, sin rastreo. Español e inglés.

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

# APK/AAB de Release (firmados con la llave de depuración; para Play Store usa tu keystore)
dotnet publish src/Ahorita.App -c Release

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
  Ahorita.App/             MAUI: Features/ (Today, Routines, Editor, Focus, Celebration, Progress,
                           Settings, Onboarding), Controls/, Theming/, Services/, Resources/,
                           Platforms/Android/ (alarmas, notificaciones, receivers)
tests/                     xUnit v3 + Shouldly + NSubstitute
```

## Licencias de terceros

Fuentes Lexend y Outfit (SIL OFL 1.1) e íconos Phosphor (MIT): ver [`docs/licenses`](docs/licenses).
