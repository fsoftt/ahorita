# Publicar en Google Play

El workflow [`publish-play-store.yml`](../.github/workflows/publish-play-store.yml) corre al empujar a la rama **`release`**:

1. Corre las pruebas.
2. Compila el `.aab` de **`com.fsoftt.ahorita`** (anuncios reales) firmado con el keystore de release.
3. Verifica que **no** quedó firmado con `debug.keystore`.
4. Guarda el `.aab` como artefacto y, si hay cuenta de servicio, lo sube a la pista **internal** de Play Console.

## Secrets (GitHub → Settings → Secrets and variables → Actions)

| Secret | Obligatorio | Qué es |
|---|---|---|
| `PLAY_KEYSTORE_BASE64` | Sí | El keystore en base64: `base64 -w0 ahorita-release.keystore` |
| `PLAY_KEYSTORE_ALIAS` | Sí | Alias de la llave dentro del keystore |
| `PLAY_KEYSTORE_PASSWORD` | Sí | Contraseña del keystore |
| `PLAY_KEY_PASSWORD` | No | Contraseña de la llave, solo si es distinta de la del keystore |
| `PLAY_SERVICE_ACCOUNT_JSON` | No | JSON de la cuenta de servicio de Google Play con acceso a la app. Sin él, el `.aab` queda como artefacto para subirlo a mano. |

> El keystore y sus contraseñas **nunca** se versionan. Guárdalos en un gestor de contraseñas con respaldo: si se pierden, no se puede volver a actualizar la app (salvo con Play App Signing).

Ver el alias de un keystore: `keytool -list -keystore ahorita-release.keystore`.

## Primera publicación

1. Crea la app en Play Console con el paquete `com.fsoftt.ahorita`.
2. Sube el **primer** `.aab` a mano (descárgalo del artefacto del workflow): la API de Play no puede crear la primera versión.
3. Completa ficha, clasificación de contenido, privacidad y la declaración de **anuncios** (la app contiene anuncios).
4. Cuentas nuevas: Google exige una **prueba cerrada con 12+ testers durante 14 días** antes de producción.

## Publicar una versión

```bash
# Sube ApplicationVersion (y ApplicationDisplayVersion) en src/Ahorita.App/Ahorita.App.csproj
git checkout release && git merge --ff-only main && git push origin release
```

Play rechaza un `ApplicationVersion` repetido: súbelo en cada publicación.

## Flavors

| | QA | Google Play |
|---|---|---|
| Paquete | `com.fsoftt.ahorita.qa` | `com.fsoftt.ahorita` |
| Nombre | Ahorita QA | Ahorita |
| Anuncios | Prueba de Google | Reales |
| Firma | `src/Ahorita.App/debug.keystore` | Keystore de release |
| Dónde | CI de cada PR / `main` | Rama `release` |
