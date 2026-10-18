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
8. **Moderna y llamativa, con propósito.** Color, movimiento y celebraciones que motiven, sin distraer de la tarea (ver §5).

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
| M | **Color propio por rutina** | Toda la UI se re-tematiza con el color de la rutina activa (ver §5). |
| C | **Temas y packs desbloqueables** | Paletas, sonidos y celebraciones nuevas como recompensa. |
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

## 5. Diseño visual: moderno y llamativo

### 5.1 Dirección de arte: *"Dopamina con calma"*

Una UI **vibrante, juguetona y con mucha personalidad**, que dé ganas de abrirla, pero que **nunca compita con la tarea**. La regla:

> **Llamativa en los bordes, calmada en el centro.** El color, el movimiento y la diversión van en las transiciones, las celebraciones y la pantalla "Hoy". La pantalla de enfoque se queda limpia: un paso, un anillo y un botón.

**Referencias de estilo:** Material 3 *Expressive*, tarjetas tipo *bento*, formas redondeadas y suaves, colores saturados sobre fondos neutros, micro-interacciones con háptica (estilo Duolingo, Headspace y Finch).

### 5.2 Rasgos visuales

| Elemento | Decisión |
|---|---|
| **Color** | Paleta saturada con un **color propio por rutina** (Mañana = mandarina, Noche = índigo, Estudio = menta…). Al entrar a una rutina, toda la UI **toma su color**: fondo, anillo y botón. Da identidad y ayuda a la memoria visual. |
| **Gradientes** | Gradientes suaves de dos tonos en la tarjeta principal, el anillo del temporizador y el botón "Empezar ahorita". Nunca sobre texto largo. |
| **Formas** | Esquinas muy redondeadas (radio de 24–32 dp en las tarjetas y forma de píldora en los botones). Blobs orgánicos de fondo en el onboarding y las celebraciones. |
| **Tipografía** | **Lexend** para el cuerpo: fue diseñada para reducir la fatiga lectora y se lleva bien con TDAH y dislexia. **Outfit** o **Bricolage Grotesque** en peso 700–800 para títulos grandes y con carácter. Números del temporizador con cifras tabulares. |
| **Emoji e íconos** | Cada rutina y cada paso lleva un emoji grande (3D tipo *Fluent Emoji*, con licencia MIT). Íconos de UI con trazo redondeado (*Phosphor* o *Material Symbols Rounded*). |
| **Profundidad** | Sombras suaves de color (tintadas con el color de la rutina, no grises) y capas, sin bordes duros. |
| **Modo oscuro** | Es de primera clase y no una inversión: fondo casi negro azulado (`#0F1020`) con los colores de rutina en versión neón suave. |
| **Material You** | Opcional en Android 12+: un tema "Usar colores de mi fondo de pantalla" que adapta los neutros y conserva los colores de las rutinas. |
| **Edge-to-edge** | Contenido detrás de la barra de estado y de navegación, con la barra de estado del color de la pantalla. |

### 5.3 Pantallas clave con su toque llamativo

**Hoy**
```
┌──────────────────────────────┐
│ Buenos días, Vale ☀️          │  ← título grande, Outfit 800
│ ┌──────────────────────────┐ │
│ │ ░░ gradiente mandarina ░░ │ │  ← tarjeta héroe (bento grande)
│ │ 🌅 Rutina de mañana       │ │
│ │ 6 pasos · ~25 min         │ │
│ │ ┌──────────────────────┐  │ │
│ │ │  ▶ EMPEZAR AHORITA   │  │ │  ← píldora blanca, "respira" suavemente
│ │ └──────────────────────┘  │ │
│ └──────────────────────────┘ │
│ ┌───────────┐ ┌────────────┐ │
│ │ 📚 Estudio │ │ 🌙 Noche    │ │  ← bento pequeño, cada uno con su color
│ │ 16:00      │ │ 22:30      │ │
│ └───────────┘ └────────────┘ │
│ ┌──────────────────────────┐ │
│ │ ●●●●○○○  5 de 7 días 🔥   │ │  ← constancia flexible, nunca en rojo
│ └──────────────────────────┘ │
└──────────────────────────────┘
```

**Enfoque (Modo Ahora)**
- Fondo con el color de la rutina en un tono muy suave.
- **Anillo grueso con gradiente** que se vacía, con un punto brillante en la punta. Al llegar al 10 % cambia de tono con suavidad, sin parpadear.
- Emoji del paso a 96 dp, con un pequeño rebote al aparecer.
- Botón "LISTO ✓" gigante. Al tocarlo se encoge un poco, vibra, se llena de color y el siguiente paso **entra deslizándose desde la derecha**.
- **Deslizar hacia arriba para terminar el paso** como gesto alternativo, muy satisfactorio.

**Celebración**
- Confeti o fuegos artificiales en **Lottie** (2–3 s), con una variante distinta cada vez para que no aburra.
- Un número grande animado ("¡6/6!") y un mensaje positivo aleatorio.
- La háptica sigue un patrón "tada" en lugar de una vibración genérica.

**Editor de rutina**
- Los pasos son tarjetas que se arrastran con una animación elástica.
- Elegir el color de la rutina con círculos grandes, y el emoji con un buscador.
- Vista previa en vivo de cómo se verá la tarjeta en "Hoy".

### 5.4 Movimiento (motion)

| Momento | Animación | Duración |
|---|---|---|
| Tocar un botón | Escala 0.95 → 1 con rebote + háptica ligera | 120 ms |
| Paso completado | El check se dibuja y la tarjeta sale deslizándose | 250 ms |
| Paso siguiente | Entra deslizándose con *ease-out* y el emoji rebota | 300 ms |
| Botón "Empezar" | "Respira" (escala 1 ↔ 1.03) en loop lento para invitar a tocarlo | 2 s |
| Fin de rutina | Celebración Lottie + contador animado | 2–3 s |
| Navegación | Transiciones compartidas: la tarjeta de "Hoy" se expande a la pantalla de enfoque | 350 ms |

**Reglas:**
- Todo el movimiento **tiene un propósito**: confirmar, orientar o premiar. Nada de animaciones decorativas permanentes en el modo enfoque, salvo el anillo.
- Respetar la escala de animaciones del sistema y "Quitar animaciones" de Android. **El modo de bajo estímulo** las apaga todas, junto con el confeti, los gradientes y los sonidos, y deja una UI plana y tranquila.

### 5.5 Sonido y háptica

- Un set propio de sonidos cortos y suaves (pop al completar, campanita al terminar) con un selector de "pack de sonidos" como recompensa desbloqueable.
- Patrones hápticos distintos para *tap*, *paso completado*, *aviso de tiempo* y *celebración*. Se aprenden sin mirar la pantalla.

### 5.6 Design system en MAUI

```
Resources/
├── Styles/
│   ├── Tokens.Colors.xaml      Paleta base + colores de rutina (claro/oscuro con AppThemeBinding)
│   ├── Tokens.Typography.xaml  Escala tipográfica: Display, Title, Body, Label, Timer
│   ├── Tokens.Spacing.xaml     Grid de 4/8 dp, radios (Sm 12, Md 20, Lg 28, Pill 999)
│   ├── Tokens.Motion.xaml      Duraciones y easings con nombre
│   └── Components.xaml         Estilos de BigActionButton, BentoCard, Chip, StepCard…
└── Fonts/                      Lexend, Outfit (OFL)
```

- **Tokens semánticos, no literales:** `SurfaceColor`, `OnSurfaceColor`, `RoutineAccent`, `RoutineAccentSoft`, `RoutineGradient`. El color de rutina se inyecta como recurso dinámico (`DynamicResource`) al entrar a la rutina, y así toda la pantalla se re-tematiza sin tocar las vistas.
- **Componentes propios reutilizables** en `Controls/`:
  - `BentoCard` (Border + RoundRectangle + Shadow de color).
  - `BigActionButton` (con animación de *press* y háptica integradas).
  - `VisualTimerView` (`GraphicsView`/`IDrawable` con arco de gradiente a 60 fps).
  - `EmojiBadge`.
  - `CelebrationOverlay`.
- **Librerías:**
  - **CommunityToolkit.Maui**: animaciones, `StatusBarBehavior`, popups.
  - **SkiaSharp.Extended.UI.Maui**: `SKLottieView` para las celebraciones y `SKConfettiView`.
  - **Microsoft.Maui.Graphics**: el anillo.
- **Handlers personalizados** para quitar el subrayado nativo de `Entry` y `Picker` y lograr campos redondeados limpios.
- **Rendimiento:**
  - Lottie de menos de 100 KB.
  - No usar blur real (es caro en Android). El efecto "glass" se simula con superficies semitransparentes y sombra.
  - Medir los frames con el GPU profiler. La meta es no bajar de 60 fps en gama media.

### 5.7 Llamativo sin dejar de ser accesible

- Contraste **AA (4.5:1)** en todo el texto, incluido el texto sobre gradientes (se verifica contra el tono más claro).
- **El color nunca es la única señal:** cada rutina tiene color **y** emoji, y cada estado tiene color **y** ícono **y** texto.
- Las animaciones se pueden desactivar, el texto escala hasta 200 % sin cortes y las áreas táctiles miden 48 dp o más.
- Se valida con usuarios: lo "llamativo" se ajusta si distrae o sobreestimula.

---

## 6. Arquitectura técnica

### 6.1 Stack

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

### 6.2 Clean Architecture

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

### 6.3 Estructura de la solución

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

### 6.4 Modelo de dominio

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

### 6.5 Ejemplos de código (estilo de referencia)

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

### 6.6 Buenas prácticas y convenciones

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

### 6.7 Detalles específicos de Android

| Tema | Decisión |
|---|---|
| Permisos | `POST_NOTIFICATIONS` (API 33+), `SCHEDULE_EXACT_ALARM` / `USE_EXACT_ALARM` (justificar como app de alarmas/rutinas), `FOREGROUND_SERVICE` + `FOREGROUND_SERVICE_SPECIAL_USE`, `RECEIVE_BOOT_COMPLETED`, `VIBRATE`. Todos pedidos en contexto. |
| Doze / optimización de batería | `setExactAndAllowWhileIdle`; pantalla guiada para excluir la app de optimización si el fabricante (Xiaomi, Samsung, Huawei) mata alarmas. |
| Reinicio del teléfono | `BootReceiver` reprograma todas las alarmas desde SQLite. |
| Canales de notificación | "Recordatorios", "Rutina en curso" (silencioso, persistente), "Celebraciones". El usuario controla cada uno. |
| Acciones en notificación | `PendingIntent` → `BroadcastReceiver` → caso de uso de `Application` (sin abrir la app). |
| Widget | `AppWidgetProvider` nativo en `Platforms/Android/Widgets` con RemoteViews; lee un snapshot ligero escrito por la app. |

---

## 7. Estrategia de pruebas

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

## 8. Roadmap

| Fase | Duración aprox. | Entregables |
|---|---|---|
| **0. Fundaciones** | 1 semana | Solución y capas, CI, `.editorconfig`, design system (tokens, fuentes, `BentoCard`, `BigActionButton`), Shell con 3 pestañas, DB + migración inicial. |
| **1. Rutinas** | 2 semanas | CRUD de rutinas y pasos, reordenar, plantillas, programación por días/hora. |
| **2. Modo Ahora** | 2 semanas | Pantalla de enfoque con re-tematizado por rutina, temporizador visual con gradiente, micro-animaciones y háptica, saltar/posponer, hora estimada de fin, celebración Lottie. |
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

## 9. Métricas de éxito (locales, sin rastreo)

- % de recordatorios que terminan en "Empezar" (vs. "Hoy no").
- Tiempo desde abrir la app hasta iniciar una rutina.
- Proporción media de pasos completados por sesión.
- Retención propia a 4 semanas (medida en estudio con usuarios, no con analítica).

---

## 10. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Fabricantes Android que matan alarmas | Guía por fabricante (dontkillmyapp), alarmas exactas, prueba en Xiaomi/Samsung reales. |
| Política de Google Play sobre `USE_EXACT_ALARM` / foreground services | Declarar uso con justificación clara; fallback a `SCHEDULE_EXACT_ALARM` solicitado al usuario. |
| MAUI sin soporte nativo para widgets | Implementación nativa aislada en `Platforms/Android`, fuera del MVP. |
| UI llamativa que sobreestimula | Modo enfoque minimalista, modo de bajo estímulo, respetar la configuración de animaciones del sistema, pruebas con usuarios. |
| Sobrecarga de features (irónicamente, muy TDAH) | MoSCoW estricto; cualquier feature nueva debe pasar el filtro "¿reduce la fricción para empezar?". |
| Gamificación que genera culpa | Revisar todo el microcopy con usuarios; opciones para desactivar rachas y celebraciones. |
