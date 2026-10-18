# Ahorita — Plan de producto y arquitectura

App Android en **.NET MAUI** para crear y seguir rutinas, pensada para personas con **ADHD/TDAH**.

---

## 1. Visión

> Ayudar a personas con TDAH a **empezar**, **mantener el foco** y **terminar** sus rutinas diarias, sin culpa y con la menor fricción posible.

### Problemas del TDAH que la app debe atacar

| Problema | Cómo se manifiesta | Respuesta de diseño |
|---|---|---|
| **Parálisis de inicio** | "Sé lo que tengo que hacer pero no arranco" | Un solo botón gigante: **"Empezar ahorita"**. Primer paso ridículamente pequeño. |
| **Ceguera temporal** | No percibir cuánto tiempo pasa | Temporizadores visuales (disco que se vacía), avisos a mitad de paso, hora estimada de fin. |
| **Memoria de trabajo limitada** | Olvidar qué seguía | Mostrar **un solo paso a la vez**, siempre visible el "siguiente". |
| **Sobrecarga / decisiones** | Demasiadas opciones paralizan | Plantillas listas, valores por defecto sensatos, máximo 3 opciones por pantalla. |
| **Búsqueda de dopamina / aburrimiento** | Abandonar rutinas monótonas | Micro-recompensas, celebraciones, variedad, progreso visible. |
| **Sensibilidad al rechazo / culpa** | Rachas rotas → abandono total | **Sin castigos**. Rachas flexibles, "días de descanso", lenguaje compasivo. |
| **Hiperfoco** | Perder la noción del tiempo en una sola cosa | Alertas suaves de "¿sigues aquí?" y avisos de transición. |

### Principios de diseño (no negociables)

1. **Un toque para empezar.** Cualquier rutina se inicia desde la pantalla de inicio, notificación o widget en ≤ 1 toque.
2. **Una cosa a la vez.** Durante la ejecución, solo existe el paso actual.
3. **Cero culpa.** Nunca rojo de error por "fallar" una rutina. Nada de "perdiste tu racha".
4. **Fricción mínima para crear.** Crear una rutina en < 60 s usando plantilla.
5. **Recordatorios que no se ignoran… pero que no agobian.** Escalamiento suave y configurable.
6. **Offline-first y privado.** Todo funciona sin cuenta ni internet. Los datos son del usuario.
7. **Accesible por defecto.** Tipografía grande, alto contraste, modo de bajo estímulo.

---

## 2. Personas

- **Valeria, 24, estudiante universitaria** — Diagnóstico reciente. Necesita rutina matutina para llegar a clase y bloques de estudio. Usa el celular todo el día.
- **Diego, 35, desarrollador remoto** — Hiperfoco en el trabajo, olvida comer y moverse. Quiere transiciones y pausas.
- **Mariana, 41, madre de un niño de 9 años con TDAH** — Quiere rutinas visuales (con pictogramas) para su hijo: vestirse, mochila, dientes.

---

## 3. Propuesta de features

Priorizadas con **MoSCoW**: `M` = Must (MVP), `S` = Should (v1.1), `C` = Could (v2), `W` = Won't (por ahora).

### 3.1 Rutinas (núcleo)

| Prio | Feature | Descripción |
|---|---|---|
| M | **Crear/editar rutinas** | Nombre, emoji/ícono, color, lista ordenada de pasos. Reordenar con drag & drop. |
| M | **Pasos con duración** | Cada paso tiene duración estimada (opcional) y nota breve. |
| M | **Plantillas** | "Mañana", "Noche", "Salir de casa", "Bloque de estudio (Pomodoro)", "Limpieza rápida 10 min". Editables. |
| M | **Programación** | Días de la semana + hora de inicio, o "a demanda". |
| S | **Pasos opcionales / saltables** | Marcar pasos como "si me da tiempo". |
| S | **Sub-pasos (checklist dentro de un paso)** | p. ej. "Mochila": llaves, cartera, cargador. |
| S | **Duplicar y versiones cortas** | "Versión día malo": misma rutina con solo los pasos esenciales, un toque para cambiar. |
| C | **Rutinas encadenadas** | Al terminar "Mañana" sugiere "Salir de casa". |
| C | **Disparadores por contexto** | Iniciar al conectar audífonos, al llegar a casa (geocerca), al apagar alarma. |

### 3.2 Modo "Ahora" (ejecución de rutina)

| Prio | Feature | Descripción |
|---|---|---|
| M | **Pantalla de enfoque** | Un paso a la vez, texto grande, botón enorme "Listo ✓". Se ve el "siguiente" en pequeño. |
| M | **Temporizador visual** | Disco/anillo que se vacía (estilo Time Timer). Sin números si el usuario lo prefiere. |
| M | **Hora estimada de fin** | "Terminas a las 8:12" — combate la ceguera temporal. |
| M | **Saltar / posponer paso** | Sin penalización. "Lo hago después" lo manda al final. |
| M | **Ejecución en segundo plano** | Foreground service + notificación persistente con el paso actual y botón "Listo". |
| S | **Aviso a mitad de paso** | Vibración suave al 50 % y al 90 % del tiempo. |
| S | **Lectura en voz alta (TTS)** | Anuncia cada paso; ideal para manos ocupadas (ducha, cocina). |
| S | **"Body doubling" ligero** | Sonidos ambientales (café, lluvia, biblioteca) durante la rutina. |
| C | **Pantalla siempre encendida opcional** | Para rutinas de cocina o niños. |
| C | **Modo niño** | Pictogramas grandes, sin texto obligatorio, celebraciones más vistosas, bloqueo de edición con PIN. |

### 3.3 Recordatorios inteligentes

| Prio | Feature | Descripción |
|---|---|---|
| M | **Notificación de inicio** | Con acciones: **Empezar** / **En 5 min** / **Hoy no**. |
| M | **Avisos previos** | "En 10 min empieza tu rutina de noche". |
| S | **Escalamiento suave** | Si no responde: recordatorio a los 5 y 15 min, cada vez un poco más notorio; tope configurable. |
| S | **Horas silenciosas** | No molestar en ciertos horarios. |
| C | **Alarma de pantalla completa** | Para rutinas críticas (medicación, salir al trabajo). |

### 3.4 Motivación sin culpa

| Prio | Feature | Descripción |
|---|---|---|
| M | **Celebración al terminar** | Animación breve + vibración + mensaje positivo aleatorio. Desactivable. |
| M | **Progreso parcial cuenta** | Completar 3 de 5 pasos = 60 % y se celebra igual. |
| S | **Rachas flexibles** | "5 de los últimos 7 días" en lugar de racha consecutiva. Días de descanso planeados. |
| S | **Historial amable** | Calendario con intensidad de color (sin rojo). Insight: "Las mañanas de martes te cuestan más". |
| C | **Colección / recompensas** | Desbloquear stickers, temas o sonidos. Variedad = dopamina. |
| C | **Recompensa personal** | El usuario define su premio: "Al terminar: 10 min de videojuego". |

### 3.5 Ajustes de estimación (ceguera temporal)

| Prio | Feature | Descripción |
|---|---|---|
| S | **Tiempo real vs. estimado** | Se registra cuánto tardó cada paso. |
| S | **Sugerencia de ajuste** | "Ducharte te toma 18 min en promedio, no 10. ¿Actualizamos?" |

### 3.6 Plataforma Android

| Prio | Feature | Descripción |
|---|---|---|
| S | **Widget de pantalla de inicio** | Próxima rutina + botón "Empezar". (Requiere código nativo en `Platforms/Android`.) |
| S | **Accesos directos (App Shortcuts)** | Mantener presionado el ícono → iniciar rutina favorita. |
| C | **Quick Settings Tile** | Iniciar/pausar la rutina activa desde el panel. |
| C | **Wear OS / respaldo en la nube** | Fuera del alcance inicial. |

### 3.7 Accesibilidad y personalización

| Prio | Feature | Descripción |
|---|---|---|
| M | **Tema claro/oscuro + alto contraste** | Respeta el tema del sistema. |
| M | **Escalado de fuente del sistema** | Toda la UI soporta tamaños grandes sin romperse. |
| S | **Modo bajo estímulo** | Sin animaciones, colores apagados, sin sonidos. |
| S | **Fuente amigable para dislexia** (opcional) | Comorbilidad frecuente con TDAH. |
| M | **Español e inglés** | Recursos `.resx` desde el día 1. |

### 3.8 Datos y privacidad

| Prio | Feature | Descripción |
|---|---|---|
| M | **Sin cuenta, offline** | SQLite local. |
| S | **Exportar / importar** | Archivo JSON; compartir rutinas con otra persona. |
| W | **Analítica de terceros** | No. Como mucho, telemetría de crashes opt-in. |

### Fuera de alcance (Won't, v1)

- Red social / comunidad.
- Gestión de tareas genérica (tipo to-do). Ahorita es de **rutinas**, no un Todoist.
- IA generativa en el MVP (ver ideas futuras).

### Ideas futuras (v2+)

- **"Rómpelo por mí"**: describir una tarea abrumadora y recibir pasos pequeños sugeridos (LLM, opt-in, con aviso de privacidad).
- **Check-in de energía**: "¿Cómo estás hoy? 🔋" → sugiere versión completa o versión día malo.
- **Recordatorio de medicación** integrado a la rutina matutina.

---

## 4. Flujos UX clave

### 4.1 Onboarding (≤ 4 pantallas, todo saltable)
1. Bienvenida: "Rutinas que se adaptan a tu cerebro, no al revés."
2. Elegir 1–2 plantillas para empezar.
3. Elegir hora (selector simple).
4. Permiso de notificaciones **explicado en contexto** (no al abrir la app).

### 4.2 Navegación (Shell, 3 pestañas)
- **Hoy** — Lista de rutinas de hoy, la próxima destacada con botón grande "Empezar ahorita".
- **Rutinas** — Biblioteca y editor.
- **Progreso** — Historial amable.
- Ajustes en ícono de engrane (no es pestaña).

### 4.3 Ejecución

```
┌────────────────────────────┐
│  Rutina de mañana  2/6     │
│                            │
│        ◔  4:12             │   ← anillo que se vacía
│                            │
│     🪥 Lavarte los dientes │   ← texto grande
│                            │
│  ┌──────────────────────┐  │
│  │       LISTO ✓        │  │   ← botón ≥ 30 % del alto
│  └──────────────────────┘  │
│   Saltar     Después  ⏸    │
│                            │
│  Sigue: 👕 Vestirte        │
│  Terminas a las 8:12       │
└────────────────────────────┘
```

### 4.4 Guía de microcopy
- ✅ "¡Hecho! Eso cuenta." · "Hoy hiciste 3 de 5. Bien." · "¿Lo dejamos para después?"
- ❌ "Fallaste" · "Perdiste tu racha" · "Tarea vencida"

---

## 5. Arquitectura técnica

### 5.1 Stack

| Área | Elección | Motivo |
|---|---|---|
| Framework | **.NET 10 (LTS) + .NET MAUI** | Soporte largo; target Android (min API 26, target API 35+). |
| Patrón UI | **MVVM** con **CommunityToolkit.Mvvm** | Source generators (`[ObservableProperty]`, `[RelayCommand]`), menos boilerplate. |
| Controles/extensiones | **CommunityToolkit.Maui** | Behaviors, converters, popups, `Snackbar`. |
| Navegación | **Shell** + servicio `INavigationService` | Desacopla ViewModels de Shell. |
| Persistencia | **SQLite** vía **EF Core SQLite** (o `sqlite-net-pcl` si el arranque pesa) | Migraciones, LINQ, repositorios testeables. |
| DI | `Microsoft.Extensions.DependencyInjection` en `MauiProgram` | Nativo de MAUI. |
| Notificaciones | `AlarmManager` + `NotificationCompat` vía interfaz propia (implementación en `Platforms/Android`) | Control fino de alarmas exactas, acciones y canales. Alternativa rápida: `Plugin.LocalNotification`. |
| Rutina activa | **Foreground Service** Android (tipo `specialUse`) | El temporizador sobrevive a la app en segundo plano. |
| TTS | `TextToSpeech` de MAUI Essentials | Ya incluido. |
| Logging | `Microsoft.Extensions.Logging` | Estándar. |
| Tests | **xUnit** + **FluentAssertions/Shouldly** + **NSubstitute** | Dominio y casos de uso 100 % testeables sin emulador. |
| CI | GitHub Actions: build, tests, `dotnet format --verify-no-changes`, APK firmado en tags | |

### 5.2 Clean Architecture

```
            ┌──────────────────────────┐
            │   Ahorita.App (MAUI)     │  Views, ViewModels, Platforms/Android
            └────────────┬─────────────┘
                         │ depende de
            ┌────────────▼─────────────┐
            │  Ahorita.Application     │  Casos de uso, DTOs, interfaces (puertos)
            └────────────┬─────────────┘
                         │
            ┌────────────▼─────────────┐
            │    Ahorita.Domain        │  Entidades, value objects, reglas. SIN dependencias
            └──────────────────────────┘
                         ▲
            ┌────────────┴─────────────┐
            │  Ahorita.Infrastructure  │  EF Core/SQLite, repositorios, reloj, JSON export
            └──────────────────────────┘
```

**Regla de dependencias:** `Domain` no referencia nada; `Application` solo a `Domain`; `Infrastructure` implementa interfaces de `Application`; la app MAUI compone todo en `MauiProgram`. Las APIs de Android (alarmas, foreground service, widget) son **adaptadores** detrás de interfaces de `Application` (`IReminderScheduler`, `IActiveRoutineHost`, `IHapticFeedback`).

### 5.3 Estructura de la solución

```
Ahorita.sln
├── src/
│   ├── Ahorita.Domain/
│   │   ├── Routines/        Routine, RoutineStep, Schedule, RoutineVariant
│   │   ├── Sessions/        RoutineSession, StepResult, SessionStatus
│   │   ├── Progress/        ConsistencyScore (racha flexible)
│   │   └── Common/          Entity, ValueObject, Result<T>, DomainErrors
│   ├── Ahorita.Application/
│   │   ├── Abstractions/    IRoutineRepository, ISessionRepository, IClock,
│   │   │                    IReminderScheduler, IActiveRoutineHost, IUnitOfWork
│   │   ├── Routines/        CreateRoutine, UpdateRoutine, CreateFromTemplate…
│   │   ├── Sessions/        StartSession, CompleteStep, SkipStep, PostponeStep, FinishSession
│   │   ├── Progress/        GetWeeklySummary, SuggestDurationAdjustments
│   │   └── DependencyInjection.cs
│   ├── Ahorita.Infrastructure/
│   │   ├── Persistence/     AhoritaDbContext, Configurations/, Migrations/, Repositories/
│   │   ├── Templates/       plantillas embebidas (JSON)
│   │   ├── Time/            SystemClock
│   │   └── DependencyInjection.cs
│   └── Ahorita.App/
│       ├── Features/
│       │   ├── Today/       TodayPage.xaml, TodayViewModel.cs
│       │   ├── Routines/    RoutineListPage, RoutineEditorPage + VMs
│       │   ├── Focus/       FocusPage (ejecución), FocusViewModel, VisualTimer control
│       │   ├── Progress/
│       │   ├── Onboarding/
│       │   └── Settings/
│       ├── Controls/        VisualTimerView (GraphicsView), BigActionButton
│       ├── Services/        NavigationService, DialogService
│       ├── Resources/       Styles, Colors (tokens), Strings (.resx es/en), Fonts
│       ├── Platforms/Android/
│       │   ├── Reminders/   AndroidReminderScheduler, ReminderReceiver, BootReceiver
│       │   ├── ActiveRoutine/ RoutineForegroundService, NotificationActions
│       │   └── Widgets/     NextRoutineWidgetProvider
│       └── MauiProgram.cs
└── tests/
    ├── Ahorita.Domain.Tests/
    ├── Ahorita.Application.Tests/
    └── Ahorita.Infrastructure.Tests/   (SQLite in-memory)
```

Organización **por feature** (vertical slices) dentro de la app, no por tipo de archivo: todo lo de "Focus" vive junto.

### 5.4 Modelo de dominio

```
Routine (aggregate root)
 ├─ Id, Name, Emoji, ColorHex, IsArchived
 ├─ Schedule: DaysOfWeek (flags) + TimeOnly? StartTime + ReminderLeadTime
 ├─ Steps: List<RoutineStep>  (Order, Title, Emoji, TimeSpan? Duration, IsOptional, Note)
 └─ Variants: "Completa" / "Día malo" (subconjunto de StepIds)

RoutineSession (aggregate root)  — una ejecución concreta
 ├─ Id, RoutineId, VariantId, StartedAt, FinishedAt?, Status (Running/Paused/Finished/Abandoned)
 └─ StepResults: List<StepResult> (StepId, Outcome: Done/Skipped/Postponed, ActualDuration)
     → CompletionRatio, EstimatedEnd(IClock)

ConsistencyScore (value object) — "X de los últimos N días", sin noción de "racha rota"
```

Reglas que viven en el **dominio** (testeables sin UI):
- Una rutina necesita al menos 1 paso; máximo recomendado 15 (aviso, no bloqueo).
- `PostponeStep` mueve el paso al final de la sesión, una sola vez.
- `FinishSession` es válido con cualquier progreso; el 0 % se registra como "Abandoned" pero **nunca** se muestra como fallo.
- `EstimatedEnd = now + restante del paso actual + Σ duración de pasos pendientes`.

### 5.5 Ejemplos de código (estilo de referencia)

**Dominio — entidad con invariantes y sin dependencias:**

```csharp
namespace Ahorita.Domain.Sessions;

public sealed class RoutineSession : Entity<Guid>
{
    private readonly List<StepResult> _results = [];
    private readonly List<Guid> _pendingStepIds;

    public Guid RoutineId { get; }
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public SessionStatus Status { get; private set; } = SessionStatus.Running;

    public IReadOnlyList<StepResult> Results => _results;
    public DateTimeOffset CurrentStepStartedAt { get; private set; }
    public Guid? CurrentStepId => _pendingStepIds.Count > 0 ? _pendingStepIds[0] : null;
    public double CompletionRatio => (double)_results.Count(r => r.Outcome == StepOutcome.Done)
                                     / (_results.Count + _pendingStepIds.Count);

    public Result CompleteCurrentStep(DateTimeOffset now)
    {
        if (Status != SessionStatus.Running || CurrentStepId is not { } stepId)
            return Result.Failure(SessionErrors.NotRunning);

        _pendingStepIds.RemoveAt(0);
        _results.Add(new StepResult(stepId, StepOutcome.Done, now - CurrentStepStartedAt));
        CurrentStepStartedAt = now;
        return Result.Success();
    }

    // SkipCurrentStep, PostponeCurrentStep, Finish…
}
```

**Aplicación — caso de uso pequeño, una responsabilidad:**

```csharp
namespace Ahorita.Application.Sessions;

public sealed class CompleteStep(
    ISessionRepository sessions,
    IActiveRoutineHost activeRoutine,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<SessionSnapshot>> ExecuteAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await sessions.GetAsync(sessionId, ct);
        if (session is null) return SessionErrors.NotFound;

        var result = session.CompleteCurrentStep(clock.Now);
        if (result.IsFailure) return result.Error;

        await unitOfWork.SaveChangesAsync(ct);

        var snapshot = SessionSnapshot.From(session, clock);
        await activeRoutine.UpdateAsync(snapshot, ct); // refresca la notificación persistente
        return snapshot;
    }
}
```

**Presentación — ViewModel delgado con CommunityToolkit.Mvvm:**

```csharp
namespace Ahorita.App.Features.Focus;

public sealed partial class FocusViewModel(
    CompleteStep completeStep,
    IHapticFeedback haptics,
    INavigationService navigation) : ViewModelBase, IQueryAttributable
{
    [ObservableProperty] private string _stepTitle = string.Empty;
    [ObservableProperty] private string? _nextStepTitle;
    [ObservableProperty] private double _remainingFraction = 1;
    [ObservableProperty] private string _estimatedEndText = string.Empty;

    private Guid _sessionId;

    [RelayCommand]
    private async Task DoneAsync()
    {
        var result = await completeStep.ExecuteAsync(_sessionId);
        if (result.IsFailure) { await ShowFriendlyErrorAsync(result.Error); return; }

        haptics.Success();
        if (result.Value.IsFinished)
            await navigation.GoToAsync(Routes.Celebration, ("sessionId", _sessionId));
        else
            Apply(result.Value);
    }

    // Apply(snapshot), ApplyQueryAttributes, SkipCommand, PostponeCommand…
}
```

**Composición en `MauiProgram`:**

```csharp
builder
    .UseMauiApp<App>()
    .UseMauiCommunityToolkit();

builder.Services
    .AddApplication()
    .AddInfrastructure(Path.Combine(FileSystem.AppDataDirectory, "ahorita.db"))
    .AddPresentation();   // Views + ViewModels + servicios de plataforma

#if ANDROID
builder.Services.AddSingleton<IReminderScheduler, AndroidReminderScheduler>();
builder.Services.AddSingleton<IActiveRoutineHost, AndroidActiveRoutineHost>();
#endif
```

### 5.6 Buenas prácticas y convenciones

- **SOLID** y **casos de uso pequeños** (un archivo = una intención del usuario). Sin "Services" gigantes.
- **`Result<T>`** para errores esperados; excepciones solo para lo excepcional.
- **Sin lógica en code-behind** salvo animaciones puramente visuales.
- **Nada de `DateTime.Now` directo**: siempre `IClock` (clave para probar temporizadores y recordatorios).
- **Nullable reference types** activado, `TreatWarningsAsErrors` en `Domain` y `Application`.
- **`.editorconfig`** + `dotnet format` en CI; analizadores `Microsoft.CodeAnalysis.NetAnalyzers` nivel `recommended`.
- **Async hasta el fondo**, `CancellationToken` en toda la capa de aplicación; nunca `.Result`/`.Wait()`.
- **Design tokens** en `Resources/Styles` (colores, espaciados, tamaños); nada de colores hardcodeados en XAML.
- **Compiled bindings** (`x:DataType`) en todas las vistas → rendimiento y errores en compilación.
- **Accesibilidad:** `SemanticProperties.Description` en controles, áreas táctiles ≥ 48 dp, probar con TalkBack.
- **Rendimiento:** trimming + AOT/ R2R en Release, arranque en frío objetivo < 1.5 s en gama media.
- **Commits convencionales** (`feat:`, `fix:`, `refactor:`), PRs pequeños, ramas por feature.

### 5.7 Detalles específicos de Android

| Tema | Decisión |
|---|---|
| Permisos | `POST_NOTIFICATIONS` (API 33+), `SCHEDULE_EXACT_ALARM` / `USE_EXACT_ALARM` (justificar como app de alarmas/rutinas), `FOREGROUND_SERVICE` + `FOREGROUND_SERVICE_SPECIAL_USE`, `RECEIVE_BOOT_COMPLETED`, `VIBRATE`. Todos pedidos en contexto. |
| Doze / optimización de batería | `setExactAndAllowWhileIdle`; pantalla guiada para excluir la app de optimización si el fabricante (Xiaomi, Samsung, Huawei) mata alarmas. |
| Reinicio del teléfono | `BootReceiver` reprograma todas las alarmas desde SQLite. |
| Canales de notificación | "Recordatorios", "Rutina en curso" (silencioso, persistente), "Celebraciones". El usuario controla cada uno. |
| Acciones en notificación | `PendingIntent` → `BroadcastReceiver` → caso de uso de `Application` (sin abrir la app). |
| Widget | `AppWidgetProvider` nativo en `Platforms/Android/Widgets` con RemoteViews; lee un snapshot ligero escrito por la app. |

---

## 6. Estrategia de pruebas

| Nivel | Qué | Herramienta | Meta |
|---|---|---|---|
| Unitarias dominio | Sesiones, reglas de pasos, `ConsistencyScore`, `EstimatedEnd` | xUnit | ~100 % de reglas |
| Unitarias aplicación | Casos de uso con repos falsos e `IClock` controlado | xUnit + NSubstitute | Todos los casos de uso |
| Integración | Repos con SQLite in-memory, migraciones | xUnit | Cada repositorio |
| ViewModels | Comandos y estado (no dependen de MAUI gracias a interfaces) | xUnit | Flujos principales |
| UI / E2E | Crear rutina → ejecutar → celebrar | Appium + emulador en CI (v1.1) | Smoke tests |
| Manual | TalkBack, fuente 200 %, modo oscuro, Doze, reinicio | Checklist de release | Cada release |
| Usuarios | 5–8 personas con TDAH probando el MVP | Entrevistas + diario de uso | Antes de v1.0 pública |

---

## 7. Roadmap

| Fase | Duración aprox. | Entregables |
|---|---|---|
| **0. Fundaciones** | 1 semana | Solución y capas, CI, `.editorconfig`, design tokens, Shell con 3 pestañas, DB + migración inicial. |
| **1. Rutinas** | 2 semanas | CRUD de rutinas y pasos, reordenar, plantillas, programación por días/hora. |
| **2. Modo Ahora** | 2 semanas | Pantalla de enfoque, temporizador visual, saltar/posponer, hora estimada de fin, celebración. |
| **3. Recordatorios** | 1–2 semanas | Alarmas exactas, acciones en notificación, foreground service, boot receiver, onboarding de permisos. |
| **4. MVP pulido** | 1 semana | Progreso básico, accesibilidad, español/inglés, beta cerrada en Play Console (pista interna). |
| **5. v1.1** | 3–4 semanas | Versión día malo, sub-pasos, rachas flexibles, aviso a mitad de paso, TTS, widget, ajuste de estimaciones, exportar/importar. |
| **6. v2** | — | Modo niño, disparadores por contexto, recompensas, check-in de energía, "Rómpelo por mí". |

### Definición de "MVP listo"
- Crear una rutina desde plantilla en < 60 s.
- Recordatorio llega a tiempo con la app cerrada y tras reiniciar el teléfono.
- Ejecutar una rutina completa con la pantalla bloqueada usando solo la notificación.
- Cero crashes en 1 semana de beta con ≥ 5 usuarios.

---

## 8. Métricas de éxito (locales, sin rastreo)

- % de recordatorios que terminan en "Empezar" (vs. "Hoy no").
- Tiempo desde abrir la app hasta iniciar una rutina.
- Proporción media de pasos completados por sesión.
- Retención propia a 4 semanas (medida en estudio con usuarios, no con analítica).

---

## 9. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Fabricantes Android que matan alarmas | Guía por fabricante (dontkillmyapp), alarmas exactas, prueba en Xiaomi/Samsung reales. |
| Política de Google Play sobre `USE_EXACT_ALARM` / foreground services | Declarar uso con justificación clara; fallback a `SCHEDULE_EXACT_ALARM` solicitado al usuario. |
| MAUI sin soporte nativo para widgets | Implementación nativa aislada en `Platforms/Android`, fuera del MVP. |
| Sobrecarga de features (irónicamente, muy TDAH) | MoSCoW estricto; cualquier feature nueva debe pasar el filtro "¿reduce la fricción para empezar?". |
| Gamificación que genera culpa | Revisar todo el microcopy con usuarios; opciones para desactivar rachas y celebraciones. |
