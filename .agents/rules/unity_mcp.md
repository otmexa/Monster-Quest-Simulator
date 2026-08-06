# Unity MCP Tools (Automatización Directa)

**INSTRUCCIÓN PRINCIPAL:** 
**SIEMPRE** que necesites hacer algo en el Editor de Unity (crear objetos, moverlos, asignar materiales, ejecutar menús, cargar escenas, etc.), **DEBES** usar las herramientas MCP disponibles en lugar de pedirle al usuario que haga clics manualmente. ¡Hazlo directo!

## MCP Server Tools Disponibles

*   **execute_menu_item**: Ejecuta funciones del menú de Unity (etiquetadas con `MenuItem`). Ej: `"GameObject/Create Empty"`.
*   **select_gameobject**: Selecciona objetos en la jerarquía por ruta o ID.
*   **update_gameobject**: Actualiza propiedades core (nombre, tag, layer, active/static) o crea un objeto si no existe.
*   **update_component**: Actualiza campos de un componente o lo añade si el objeto no lo tiene.
*   **add_package**: Instala nuevos paquetes vía Unity Package Manager.
*   **run_tests**: Corre tests usando Unity Test Runner.
*   **send_console_log**: Envía un log a la consola de Unity.
*   **add_asset_to_scene**: Añade un asset (como un Prefab) a la escena actual.
*   **create_prefab**: Crea un prefab a partir de un script con valores serializados opcionales.
*   **create_scene / load_scene / delete_scene / save_scene / get_scene_info / unload_scene**: Administración completa de escenas.
*   **get_gameobject**: Obtiene info detallada de un GameObject y todos sus componentes.
*   **get_console_logs**: Obtiene logs de la consola con soporte de paginación.
*   **recompile_scripts**: Recompila todos los scripts del proyecto.
*   **duplicate_gameobject / delete_gameobject / reparent_gameobject**: Administración de jerarquía y ciclo de vida de GameObjects.
*   **move_gameobject / rotate_gameobject / scale_gameobject / set_transform**: Herramientas para manipular la transformación espacial (posición, rotación, escala) en local o world space.
*   **create_material / assign_material / modify_material / get_material_info**: Administración completa de materiales, shaders y colores.
*   **batch_execute**: Ejecuta múltiples de estas operaciones en una sola petición atómica.

## MCP App Tools (Dashboard y Play Mode)
*   **show_unity_dashboard**: Abre la app MCP de Unity en VS Code.
*   **get_play_mode_status**: Obtiene si Unity está en Play o Pausa.
*   **set_play_mode_status**: Controla el Play Mode (start, pause, stop, step).

## MCP Resources & Prompts
*   **unity://menu-items**, **unity://scenes-hierarchy**, **unity://gameobject/{id}**, **unity://logs**, **unity://packages**, **unity://assets**, **unity://tests/{testMode}**: Recursos para consultar información vital del estado de Unity.
*   Utiliza los prompts integrados como **unity_dashboard** o **gameobject_handling_strategy** para guiar flujos de trabajo complejos.

**RECUERDA:** Antes de decirle al usuario "Ve y haz clic en...", verifica si puedes hacerlo tú mismo con uno de estos comandos.
