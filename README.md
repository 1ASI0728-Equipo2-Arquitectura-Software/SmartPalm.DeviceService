# SmartPalm Device Service

Microservicio dueño del registro y conectividad de Edge Gateways y dispositivos IoT. Conserva las rutas del monolito y publica `device.iot-device-registered.v1` para que Crop e Ingestion creen sus proyecciones sin consultar esta base de datos.

## Rol en la plataforma

Device es la autoridad de registro del inventario físico de Edge Gateways y sensores IoT. Durante el flujo administrativo valida el gateway, el usuario propietario, la plantación y el sector, y luego emite la asociación del dispositivo que necesita el resto de la plataforma. Aquí se establece la cadena de propiedad antes de aceptar cualquier lectura.

La respuesta de registro puede exponer el valor sin procesar de CloudKey/bootstrap una sola vez para la preparación física. El backend persiste hashes y metadatos de provisión; la clave operativa del sensor es generada por Edge API durante la provisión dinámica y no es un valor fijo del firmware ni de este servicio.

## Relaciones de integración

- **Identity Service:** valida la cuenta y el plan activo que serán propietarios del gateway/dispositivo.
- **Crop Service:** valida la propiedad y compatibilidad de la plantación y el sector.
- **Edge API y Edge Node:** sincronizan el registry y utilizan el bootstrap de un solo uso para provisionar el sensor.
- **Ingestion Service:** consume el evento de registro y rechaza la telemetría cuyos identificadores de gateway, dispositivo o propiedad no coincidan.
- **API Gateway y clientes:** exponen las rutas administrativas de registro, registry y provisión/revocación.

Device no persiste lecturas de sensores. Las URL públicas de lecturas son rutas de compatibilidad que el Gateway dirige a Ingestion.

## Ejecución local

1. Copiar `.env.example` a `.env`.
2. Usar el mismo `JWT_KEY` de Identity Service.
3. Levantar solo PostgreSQL con `docker compose -f compose.local.yaml --env-file .env up -d`.
4. Ejecutar la API con `dotnet run --project SmartPalm.DeviceService.API`.

Para levantar también RabbitMQ y la API en contenedores se pueden activar los perfiles
`isolated` y `api`. Los puertos se controlan desde `.env` y no se utilizan nombres fijos
de contenedor.

La API queda en `http://localhost:5103`.

## Despliegue con Coolify

Para un despliegue mediante Docker Compose en Coolify, configurar el build pack como
`Dockerfile` y usar `compose.coolify.yaml` como composición opcional. El contenedor
publica el puerto interno `8080` y expone `/health/live` y `/health/ready`.

Registrar las siguientes variables en Coolify como secretos o variables de ejecución;
no colocar contraseñas reales en el repositorio:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
DEVICE_DB_HOST=<ip-privada-vultr-2>
DEVICE_DB_PORT=5432
DEVICE_DB_NAME=smartpalm_device
DEVICE_DB_USERNAME=<usuario-database>
DEVICE_DB_PASSWORD=<password-database>
JWT_ISSUER=smartpalm.identity
JWT_AUDIENCE=smartpalm.clients
JWT_KEY=<misma-clave-de-identity-service-de-al-menos-32-caracteres>
RABBITMQ_ENABLED=true
RABBITMQ_URI=amqp://<usuario>:<password>@<ip-privada-vultr-2>:5672/
RABBITMQ_EXCHANGE=smartpalm.events
```

La aplicación ejecuta las migraciones EF Core al iniciar. Coolify debe configurar
`/health/ready` como health check y no debe publicar el puerto 8080 públicamente.
El API Gateway es el único punto HTTP público.

Ejemplo local de la composición de Coolify:

```powershell
docker compose --env-file .env -f compose.coolify.yaml up -d --build
```

## Verificación

```powershell
dotnet restore SmartPalm.DeviceService.slnx
dotnet build SmartPalm.DeviceService.slnx
dotnet test SmartPalm.DeviceService.slnx
```

