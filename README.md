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

## Verificación

```powershell
dotnet restore SmartPalm.DeviceService.slnx
dotnet build SmartPalm.DeviceService.slnx
dotnet test SmartPalm.DeviceService.slnx
```

