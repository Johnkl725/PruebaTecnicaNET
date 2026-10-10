# Prueba técnica — Backend .NET 8

API REST de usuarios con arquitectura hexagonal, Entity Framework Core, SQL Server y autorización JWT por permisos. Solo implementa los tres endpoints solicitados.

## Requisitos

- **SDK de .NET 8**, no solo el runtime. Comprueba `dotnet --list-sdks`. `global.json` admite las bandas 8.0 instaladas.
- Podman con su máquina Linux iniciada y un proveedor Compose instalado (`podman compose version`), o Docker Compose.
- Puerto **14333** disponible para SQL Server y **5080** para la API.
- Acceso a NuGet para restaurar paquetes y al registro de Microsoft para descargar la imagen.

En Windows puedes instalar el SDK con `winget install --id Microsoft.DotNet.SDK.8 --exact`. Abre otra terminal después de instalarlo. Si Podman ya tiene una máquina, iníciala desde Podman Desktop o con `podman machine start`; no es necesario crear otra.

## Base de datos y configuración

| Dato | Valor |
| --- | --- |
| Motor | SQL Server 2022 Developer CU26, **16.0.4265.3** |
| Imagen | `mcr.microsoft.com/mssql/server:2022-latest` fijada al digest indicado en Compose |
| Servidor desde el host | `127.0.0.1,14333` (puerto interno 1433) |
| Base de datos | `UsersDemo` |
| Usuario de desarrollo | `sa` |
| Contraseña de desarrollo | Generada por `scripts/setup-dev.ps1`, guardada como `MSSQL_SA_PASSWORD` en `.env` |

El digest `sha256:ba4c8329f48fb8f02e1416be6a930ebfd71268caee78aa985f3af4315e457c89` fija exactamente la imagen, aunque el tag `2022-latest` cambie. La compilación se comprobó consultando `SERVERPROPERTY('ProductVersion')` y se puede contrastar con las [versiones publicadas por Microsoft](https://learn.microsoft.com/en-us/troubleshoot/sql/releases/sqlserver-2022/build-versions). El Compose usa el cliente `mssql-tools18` incluido en esa imagen para verificar la disponibilidad del servidor.

El script genera la misma contraseña en la cadena `ConnectionStrings:UsersDatabase` y una clave JWT aleatoria de 32 bytes. Los archivos `.env` y `src/Api/appsettings.Local.json` están excluidos de Git. El script conserva cualquier configuración existente y se detiene si encuentra alguno de esos archivos.

La aplicación lee emisor, audiencia y duración desde `appsettings.json`; la clave y la conexión desde la configuración local. Se pueden reemplazar mediante variables de entorno: `ConnectionStrings__UsersDatabase`, `Jwt__SigningKey`, `Jwt__Issuer`, `Jwt__Audience` y `Jwt__DurationMinutes`. `.env` es para Compose; la API no lo lee. La conexión SQL usa IPv4 explícita para evitar que `localhost` resuelva a `::1` mientras Podman publica el puerto en `127.0.0.1`.

## Levantar y ejecutar

Ejecuta desde la raíz del repositorio en PowerShell:

```powershell
# 1. Crear configuración local una sola vez.
powershell -NoProfile -ExecutionPolicy Bypass -File ./scripts/setup-dev.ps1

# 2. Levantar únicamente SQL Server.
podman compose up -d
podman compose ps
podman compose logs sqlserver
```

Espera a que SQL Server esté `healthy` antes de aplicar la migración. En Docker, sustituye `podman compose` por `docker compose`. El puerto 14333 permite conservar otra instancia local que utilice 1433. El volumen mantiene los datos al detener el contenedor; no cambies la contraseña en `.env` esperando que cambie la de un volumen ya inicializado.

```powershell
# 3. Restaurar herramientas y compilar.
dotnet tool restore
dotnet restore Users.sln
dotnet build Users.sln --no-restore

# 4. Crear el esquema mediante la migración versionada.
dotnet ef database update --project src/Infrastructure --startup-project src/Api

# 5. Arrancar la API y cargar los datos semilla de desarrollo.
dotnet run --project src/Api --launch-profile http
```

Swagger: **http://localhost:5080/swagger**. La semilla se ejecuta solo con entorno `Development` y `Seed:Enabled=true`; no crea el esquema. Es transaccional y reconoce usuarios por sus identificadores fijos, sin restablecer los cambios hechos durante la revisión.

## Usuarios de prueba

| Email | Contraseña | Rol | Permisos | Activo |
| --- | --- | --- | --- | --- |
| `admin@demo.com` | `AdminDemo!2026` | Administrador | `users.read`, `users.update` | Sí |
| `consulta@demo.com` | `ConsultaDemo!2026` | Consulta | `users.read` | Sí |
| `inactivo@demo.com` | `InactivoDemo!2026` | Consulta | `users.read` | No |

Estas contraseñas son exclusivamente datos públicos de demostración; la base almacena PBKDF2-SHA256 con 600 000 iteraciones y salt aleatorio individual de 16 bytes. No se almacenan contraseñas en texto plano en las tablas.

## Endpoints y verificación manual

| Método y ruta | Acceso | Respuesta correcta |
| --- | --- | --- |
| `POST /api/v1/auth/token` | Público; body con `email` y `password` | `accessToken`, `tokenType`, `expiresIn` (segundos) |
| `GET /api/v1/users?page=1&pageSize=20` | JWT válido | `items`, `page`, `pageSize`, `totalCount` |
| `PUT /api/v1/users/{id}` | JWT y permiso `users.update` | Usuario actualizado |

El body del PUT contiene `name`, `email` e `isActive`; los tres son obligatorios. Los identificadores son GUID. Nombre: 1–100 caracteres; email: hasta 254; `page >= 1`, `pageSize` entre 1 y 100. Una página posterior al último resultado devuelve una lista vacía. El listado incluye usuarios activos e inactivos y nunca proyecta el hash.

En Swagger ejecuta el login, copia `accessToken` y pégalo en **Authorize**, sin el prefijo `Bearer`. También puedes abrir [`requests/users.http`](requests/users.http) con REST Client de VS Code: sus solicitudes nombradas reutilizan el JWT automáticamente. Ejecuta primero los logins de administrador y consulta. Otros clientes HTTP pueden requerir copiar el token manualmente.

El archivo incluye casos **200, 400, 401, 403, 404 y 409**. También puedes ejecutar `powershell -NoProfile -ExecutionPolicy Bypass -File ./scripts/smoke-test.ps1` con la API y los datos iniciales disponibles. Para comprobar caducidad manualmente, configura `Jwt__DurationMinutes=1` antes de arrancar, solicita un token y repite el GET después de 60 segundos: debe responder 401, sin margen adicional de reloj.

## Arquitectura y decisiones

```text
src/Domain          Entidades User, Role y Permission; invariantes. Sin dependencias externas.
src/Application     Puertos de entrada/salida, DTOs y casos de uso. Solo referencia Domain.
src/Infrastructure  Adaptadores EF Core, SQL Server, PBKDF2 y JWT; migraciones y semilla.
src/Api             Controllers, políticas, errores y composición de dependencias.
tests/Application.Tests  Pruebas unitarias con un repositorio falso, sin base real.
requests/           Solicitudes HTTP para la revisión.
scripts/            Configuración local y verificación HTTP.
```

- **Puertos y adaptadores:** Application define las interfaces de persistencia, hash y token; Infrastructure las implementa. Api solo referencia Infrastructure para registrar dependencias; la semilla se ejecuta mediante un servicio de arranque registrado por Infrastructure. Domain y Application no conocen EF, ASP.NET Core ni JWT.
- **Casos de uso explícitos:** tres servicios pequeños, sin repositorio genérico, mediador ni endpoints adicionales. Las operaciones de E/S propagan `CancellationToken`.
- **Permisos:** relaciones muchos a muchos usuario–rol y rol–permiso. El JWT incluye la unión de permisos sin duplicados. Una política de ASP.NET Core exige el claim `permission=users.update`; los controllers no deciden por rol. GET solo exige JWT, según el enunciado.
- **Email único:** se recorta y normaliza a minúsculas. La consulta previa da un conflicto claro; el índice único y la traducción de errores de persistencia protegen también las actualizaciones simultáneas.
- **Errores centralizados:** validación de entrada con `ApiController`, excepciones traducidas a Problem Details y errores inesperados registrados sin exponer sus detalles al cliente.
- **JWT:** firma HS256 y validación de firma, algoritmo, emisor, audiencia y expiración. Un usuario inactivo no obtiene tokens. Un JWT ya emitido sigue siendo válido hasta caducar aunque cambien permisos o estado; no se añadió revocación ni refresh tokens.

Para una siguiente iteración: pruebas de integración automatizadas contra SQL Server y, si el producto lo requiere, revocación de tokens y control de concurrencia de actualizaciones. El HTTP y la confianza en el certificado de SQL Server de este ejemplo son configuración local; un despliegue real requiere HTTPS y secretos administrados.

## Pruebas y cambios de esquema

```powershell
dotnet test Users.sln
dotnet ef migrations has-pending-model-changes --project src/Infrastructure --startup-project src/Api
```

Las pruebas xUnit cubren actualización correcta, normalización, email propio y ajeno, usuario ausente, entradas inválidas, cancelación y conflicto al persistir. También cubren credenciales, usuario inactivo y paginación. No necesitan SQL Server, secretos ni un contenedor. No se estableció un porcentaje de cobertura artificial.

Para un cambio posterior del modelo:

```powershell
dotnet ef migrations add NombreDelCambio --project src/Infrastructure --startup-project src/Api --output-dir Persistence/Migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

Las migraciones y su snapshot se versionan; no se usa `EnsureCreated` ni scripts SQL manuales para crear el esquema.

## Validación realizada

Comprobado localmente con SDK **8.0.425** y SQL Server de Compose:

- Solución compilada sin advertencias ni errores; **23 pruebas unitarias correctas**.
- Migración aplicada a una base nueva y modelo sin cambios pendientes respecto al snapshot.
- **16 verificaciones HTTP correctas**, incluidos los seis códigos de respuesta solicitados.
- Rechazo de JWT expirado, firma incorrecta, emisor incorrecto y audiencia incorrecta.
- Swagger con Bearer, tres usuarios semilla con hashes PBKDF2 y segundo arranque sin duplicar datos.
