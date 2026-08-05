---
name: unity-6-api-changes
description: Registro vivo de funciones y APIs obsoletas en Unity 6 (6000.5) y sus nuevas alternativas.
---

# 🔄 Cambios de API y Funciones en Unity 6.5

Este documento sirve como regla para Antigravity. Al escribir código en C# para este proyecto, debes revisar esta lista para evitar usar APIs obsoletas (deprecated) de Unity.

Si durante el desarrollo Antigravity o el usuario detectan una advertencia de obsolescencia (`warning CS0618`), se debe investigar la nueva alternativa y documentarla aquí.

## 📝 Registro de Cambios

### Gráficos y Renderizado
- **[DEPRECATED]** Built-In Render Pipeline (BIRP) materials y shaders.
- **[USAR AHORA]** Universal Render Pipeline (URP) materials, Lit shaders, y `UnityEngine.Rendering.Universal`.

### Core y Entidades
- **[DEPRECATED]** Dependencia exclusiva de `InstanceID` para sistemas avanzados de GameObjects.
- **[USAR AHORA]** `EntityID` (Parte de la nueva unificación de entidades en Unity 6.5).

### UI y Canvas
- *Sin cambios mayores registrados aún en 6.5.*

### Inputs (Sistema de Controles)
- **[DEPRECATED]** `Input.GetKeyDown()`, `Input.GetAxis()` (Legacy Input Manager).
- **[USAR AHORA]** El nuevo `UnityEngine.InputSystem` (El proyecto ya tiene configurado el archivo `InputSystem_Actions.inputactions`).

---
*Nota para Antigravity: Agrega nuevos descubrimientos a esta lista usando tus herramientas de edición de archivos cuando encuentres un error de compilación por API obsoleta.*
