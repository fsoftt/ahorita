# Guía de pruebas del MVP

Checklist para probar Ahorita en un teléfono Android real (8.0+). Marca cada punto y anota lo que se sienta raro: para una app de TDAH, **"esto me dio flojera"** también es un bug.

## Instalación

1. Activa *Instalar apps desconocidas* para tu gestor de archivos o navegador.
2. Instala `com.fsoftt.ahorita-Signed.apk`.
3. Abre **Ahorita**.

> Para depurar desde una computadora: `dotnet build src/Ahorita.App -t:Run` con el teléfono conectado por USB (depuración USB activada).

## 1. Primera vez (≈ 2 min)

- [ ] Aparece la bienvenida en 3 pasos y se puede **saltar** en cualquier momento.
- [ ] En "¿Con cuál empezamos?" se pueden marcar varias plantillas.
- [ ] "Activar recordatorios" muestra el permiso de notificaciones de Android (Android 13+).
- [ ] Al terminar, **Hoy** muestra la rutina elegida como protagonista con su color.
- [ ] Cerrar y abrir la app **no** vuelve a mostrar la bienvenida.

## 2. Crear y editar (meta: < 60 s desde plantilla)

- [ ] **Rutinas → + Nueva rutina**: la vista previa cambia en vivo con el nombre, emoji y color.
- [ ] "Programada": se eligen días, hora y aviso previo.
- [ ] **+ Agregar paso**: el teclado aparece solo; la duración se elige con chips.
- [ ] Reordenar pasos **manteniendo presionado y arrastrando**; también con ↑ Subir / ↓ Bajar dentro del paso.
- [ ] Guardar sin nombre muestra un mensaje amable (no un error técnico).
- [ ] Editar una rutina existente y salir con "‹" **no** guarda cambios; "Guardar" sí.
- [ ] "Eliminar rutina" pide confirmación y la rutina desaparece de Hoy y de Rutinas.

## 3. Modo Ahora

- [ ] Desde **Hoy**, "Empezar ahorita" abre un paso a la vez con el anillo que se vacía.
- [ ] "Listo ✓" vibra, y el siguiente paso entra con animación.
- [ ] "Después" manda el paso al final (solo una vez por paso); "Saltar" avanza.
- [ ] "Pausar" congela el tiempo; "Seguir" lo reanuda.
- [ ] "Terminas a las…" se actualiza al avanzar.
- [ ] Si un paso se pasa de tiempo: el anillo queda vacío, el tiempo muestra `+0:12` y aparece un mensaje amable (sin rojo).
- [ ] La pantalla **no se apaga** mientras estás en el modo Ahora.
- [ ] Al terminar: confeti, vibración "tada" y mensaje positivo. Terminar antes también celebra ("Hiciste 3 de 5").
- [ ] Cerrar con "✕" a mitad: **Hoy** muestra "En curso · paso X de Y" y "Continuar" retoma donde ibas.

## 4. Notificaciones y recordatorios

Usa una rutina programada 3–5 minutos en el futuro con aviso previo de 5 min (o crea una para "ahora + 2 min").

- [ ] Llega el aviso previo ("En N min: …").
- [ ] A la hora llega "Es hora: …" con **Empezar / En 5 min / Hoy no**.
- [ ] Tocar la notificación (o "Empezar") abre **directo** el modo Ahora.
- [ ] "En 5 min" la vuelve a mostrar 5 minutos después.
- [ ] "Hoy no" la quita y no pasa nada más (sin culpa).
- [ ] Con la app cerrada (deslizada fuera de recientes) los avisos siguen llegando.
- [ ] **Tras reiniciar el teléfono**, los avisos siguen llegando.
- [ ] Rutina en curso: hay una notificación fija con el paso actual, barra de progreso y **cuenta regresiva**.
- [ ] "Listo ✓" desde la notificación avanza el paso sin abrir la app; al terminar llega "¡Lo lograste! 🎉".
- [ ] Ajustes → "Avisos a la hora exacta" → Activar abre la pantalla del sistema "Alarmas y recordatorios".

## 5. Progreso

- [ ] Sin actividad: mensaje amable con 🌱.
- [ ] Tras completar pasos: "X de 7 días", los últimos 7 días marcados y el mapa de 4 semanas (sin rojo).
- [ ] Tras 3+ sesiones donde un paso tarda bastante más de lo planeado: aparece la sugerencia y "Ajustar a N min" cambia la duración del paso.

## 6. Ajustes y accesibilidad

- [ ] **Modo de bajo estímulo**: sin animaciones, sin confeti, tarjetas de color plano (al volver a entrar a cada pantalla).
- [ ] Apagar "Celebraciones" quita el confeti; apagar "Vibración" quita la háptica.
- [ ] "Leer pasos en voz alta" lee cada paso al empezar.
- [ ] Tema Claro / Oscuro / Sistema cambia toda la app.
- [ ] Ajustes de Android → *Quitar animaciones*: la app respeta la preferencia.
- [ ] Tamaño de letra del sistema al máximo: nada se corta ni se encima.
- [ ] TalkBack: los botones principales se leen con sentido (Listo, Saltar, nombre de la rutina…).
- [ ] Idioma del teléfono en inglés: toda la app y las plantillas aparecen en inglés.
- [ ] Ajustes → **Idioma** → English / Español / Del teléfono: la app cambia al instante (sin cerrarla) y vuelve a Ajustes.
- [ ] Tras cambiar el idioma, las notificaciones nuevas y las fechas ("Terminas a las…", días de la semana) usan ese idioma.

## 7. Donar y apoyar con anuncios

Los APK de prueba (Debug y CI) muestran **anuncios de prueba** de Google (dicen "Test Ad"). **Nunca toques anuncios reales de tu propia app**: AdMob lo considera tráfico inválido.

- [ ] "Apoyar con anuncios" está **apagado** al instalar: no aparece ningún banner.
- [ ] Ajustes → *Apoyar con anuncios* encendido: aparece un banner pequeño al pie de Hoy, Rutinas, Progreso y Ajustes, sin tapar contenido.
- [ ] El banner **no** aparece en el modo Ahora, la celebración, los editores ni la bienvenida.
- [ ] Al apagarlo, el banner desaparece al momento.
- [ ] 💜 en Hoy (o Ajustes → Donar) abre la página Donar con el mismo texto que PiccoloReader.
- [ ] "Ver anuncio" muestra un anuncio bonificado; al cerrarlo: "¡Gracias por tu propina! 💜".
- [ ] Sin internet, "Ver anuncio" responde con un mensaje amable (no se queda cargando para siempre).
- [ ] En la UE/RU: aparece el formulario de consentimiento y en Ajustes la fila "Opciones de privacidad de anuncios".

## 8. Casos raros

- [ ] Cambiar la zona horaria o la hora del teléfono: los avisos se reprograman.
- [ ] Empezar una rutina con otra en curso: la anterior termina (lo hecho cuenta) y empieza la nueva.
- [ ] Editar una rutina mientras está en curso: la sesión actual no cambia.
- [ ] Fabricantes con ahorro de batería agresivo (Xiaomi, Samsung, Huawei): anotar si los avisos se retrasan. Ver [dontkillmyapp.com](https://dontkillmyapp.com).

## Cómo reportar

Para cada problema: **qué hiciste → qué esperabas → qué pasó**, modelo de teléfono y versión de Android. Si se cerró sola, el log ayuda: `adb logcat -s Ahorita AndroidRuntime`.
