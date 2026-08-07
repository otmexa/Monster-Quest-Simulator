# Checklist Maestro Actualizado — Sistema de Armas Compañeras

## Fase 1: Addressables y entorno TESTED [OK]

* `[x]` Instalar el paquete Addressables.
* `[x]` Convertir el prefab base `sword1h01_blue` a Addressable.
* `[x]` Configurar los grupos iniciales:

  * `Weapon_Core_Local`
  * `Weapon_Icons`
* `[x]` Confirmar que todas las claves usen el formato definitivo:

  * `weapon.visual.000123`
  * `weapon.icon.000123`
* `[x]` Convertir las demás armas de prueba y sus iconos a Addressables.
* `[x]` Agregar un prefab visual placeholder para claves inexistentes.
* `[x]` Agregar un icono placeholder para cargas pendientes o fallidas.

---

## Fase 2: Catálogo lógico JSON TESTED [OK]

* `[x]` Crear `WeaponCatalogResponse`.
* `[x]` Crear `WeaponData`.
* `[x]` Crear `WeaponInstanceData`.
* `[x]` Crear `IWeaponCatalogSource`.
* `[x]` Implementar `LocalWeaponCatalogSource`.
* `[x]` Leer un JSON local dummy sin VPS.
* `[x]` Deserializar el arreglo de armas.
* `[x]` Construir `Dictionary<int, WeaponData>`.
* `[x]` Validar JSON vacío o corrupto.
* `[x]` Validar IDs duplicados.
* `[x]` Validar campos obligatorios.
* `[x]` Implementar `TryGetWeapon(int weaponId, out WeaponData weapon)`.
* `[x]` Probar el catálogo con tres armas locales.
* `[x]` Marcar `WeaponDatabase.cs` como `[Obsolete]`.
* `[/]` Retirar gradualmente las dependencias restantes de `WeaponDatabase.cs`.
* `[ ]` Eliminar definitivamente `WeaponDatabase.cs` cuando ningún sistema lo utilice.
* `[x]` Verificar o implementar `GetAllWeapons()` si todavía lo necesita la UI o el editor.

---

## Fase 3: Datos del jugador e instancias únicas TESTED [OK]

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

* `[x]` Refactorizar el inventario para trabajar con instancias únicas.
* `[x]` Implementar `AddWeapon`.
* `[x]` Generar un GUID por cada arma obtenida.
* `[x]` Implementar `GetWeapon`.
* `[x]` Adaptar los métodos existentes al nuevo esquema.
* `[x]` Confirmar que no se pueda equipar dos veces la misma instancia.
* `[x]` Validar que cada `instanceId` equipado exista en `ownedWeapons`.
* `[x]` Probar equipamiento y desequipamiento por `instanceId`.
* `[x]` Resetear o ignorar partidas antiguas incompatibles.

### `WeaponSelectionUI.cs`

* `[x]` Adaptar la pantalla de selección al catálogo JSON.
* `[x]` Utilizar IDs numéricos.
* `[x]` Crear una `WeaponInstanceData` al seleccionar el arma inicial.
* `[x]` Guardar la nueva instancia mediante `PlayerInventory`.

---

## Fase 4: Gestión centralizada de assets 3D (Implementada - Testing pendiente: prueba de rendimiento muchas invocaciones equipar y desequipar con script en cliente)

### `WeaponAssetService.cs`

* `[x]` Confirmar que existe un solo handle por `visualKey`.
* `[x]` Compartir una misma carga pendiente cuando varias armas soliciten simultáneamente la misma clave.
* `[x]` Destruir individualmente cada GameObject retirado.
* `[x]` Liberar el handle solamente cuando desaparezca la última instancia.
* `[x]` Manejar claves inexistentes mediante el visual placeholder.
* `[x]` Limpiar correctamente cargas fallidas u obsoletas.
* `[x]` Limpiar handles al cambiar escena, cerrar sesión o destruir al jugador.

### `WeaponVisualService.cs`

* `[x]` Crear la máquina de estados:

  * `None`
  * `Loading`
  * `Equipped`
  * `Failed`
* `[x]` Implementar `equipRequestVersion`.
* `[x]` Probar la instanciación de un arma en Play Mode.
* `[x]` Refactorizarlo para utilizar internamente `WeaponAssetService`.
* `[x]` Evitar que ambos servicios carguen el mismo prefab por separado.
* `[x]` Decidir si seguirá usándose para previews o armas normales.
* `[x]` Marcarlo como obsoleto si `WeaponAssetService` y los compañeros reemplazan completamente su función.

---

## Fase 5: Corrección visual de armas (Implementada - Testing pendiente hasta la Fase del Weapon Manager)

### `WeaponVisualConfig.cs`

* `[x]` Crear `WeaponVisualConfig`.
* `[x]` Guardar por arma:

  * posición local
  * rotación local
  * escala
  * orientación de ataque
  * escala de preview
  * rotación de preview
* `[x]` Aplicar la configuración al crear cada arma compañera.
* `[x]` Aplicar la configuración en el preview 3D.
* `[x]` Validar que cada prefab tenga una configuración visual válida.

---

## Fase 6: Inventario virtualizado e iconos (Implementada - Testing pendiente hasta generar todos los íconos de las armas)

### `IconCacheService.cs`

* `[x]` Crear la caché LRU.
* `[x]` Agregar soporte de `CancellationToken`.
* `[x]` Compartir una misma carga cuando varias celdas soliciten el mismo icono.
* `[x]` Evitar que un icono tardío se asigne a una celda reciclada.
* `[x]` Limitar la caché por cantidad de iconos.
* `[x]` Limitar la caché por presupuesto aproximado de memoria.
* `[x]` Liberar solamente entradas sin usuarios activos.
* `[x]` Mostrar placeholder mientras carga.
* `[x]` Mostrar placeholder definitivo si la carga falla.

### `WeaponInventoryUI.cs`

* `[x]` Separar responsabilidades: `WeaponInventoryUI` solo maneja la grilla, `WeaponSlotCell` maneja la celda.
* `[x]` Escuchar cambios en `PlayerData.inventory.ownedWeapons` para refrescar celdas.
* `[x]` Instanciar `WeaponSlotCell` desde un prefab por cada arma poseída.

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
* `[x]` Configurar cámara exclusiva (cullingMask) y enviar a `RenderTexture`.
* `[x]` Cargar y liberar usando `WeaponAssetService`.
* `[x]` Proteger estado con `previewRequestVersion` (invalidar solicitudes anteriores; si una carga obsoleta termina, liberar inmediatamente su instancia).
* `[x]` Mantener modelo viejo activo hasta que el nuevo termine de cargar.
* `[x]` Aplicar `WeaponVisualConfig` (`previewPosition`, `previewRotation`, `previewScale`).
* `[x]` Al cerrar inventario: liberar modelo, desactivar cámara y liberar la `RenderTexture` con `RenderTexture.Release()`.

### `WeaponPreviewInputHandler.cs`

* `[x]` Crear script que implemente `IDragHandler` para detectar arrastre (mouse/dedo).
* `[x]` Comunicarse con `WeaponPreviewService` para rotar el modelo actual.

### Pruebas Faltantes (Fase 7)

* `[x]` `ReleaseWeaponInstance` destruye el clon y reduce el conteo en `WeaponAssetService`. (Comprobado por código)
* `[x]` El modelo anterior permanece visible hasta que el nuevo termine de cargar. (Comprobado por código)
* `[x]` `HidePreview()` puede ejecutarse dos veces sin errores. (Comprobado por código)
* `[x]` Solo destruye la `RenderTexture` si fue creada por código. (Comprobado por código)
* `[x]` Al cerrar durante una carga, el modelo tardío se libera. (Comprobado por código)
* `[x]` Pruebas A → B → C rápidas, clave inexistente y abrir/cerrar repetidamente pasan en el Profiler. (Validado por ti al probar UI)
* `[ ]` Reutilizar el servicio para inventario, tienda, recompensa de huevo y gacha (Queda abierto para aplicarlo en el futuro).
  * tienda
  * recompensa de huevo
  * apertura de gacha

---

## Fase 8: Estadísticas de combate en ejecución

### `WeaponRuntimeStats.cs`

* `[x]` Crear `WeaponRuntimeStats`.
* `[x]` Crear la estructura inicial de cálculo de daño y cooldown.
* `[ ]` Confirmar que combina correctamente:

  * `WeaponData`
  * `WeaponInstanceData`
  * nivel
  * enhancement
  * encantamientos
  * buffs temporales
* `[ ]` Preparar o validar campos para:

  * daño final
  * attack speed
  * cooldown
  * rango
  * velocidad de desplazamiento
  * efectos mágicos
  * efectos pasivos
* `[ ]` Permitir recalcular estadísticas sin volver a cargar el modelo 3D.
* `[ ]` Mantener el estado temporal fuera de `WeaponData`.

---

## Fase 9: Sistema de armas compañeras

### `WeaponOrbitVisualController.cs`

* `[x]` Crear el controlador orbital.
* `[x]` Calcular posiciones base según la cantidad de armas.
* `[ ]` Reutilizar o portar completamente el algoritmo orbital anterior.
* `[ ]` Probar formaciones de 1 a 15 armas.
* `[ ]` Recalcular la formación al equipar o desequipar.
* `[ ]` Asignar un offset orbital a cada agente.
* `[ ]` Reacomodar suavemente las armas existentes.
* `[ ]` Confirmar que no calcula daño.
* `[ ]` Confirmar que no selecciona objetivos.
* `[ ]` Confirmar que no carga Addressables directamente.

### `WeaponCompanionAgent.cs`

* `[x]` Crear un agente independiente por arma.
* `[x]` Implementar la máquina de estados inicial.
* `[ ]` Confirmar los estados:

  * `Orbiting`
  * `MovingToTarget`
  * `Attacking`
  * `Returning`
* `[ ]` Mantener:

  * `instanceId`
  * `weaponId`
  * `WeaponRuntimeStats`
  * target actual
  * visual propio
  * posición orbital asignada
* `[ ]` Moverse desde la órbita hasta el objetivo.
* `[ ]` Atacar según su propio attack speed y cooldown.
* `[ ]` Aplicar daño individualmente.
* `[ ]` Preparar efectos mágicos y pasivas.
* `[ ]` Regresar a su posición orbital.
* `[ ]` Manejar que el objetivo muera durante el movimiento.
* `[ ]` Manejar desequipado durante el ataque.
* `[ ]` Liberar correctamente su visual al destruirse.

### `WeaponCompanionManager.cs`

* `[x]` Crear el manager.
* `[x]` Mantener la lista de agentes.
* `[x]` Recibir targets y órdenes de ataque.
* `[x]` Propagar la orden a las armas.
* `[ ]` Confirmar que todas reciben el mismo target.
* `[ ]` Crear un agente por cada instancia equipada.
* `[ ]` Retirar el agente correspondiente al desequipar.
* `[ ]` Cancelar el ataque cuando:

  * muere el objetivo
  * desaparece el objetivo
  * se cancela la orden
  * queda fuera del rango permitido
* `[ ]` Ordenar a todas las armas que regresen a la órbita.
* `[ ]` Actualizar la formación cuando cambia el equipo.

### `PlayerCombat.cs`

* `[x]` Limpiar la implementación anterior.
* `[x]` Delegar la lógica al `WeaponCompanionManager`.
* `[ ]` Confirmar que ya no usa `Resources.Load`.
* `[ ]` Confirmar que ya no carga Addressables directamente.
* `[ ]` Confirmar que ya no mueve las armas orbitales directamente.
* `[ ]` Confirmar que ya no suma todo el daño como un único golpe.
* `[ ]` Mantener únicamente selección de target y despacho de órdenes.
* `[ ]` Eliminar código legado después de validar el nuevo combate.

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

* `[ ]` Revisar tiendas, cofres, recompensas y drops que todavía entreguen IDs antiguos.
* `[ ]` Migrar cualquier sistema que siga usando `WeaponDatabase`.
* `[ ]` Migrar cualquier sistema que guarde armas mediante nombres o IDs de texto.

---

## Fase 11: Herramientas de Editor y validación

### `WeaponDatabaseEditor.cs`

* `[ ]` Actualizarlo para utilizar el catálogo JSON.
* `[ ]` O retirarlo si ya no aporta valor.
* `[ ]` Preparar exportación de datos si se mantiene una herramienta visual.

### `WeaponPreFlightCheck.cs`

* `[ ]` Crear la herramienta de validación.
* `[ ]` Validar:

  * IDs únicos
  * campos obligatorios
  * `visualKey`
  * `iconKey`
  * prefab existente
  * icono configurado como Sprite
  * `WeaponVisualConfig`
  * rareza
  * tipo de arma
* `[ ]` Bloquear exportación o publicación si hay errores.

---

## Fase 12: Pruebas del sistema

### Catálogo e inventario

* `[ ]` Crear dos copias del mismo `weaponId` con GUID diferentes.
* `[ ]` Guardar y cargar correctamente ambas instancias.
* `[ ]` Confirmar que una misma instancia no pueda equiparse dos veces.
* `[ ]` Confirmar que IDs inexistentes no rompan el juego.
* `[ ]` Probar el reseteo de partidas antiguas.

### Assets y memoria

* `[ ]` Varias armas con el mismo `visualKey` comparten un solo handle.
* `[ ]` Cada arma conserva una instancia visual independiente.
* `[ ]` El handle se libera al retirar la última instancia.
* `[ ]` Una clave inexistente muestra placeholder.
* `[ ]` Cambiar de escena no deja handles activos.
* `[ ]` Una solicitud obsoleta no crea un visual tardío.
* `[ ]` Repetir equipamiento y desequipamiento sin crecimiento progresivo de memoria.

### UI

* `[ ]` Crear `TestInventoryUI.cs`.
* `[ ]` Probar scroll rápido por 500 entradas.
* `[ ]` Confirmar que la memoria permanece estable.
* `[ ]` Confirmar que dos celdas comparten la misma carga.
* `[ ]` Confirmar que una celda reciclada no recibe un icono anterior.
* `[ ]` Cerrar el inventario durante una carga.
* `[ ]` Confirmar placeholders de carga y error.

### Compañeros

* `[ ]` Probar formaciones de 1 a 15 armas.
* `[ ]` Equipar tres copias de la misma espada.
* `[ ]` Confirmar que todas reciben el mismo target.
* `[ ]` Confirmar que cada arma ataca con su propio daño.
* `[ ]` Confirmar que cada arma utiliza su propio attack speed.
* `[ ]` Confirmar que el daño no se aplica como un único golpe global.
* `[ ]` Matar el objetivo y comprobar que todas regresan.
* `[ ]` Cancelar el target y comprobar el regreso.
* `[ ]` Desequipar una arma mientras está atacando.
* `[ ]` Equipar una arma durante el combate.
* `[ ]` Cambiar la cantidad de armas y recalcular la formación.
* `[ ]` Confirmar que no hay fugas de agentes, modelos ni handles.

---

## Fase 13: VPS y LiveOps — Pausada

No se implementará todavía, pero la arquitectura queda preparada.

* `[x]` Crear `IWeaponCatalogSource` para desacoplar el origen de los datos.
* `[x]` Mantener `WeaponCatalogService` independiente del JSON local.
* `[x]` Utilizar IDs estables y datos serializables.
* `[x]` Separar datos, combate y visuales.
* `[ ]` Crear `HttpWeaponCatalogSource` cuando exista la API.
* `[ ]` Crear `ContentVersionService`.
* `[ ]` Crear manifiesto de compatibilidad.
* `[ ]` Mover la generación y validación de `instanceId` al servidor.
* `[ ]` Validar daño, cooldown, target y recompensas en el VPS.
* `[ ]` Configurar Addressables remotos.
* `[ ]` Implementar publicación y rollback.

Por ahora:

```text
LocalWeaponCatalogSource
→ simula la futura respuesta del VPS
```
