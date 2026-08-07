# Checklist Maestro Actualizado â€” Sistema de Armas CompaÃ±eras

## Fase 1: Addressables y entorno TESTED [OK]

* `[x]` Instalar el paquete Addressables.
* `[x]` Convertir el prefab base `sword1h01_blue` a Addressable.
* `[x]` Configurar los grupos iniciales:

  * `Weapon_Core_Local`
  * `Weapon_Icons`
* `[x]` Confirmar que todas las claves usen el formato definitivo:

  * `weapon.visual.000123`
  * `weapon.icon.000123`
* `[x]` Convertir las demÃ¡s armas de prueba y sus iconos a Addressables.
* `[x]` Agregar un prefab visual placeholder para claves inexistentes.
* `[x]` Agregar un icono placeholder para cargas pendientes o fallidas.

---

## Fase 2: CatÃ¡logo lÃ³gico JSON TESTED [OK]

* `[x]` Crear `WeaponCatalogResponse`.
* `[x]` Crear `WeaponData`.
* `[x]` Crear `WeaponInstanceData`.
* `[x]` Crear `IWeaponCatalogSource`.
* `[x]` Implementar `LocalWeaponCatalogSource`.
* `[x]` Leer un JSON local dummy sin VPS.
* `[x]` Deserializar el arreglo de armas.
* `[x]` Construir `Dictionary<int, WeaponData>`.
* `[x]` Validar JSON vacÃ­o o corrupto.
* `[x]` Validar IDs duplicados.
* `[x]` Validar campos obligatorios.
* `[x]` Implementar `TryGetWeapon(int weaponId, out WeaponData weapon)`.
* `[x]` Probar el catÃ¡logo con tres armas locales.
* `[x]` Marcar `WeaponDatabase.cs` como `[Obsolete]`.
* `[/]` Retirar gradualmente las dependencias restantes de `WeaponDatabase.cs`.
* `[ ]` Eliminar definitivamente `WeaponDatabase.cs` cuando ningÃºn sistema lo utilice.
* `[x]` Verificar o implementar `GetAllWeapons()` si todavÃ­a lo necesita la UI o el editor.

---

## Fase 3: Datos del jugador e instancias Ãºnicas TESTED [OK]

### `PlayerInventoryData.cs`

* `[x]` Cambiar el inventario a:

```csharp
List<WeaponInstanceData> ownedWeapons;
```

* `[x]` Cambiar las armas equipadas a:

```csharp
List<EquippedWeaponSlotData> equippedWeapons;
```

* `[x]` Crear `EquippedWeaponSlotData` con:

  * `slotIndex`
  * `instanceId`

### `PlayerInventory.cs`

* `[x]` Refactorizar el inventario para trabajar con instancias Ãºnicas.
* `[x]` Implementar `AddWeapon`.
* `[x]` Generar un GUID por cada arma obtenida.
* `[x]` Implementar `GetWeapon`.
* `[x]` Adaptar los mÃ©todos existentes al nuevo esquema.
* `[x]` Confirmar que no se pueda equipar dos veces la misma instancia.
* `[x]` Validar que cada `instanceId` equipado exista en `ownedWeapons`.
* `[x]` Probar equipamiento y desequipamiento por `instanceId`.
* `[x]` Resetear o ignorar partidas antiguas incompatibles.

### `WeaponSelectionUI.cs`

* `[x]` Adaptar la pantalla de selecciÃ³n al catÃ¡logo JSON.
* `[x]` Utilizar IDs numÃ©ricos.
* `[x]` Crear una `WeaponInstanceData` al seleccionar el arma inicial.
* `[x]` Guardar la nueva instancia mediante `PlayerInventory`.

---

## Fase 4: GestiÃ³n centralizada de assets 3D (Implementada - Testing pendiente: prueba de rendimiento muchas invocaciones equipar y desequipar con script en cliente)

### `WeaponAssetService.cs`

* `[x]` Confirmar que existe un solo handle por `visualKey`.
* `[x]` Compartir una misma carga pendiente cuando varias armas soliciten simultÃ¡neamente la misma clave.
* `[x]` Destruir individualmente cada GameObject retirado.
* `[x]` Liberar el handle solamente cuando desaparezca la Ãºltima instancia.
* `[x]` Manejar claves inexistentes mediante el visual placeholder.
* `[x]` Limpiar correctamente cargas fallidas u obsoletas.
* `[x]` Limpiar handles al cambiar escena, cerrar sesiÃ³n o destruir al jugador.

### `WeaponVisualService.cs`

* `[x]` Crear la mÃ¡quina de estados:

  * `None`
  * `Loading`
  * `Equipped`
  * `Failed`
* `[x]` Implementar `equipRequestVersion`.
* `[x]` Probar la instanciaciÃ³n de un arma en Play Mode.
* `[x]` Refactorizarlo para utilizar internamente `WeaponAssetService`.
* `[x]` Evitar que ambos servicios carguen el mismo prefab por separado.
* `[x]` Decidir si seguirÃ¡ usÃ¡ndose para previews o armas normales.
* `[x]` Marcarlo como obsoleto si `WeaponAssetService` y los compaÃ±eros reemplazan completamente su funciÃ³n.

---

## Fase 5: CorrecciÃ³n visual de armas (Implementada - Testing pendiente hasta la Fase del Weapon Manager)

### `WeaponVisualConfig.cs`

* `[x]` Crear `WeaponVisualConfig`.
* `[x]` Guardar por arma:

  * posiciÃ³n local
  * rotaciÃ³n local
  * escala
  * orientaciÃ³n de ataque
  * escala de preview
  * rotaciÃ³n de preview
* `[x]` Aplicar la configuraciÃ³n al crear cada arma compaÃ±era.
* `[x]` Aplicar la configuraciÃ³n en el preview 3D.
* `[x]` Validar que cada prefab tenga una configuraciÃ³n visual vÃ¡lida.

---

## Fase 6: Inventario virtualizado e iconos (Implementada - Testing pendiente hasta generar todos los Ã­conos de las armas)

### `IconCacheService.cs`

* `[x]` Crear la cachÃ© LRU.
* `[x]` Agregar soporte de `CancellationToken`.
* `[x]` Compartir una misma carga cuando varias celdas soliciten el mismo icono.
* `[x]` Evitar que un icono tardÃ­o se asigne a una celda reciclada.
* `[x]` Limitar la cachÃ© por cantidad de iconos.
* `[x]` Limitar la cachÃ© por presupuesto aproximado de memoria.
* `[x]` Liberar solamente entradas sin usuarios activos.
* `[x]` Mostrar placeholder mientras carga.
* `[x]` Mostrar placeholder definitivo si la carga falla.

### `WeaponInventoryUI.cs`

* `[x]` Separar responsabilidades: `WeaponInventoryUI` solo maneja la grilla, `WeaponSlotCell` maneja la celda.
* `[x]` Escuchar cambios en `PlayerData.inventory.ownedWeapons` para refrescar celdas.
* `[x]` Instanciar `WeaponSlotCell` desde un prefab por cada arma poseÃ­da.

### `WeaponSlotCell.cs`

* `[x]` Solicitar el icono mediante `IconCacheService.Instance.GetIconAsync`.
* `[x]` Cancelar la carga si la celda se recicla (usando `CancellationTokenSource`).
* `[x]` Manejar clic para emitir evento `OnWeaponSelected(instanceId)`.
* `[x]` Mostrar nivel, mejora y datos propios de cada instancia.
* `[x]` Mostrar placeholders correctamente.
* `[x]` Retirar o adaptar cualquier clase temporal como `MockWeapon`.

---

## Fase 7: Preview 3D del Arma Seleccionada TESTED [OK]

### `WeaponPreviewService.cs`

* `[x]` Crear servicio con un `PreviewRoot` en la capa `WeaponPreview`.
* `[x]` Configurar cÃ¡mara exclusiva (cullingMask) y enviar a `RenderTexture`.
* `[x]` Cargar y liberar usando `WeaponAssetService`.
* `[x]` Proteger estado con `previewRequestVersion` (invalidar solicitudes anteriores; si una carga obsoleta termina, liberar inmediatamente su instancia).
* `[x]` Mantener modelo viejo activo hasta que el nuevo termine de cargar.
* `[x]` Aplicar `WeaponVisualConfig` (`previewPosition`, `previewRotation`, `previewScale`).
* `[x]` Al cerrar inventario: liberar modelo, desactivar cÃ¡mara y liberar la `RenderTexture` con `RenderTexture.Release()`.

### `WeaponPreviewInputHandler.cs`

* `[x]` Crear script que implemente `IDragHandler` para detectar arrastre (mouse/dedo).
* `[x]` Comunicarse con `WeaponPreviewService` para rotar el modelo actual.

### Pruebas Faltantes (Fase 7)

* `[x]` `ReleaseWeaponInstance` destruye el clon y reduce el conteo en `WeaponAssetService`. (Comprobado por cÃ³digo)
* `[x]` El modelo anterior permanece visible hasta que el nuevo termine de cargar. (Comprobado por cÃ³digo)
* `[x]` `HidePreview()` puede ejecutarse dos veces sin errores. (Comprobado por cÃ³digo)
* `[x]` Solo destruye la `RenderTexture` si fue creada por cÃ³digo. (Comprobado por cÃ³digo)
* `[x]` Al cerrar durante una carga, el modelo tardÃ­o se libera. (Comprobado por cÃ³digo)
* `[x]` Pruebas A â†’ B â†’ C rÃ¡pidas, clave inexistente y abrir/cerrar repetidamente pasan en el Profiler. (Validado por ti al probar UI)
* `[ ]` Reutilizar el servicio para inventario, tienda, recompensa de huevo y gacha (Queda abierto para aplicarlo en el futuro).
  * tienda
  * recompensa de huevo
  * apertura de gacha

---

## Fase 8: Arquitectura y Sincronización Visual de Compañeras (Tested OK)

### `WeaponOrbitVisualController.cs`
* `[x]` Crear el controlador orbital.
* `[x]` Calcular posiciones base según la cantidad de armas.
* `[x]` Reutilizar o portar completamente el algoritmo orbital anterior.
* `[x]` Probar formaciones de 1 a 15 armas.
* `[x]` Recalcular la formación al equipar o desequipar.
* `[x]` Asignar un offset orbital a cada agente.
* `[x]` Reacomodar suavemente las armas existentes.
* `[x]` Confirmar que no calcula daño.
* `[x]` Confirmar que no selecciona objetivos.
* `[x]` Confirmar que no carga Addressables directamente.

### `WeaponCompanionManager.cs`
* `[x]` Crear el manager.
* `[x]` Mantener la lista de agentes.
* `[x]` Crear un agente por cada instancia equipada.
* `[x]` Retirar el agente correspondiente al desequipar.
* `[x]` Actualizar la formación cuando cambia el equipo.

### `PlayerCombat.cs`
* `[x]` Limpiar la implementación anterior.
* `[x]` Confirmar que ya no usa `Resources.Load`.
* `[x]` Confirmar que ya no carga Addressables directamente.
* `[x]` Confirmar que ya no mueve las armas orbitales directamente.
* `[x]` Eliminar código legado después de validar el nuevo combate.

### `WeaponCompanionAgent.cs` (Base)
* `[x]` Crear un agente independiente por arma.
* `[x]` Liberar correctamente su visual al destruirse.
* `[x]` Mantener: `instanceId`, `weaponId`, visual propio, posición orbital asignada.

---

## Fase 9: Lógica de Combate y Estadísticas de Ejecución

### `WeaponRuntimeStats.cs`
* `[x]` Crear la estructura completa de cálculo de daño y cooldown.
* `[x]` Confirmar que combina correctamente: Nivel, Enhancement, Buffs Temporales.
* `[x]` Preparar campos para: Daño final, Attack speed, Cooldown, Rango, Velocidad de desplazamiento.
* `[x]` Permitir recalcular estadísticas en caliente sin volver a cargar el modelo 3D.

### `WeaponCompanionManager.cs` (Combate)
* `[x]` Recibir targets y órdenes de ataque.
* `[x]` Propagar la orden a las armas.
* `[x]` Confirmar que todas reciben el mismo target.
* `[x]` Cancelar el ataque cuando: muere el objetivo, desaparece el objetivo, se cancela la orden, queda fuera del rango permitido.
* `[x]` Ordenar a todas las armas que regresen a la órbita.

### `PlayerCombat.cs` (Combate)
* `[x]` Delegar la lógica al `WeaponCompanionManager`.
* `[x]` Confirmar que ya no suma todo el daño como un único golpe.
* `[x]` Mantener únicamente selección de target y despacho de órdenes.

### `WeaponCompanionAgent.cs` (Combate Real)
* `[x]` Implementar la máquina de estados inicial y confirmarlos (`Orbiting`, `MovingToTarget`, `Attacking`, `Returning`).
* `[x]` Mantener: `WeaponRuntimeStats`, target actual.
* `[x]` Moverse desde la órbita hasta el objetivo.
* `[x]` Atacar según su propio attack speed y cooldown.
* `[x]` Aplicar daño individualmente a la entidad Enemy.
* `[x]` Preparar efectos mágicos y pasivas.
* `[x]` Regresar a su posición orbital.
* `[x]` Manejar que el objetivo muera durante el movimiento.
* `[x]` Manejar desequipado durante el ataque.

---

## Fase 10: Integraciones de gameplay

### Gacha y recompensas

* `[ ]` Refactorizar `GachaPod.cs`.
* `[ ]` Refactorizar `EggHatchingSystem.cs`.
* `[ ]` Obtener recompensas desde `WeaponCatalogService`.
* `[ ]` Crear una nueva `WeaponInstanceData` al entregar un arma.
* `[ ]` Guardar la instancia mediante `PlayerInventory.AddWeapon`.
* `[ ]` Mostrar la recompensa mediante `WeaponPreviewService`.
* `[ ]` Soportar varias copias del mismo `weaponId`.

### Otras integraciones

* `[ ]` Revisar tiendas, cofres, recompensas y drops que todavÃ­a entreguen IDs antiguos.
* `[ ]` Migrar cualquier sistema que siga usando `WeaponDatabase`.
* `[ ]` Migrar cualquier sistema que guarde armas mediante nombres o IDs de texto.

---

## Fase 11: Herramientas de Editor y validaciÃ³n

### `WeaponDatabaseEditor.cs`

* `[ ]` Actualizarlo para utilizar el catÃ¡logo JSON.
* `[ ]` O retirarlo si ya no aporta valor.
* `[ ]` Preparar exportaciÃ³n de datos si se mantiene una herramienta visual.

### `WeaponPreFlightCheck.cs`

* `[ ]` Crear la herramienta de validaciÃ³n.
* `[ ]` Validar:

  * IDs Ãºnicos
  * campos obligatorios
  * `visualKey`
  * `iconKey`
  * prefab existente
  * icono configurado como Sprite
  * `WeaponVisualConfig`
  * rareza
  * tipo de arma
* `[ ]` Bloquear exportaciÃ³n o publicaciÃ³n si hay errores.

---

## Fase 12: Pruebas del sistema

### CatÃ¡logo e inventario

* `[ ]` Crear dos copias del mismo `weaponId` con GUID diferentes.
* `[ ]` Guardar y cargar correctamente ambas instancias.
* `[ ]` Confirmar que una misma instancia no pueda equiparse dos veces.
* `[ ]` Confirmar que IDs inexistentes no rompan el juego.
* `[ ]` Probar el reseteo de partidas antiguas.

### Assets y memoria

* `[ ]` Varias armas con el mismo `visualKey` comparten un solo handle.
* `[ ]` Cada arma conserva una instancia visual independiente.
* `[ ]` El handle se libera al retirar la Ãºltima instancia.
* `[ ]` Una clave inexistente muestra placeholder.
* `[ ]` Cambiar de escena no deja handles activos.
* `[ ]` Una solicitud obsoleta no crea un visual tardÃ­o.
* `[ ]` Repetir equipamiento y desequipamiento sin crecimiento progresivo de memoria.

### UI

* `[ ]` Crear `TestInventoryUI.cs`.
* `[ ]` Probar scroll rÃ¡pido por 500 entradas.
* `[ ]` Confirmar que la memoria permanece estable.
* `[ ]` Confirmar que dos celdas comparten la misma carga.
* `[ ]` Confirmar que una celda reciclada no recibe un icono anterior.
* `[ ]` Cerrar el inventario durante una carga.
* `[ ]` Confirmar placeholders de carga y error.

### CompaÃ±eros

* `[ ]` Probar formaciones de 1 a 15 armas.
* `[ ]` Equipar tres copias de la misma espada.
* `[x]` Confirmar que todas reciben el mismo target.
* `[ ]` Confirmar que cada arma ataca con su propio daÃ±o.
* `[ ]` Confirmar que cada arma utiliza su propio attack speed.
* `[ ]` Confirmar que el daÃ±o no se aplica como un Ãºnico golpe global.
* `[ ]` Matar el objetivo y comprobar que todas regresan.
* `[ ]` Cancelar el target y comprobar el regreso.
* `[ ]` Desequipar una arma mientras estÃ¡ atacando.
* `[ ]` Equipar una arma durante el combate.
* `[ ]` Cambiar la cantidad de armas y recalcular la formaciÃ³n.
* `[ ]` Confirmar que no hay fugas de agentes, modelos ni handles.

---

## Fase 13: VPS y LiveOps â€” Pausada

No se implementarÃ¡ todavÃ­a, pero la arquitectura queda preparada.

* `[x]` Crear `IWeaponCatalogSource` para desacoplar el origen de los datos.
* `[x]` Mantener `WeaponCatalogService` independiente del JSON local.
* `[x]` Utilizar IDs estables y datos serializables.
* `[x]` Separar datos, combate y visuales.
* `[ ]` Crear `HttpWeaponCatalogSource` cuando exista la API.
* `[ ]` Crear `ContentVersionService`.
* `[ ]` Crear manifiesto de compatibilidad.
* `[ ]` Mover la generaciÃ³n y validaciÃ³n de `instanceId` al servidor.
* `[ ]` Validar daÃ±o, cooldown, target y recompensas en el VPS.
* `[ ]` Configurar Addressables remotos.
* `[ ]` Implementar publicaciÃ³n y rollback.

Por ahora:

```text
LocalWeaponCatalogSource
â†’ simula la futura respuesta del VPS
```





