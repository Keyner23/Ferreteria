# Ferretería El Tornillo

Tienda en línea para una ferretería: catálogo público, carrito, compras con descuento de
inventario y un panel de administración. API REST en .NET 10 con autenticación JWT y
cliente Blazor WebAssembly hospedado dentro de la propia API.

Proyecto de práctica, escrito para ejercitar arquitectura por capas, autenticación con
tokens y consumo de API desde un frontend.

---

## Arquitectura

Seis proyectos, con las dependencias apuntando **hacia adentro**: `Domain` no referencia
a nadie y `Api` conoce a todos.

```
Ferreteria.Domain          Entidades de negocio. Cero dependencias.
       ↑
Ferreteria.Application     Interfaces de servicios y excepciones.  → Domain, Shared
       ↑
Ferreteria.Infrastructure  EF Core, BCrypt, JWT, implementaciones. → Application
       ↑
Ferreteria.Api             Controladores. Hospeda el Blazor.       → Infrastructure, Web
                                    ↓
Ferreteria.Web             Blazor WebAssembly.                     → Shared
```

`Ferreteria.Shared` contiene los DTOs y lo referencian los dos extremos, así que el
contrato entre la API y el navegador está escrito **una sola vez**: si cambia un DTO y el
front usaba el campo que se quitó, no compila.

### Por qué el Blazor va dentro de la API

`Ferreteria.Api` referencia a `Ferreteria.Web` y lo sirve con `UseBlazorFrameworkFiles()`
más un fallback. El resultado:

- Un solo proceso y un solo puerto
- Sin configuración de CORS
- Sin URL de la API que mantener sincronizada
- Un solo artefacto que desplegar

El fallback distingue las rutas: `/api/...` que ningún controlador reclame devuelve
**404 en JSON**, y cualquier otra ruta va al enrutador de Blazor.

---

## Requisitos

- .NET SDK 10.0
- SQL Server LocalDB (viene con Visual Studio) o cualquier SQL Server
- `dotnet-ef` como herramienta global:

```bash
dotnet tool install --global dotnet-ef
```

---

## Puesta en marcha

### 1. Clonar y restaurar

```bash
git clone https://github.com/Keyner23/Ferreteria.git
```

```bash
cd Ferreteria
```

```bash
dotnet restore
```

### 2. Configurar los secretos

El proyecto usa **user secrets**: la cadena de conexión pública va en `appsettings.json`,
pero el secreto del JWT y las credenciales del administrador nunca se versionan.

Genera un secreto largo (mínimo 32 bytes, lo exige HMAC-SHA256):

```bash
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))
```

Y guárdalo junto con las credenciales del admin:

```bash
dotnet user-secrets set "Jwt:Secret" "<el-valor-generado>" --project src/Ferreteria.Api
```

```bash
dotnet user-secrets set "Admin:Correo" "admin@ferreteria.com" --project src/Ferreteria.Api
```

```bash
dotnet user-secrets set "Admin:Password" "<una-clave>" --project src/Ferreteria.Api
```

Si falta `Jwt:Secret`, la aplicación **no arranca** y dice por qué. Si faltan las
credenciales del admin, arranca igual y lo advierte en el log.

### 3. Crear la base de datos

La aplicación aplica las migraciones al arrancar, así que basta con ejecutarla. Si
prefieres hacerlo aparte:

```bash
dotnet ef database update --project src/Ferreteria.Infrastructure --startup-project src/Ferreteria.Api
```

El `DbSeeder` crea el administrador y seis categorías iniciales la primera vez. Es
idempotente: reiniciar no duplica nada.

### 4. Ejecutar

```bash
dotnet run --project src/Ferreteria.Api
```

| | |
|---|---|
| Tienda | http://localhost:5265 |
| Documentación de la API | http://localhost:5265/swagger |

**Ejecuta solo `Ferreteria.Api`.** El proyecto `Ferreteria.Web` no se levanta por
separado: viaja dentro de la API. Si lo lanzas aparte, verás la interfaz pero las
llamadas a `/api/...` darán 405.

---

## Roles y acceso

| Rol | Qué puede hacer |
|---|---|
| *(sin cuenta)* | Ver el catálogo y armar el carrito |
| **Cliente** | Comprar y consultar su historial |
| **Admin** | CRUD de productos, categorías y clientes; ver todas las ventas |

El administrador se crea desde el seeder. El registro público **siempre** crea clientes:
`RegistroRequest` no tiene campo de rol, así que no hay forma de pedir ser admin.

---

## Endpoints

```
POST   /api/Auth/registro                    público
POST   /api/Auth/login                       público
GET    /api/Auth/yo                          autenticado

GET    /api/Categorias                       público
POST   /api/Categorias                       Admin
PUT    /api/Categorias/{id}                  Admin
DELETE /api/Categorias/{id}                  Admin

GET    /api/Productos[?incluirInactivos]     público (los inactivos, solo Admin)
GET    /api/Productos/{id}                   público
POST   /api/Productos                        Admin
PUT    /api/Productos/{id}                   Admin
DELETE /api/Productos/{id}                   Admin  (desactiva, no borra)

GET    /api/Clientes[?incluirInactivos]      Admin
GET    /api/Clientes/{id}                    Admin
POST   /api/Clientes                         Admin  (cliente de mostrador, sin cuenta)
PUT    /api/Clientes/{id}                    Admin
DELETE /api/Clientes/{id}                    Admin  (desactiva y bloquea el acceso)

POST   /api/Ventas                           autenticado
GET    /api/Ventas/mis-compras               autenticado
GET    /api/Ventas                           Admin
GET    /api/Ventas/{id}                      autenticado (solo las propias; Admin, todas)
```

Los errores se devuelven como **ProblemDetails**: `404` no encontrado, `409` conflicto,
`401` sin credenciales, `403` sin permiso, `400` validación.

---

## Decisiones de diseño

Las que no son obvias al leer el código.

### `Usuario` y `Cliente` son entidades separadas

`Usuario` es quien se autentica; `Cliente` es quien compra. La relación es 1:1 con
`UsuarioId` **nullable**, porque una ferretería tiene clientes de mostrador que el
administrador registra y que nunca inician sesión. Con una sola entidad, esos clientes
tendrían un `PasswordHash` vacío mintiendo sobre lo que es.

Además el administrador es un `Usuario` sin `Cliente`: no tiene dirección de envío
porque no compra.

### La venta guarda una copia del producto

`DetalleVenta` copia `NombreProducto` y `PrecioUnitario` al momento de vender, además de
mantener el `ProductoId`.

Si el monto se calculara leyendo `producto.PrecioUnitario`, subir un precio cambiaría el
total de **todas las ventas históricas**. Una venta es una foto del pasado y se guarda
como tal.

### El cliente nunca manda precios ni identidad

`VentaRequest` solo lleva `ProductoId` y `Cantidad`. Ni precio, ni total, ni `ClienteId`:

- **El precio** lo lee el servidor de la base al registrar la venta
- **El total** lo calcula el servidor sumando lo que él mismo puso
- **El cliente** sale del claim `clienteId` del token firmado

Un usuario que edite la petición para poner un precio de $1 no logra nada: esos campos no
existen en el DTO, así que el deserializador los descarta.

### Borrado lógico en productos y clientes

`DELETE /api/Productos/{id}` desactiva, no borra. Las claves foráneas hacia
`DetalleVenta` y `Venta` usan `Restrict` precisamente para que borrar un producto o un
cliente no pueda llevarse por delante el historial de facturas.

Desactivar un cliente también apaga su `Usuario`, así que deja de poder iniciar sesión.

### La autenticación del cliente es para la interfaz

`JwtAuthenticationStateProvider` **decodifica** el token, no valida la firma — el
navegador no tiene el secreto ni debe tenerlo. Sirve para mostrar u ocultar menús.

Quien decide de verdad es el servidor: los `[Authorize(Roles = "Admin")]` de los
controladores. Un token manipulado haría aparecer el menú de administrador y luego
recibiría 401 en cada llamada.

### El carrito vive en el navegador

Se guarda en `localStorage`, así que sobrevive a recargar y cerrar el navegador, pero no
se comparte entre dispositivos. Los precios que guarda son solo para mostrar: al confirmar
solo viajan producto y cantidad.

---

## Seguridad

Lo que está cubierto:

- Contraseñas con **BCrypt**, factor de trabajo 12 (la sal va dentro del hash)
- Login con el **mismo mensaje** para usuario inexistente y contraseña incorrecta, para
  no permitir enumerar cuentas
- El chequeo de cuenta activa va **después** de validar la contraseña, por lo mismo
- **IDOR** bloqueado: un cliente que pida `/api/Ventas/{id}` de otro recibe 403
- Precios e identidad fuera del alcance del cliente
- Secretos fuera del repositorio (user secrets)
- Índices únicos en `Correo`, `Documento`, `Codigo` y nombre de categoría — la única
  garantía real contra duplicados en condiciones de carrera

Lo que falta para producción:

- **Token de concurrencia** en `Producto`. Dos compras simultáneas de la última unidad
  pueden pasar ambas la validación y dejar el stock negativo. Se resuelve con
  `IsRowVersion()`
- El token va en `localStorage`, vulnerable a XSS. Lo robusto son cookies `HttpOnly`
  con un BFF
- Las migraciones se aplican al arrancar, cómodo en desarrollo y discutible en producción
- Sin registro centralizado de errores ni endpoint de salud

---

## Estructura

```
src/
  Ferreteria.Domain/
    Entities/              Usuario, Rol, Cliente, Categoria, Producto, Venta, DetalleVenta

  Ferreteria.Shared/
    Dtos/Auth/             LoginRequest, LoginResponse, RegistroRequest
    Dtos/Productos/        ProductoDto, ProductoRequest, CategoriaDto, CategoriaRequest
    Dtos/Clientes/         ClienteDto, ClienteRequest
    Dtos/Ventas/           VentaDto, DetalleVentaDto, VentaRequest, ItemVentaRequest

  Ferreteria.Application/
    Interfaces/            IAuthService, IProductoService, IClienteService,
                           IVentaService, ICategoriaService, IPasswordHasher, ITokenService
    Exceptions/            AppException y sus cuatro hijas

  Ferreteria.Infrastructure/
    Data/                  FerreteriaDbContext, DbSeeder
    Auth/                  PasswordHasher (BCrypt), TokenService (JWT), JwtSettings
    Services/              Implementaciones de los cinco servicios
    Migrations/
    DependencyInjection.cs Registro de todo en una línea para la Api

  Ferreteria.Api/
    Controllers/           Auth, Categorias, Productos, Clientes, Ventas
    Errors/                AppExceptionHandler → ProblemDetails
    Program.cs

  Ferreteria.Web/
    Auth/                  TokenStore, JwtAuthenticationStateProvider, AuthorizationHandler
    Carrito/               CarritoService, ItemCarrito
    Services/              AuthApi, ProductoApi, CategoriaApi, ClienteApi, VentaApi
    Components/            TablaVentas
    Pages/                 Home, Catalogo, Carrito, Compra, Login, Registro, MisCompras
    Pages/Admin/           AdminProductos, AdminClientes, AdminVentas
```

---

## Notas técnicas

**`Blazored.LocalStorage`** se instala con versión explícita (`--version 4.5.0`): el
paquete todavía no declara compatibilidad con `net10.0`, aunque funciona.

**Swashbuckle 10** usa Microsoft.OpenApi 2.x, que movió los modelos al namespace raíz.
El `using` correcto es `Microsoft.OpenApi`, no `Microsoft.OpenApi.Models` — casi todo el
material en línea es de .NET 8/9 y usa el antiguo.

**El formato de moneda** se escribe como `$` + `ToString("N0", cultura)` en vez de
`ToString("C0")`. Blazor WebAssembly recorta los datos de globalización, así que el
formato de moneda cae al código ISO (`25.000 COP`).

---

## Despliegue

Al estar el Blazor hospedado, se publica un solo artefacto:

```bash
dotnet publish src/Ferreteria.Api -c Release -o publicar
```

En el servidor, la configuración va por variables de entorno (el doble guion bajo
representa la jerarquía del JSON):

```
ConnectionStrings__SqlServer=Server=...;Database=...;User Id=...;Password=...
Jwt__Secret=...
Admin__Correo=...
Admin__Password=...
```
