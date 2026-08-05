---
name: architecture-and-design-rules
description: Reglas fundamentales de arquitectura, UI y diseño de juego para Monster Quest Simulator.
---

# 🏗️ Reglas de Arquitectura y Diseño: Monster Quest Simulator

## 0. Práctica de Desarrollo Obligatoria 🚨
* **Regla:** JAMÁS dar instrucciones manuales al usuario para que mueva cosas en el Editor de Unity.
* **Práctica:** Todo debe ser automatizado por el agente (usando scripts `[InitializeOnLoad]` o MCP tools). El usuario no debe dar un solo clic en la interfaz para arreglar bugs.
* **Regla:** JAMÁS adivinar la sintaxis de una API nueva o de paquetes que cambian constantemente (como Unity 6 o Netcode for GameObjects 2+).
* **Práctica:** Al implementar una librería nueva o corregir un bug de sintaxis, siempre se debe consultar la documentación oficial (usar búsqueda web enfocada a "Documentation") en lugar de foros o memoria de versiones viejas.

Este documento contiene los pilares de diseño que Antigravity DEBE respetar en todo momento al escribir código, diseñar interfaces o estructurar la lógica del juego.

## 1. UI 100% Responsiva 📱
* **Regla:** Está PROHIBIDO usar coordenadas absolutas fijas sin anclajes (Anchors). 
* **Práctica:** Toda la UI (Inventarios, Barras de Vida, Botones de Habilidades) debe estar construida usando `RectTransform` con los Anchors correctos (ej. estirar a los bordes, anclar a las esquinas), Canvas Scaler configurado en `Scale With Screen Size` (usualmente basándose en 1920x1080) y Layout Groups (`HorizontalLayoutGroup`, `VerticalLayoutGroup`) para asegurar que se vea perfecto en cualquier pantalla de celular o monitor de PC.

## 2. Sistemas Estrictamente Modulares 🧩
* **Regla:** El código no puede estar "hardcodeado" para un solo nivel. 
* **Práctica:** Como el juego apunta a niveles casi infinitos (Isla 1, Isla 2, etc.), usaremos la arquitectura de `ScriptableObjects` para definir las estadísticas de los monstruos, los tipos de islas y los items del inventario. Los scripts (ej. `IslandManager`, `EnemySpawner`) deben ser agnósticos al nivel actual y solo leer configuraciones externas.

## 3. Generación Procedural Controlada 🏝️
* **Regla:** El terreno procedural no se genera deformando vértices a lo loco.
* **Práctica:** Se usarán EXCLUSIVAMENTE Prefabs base previamente aprobados por el usuario. El código procedural simplemente seleccionará un Prefab de Isla base de una lista, lo instanciará, y poblará sus "Socket points" (puntos de anclaje predefinidos) con decoraciones (bambú, cofres) y enemigos de forma aleatoria para mantener el control artístico.

## 4. Filosofía de "Avance Rápido y Dopamina" ⚡
* **Regla:** Las mecánicas deben sentirse fluidas y recompensantes al instante.
* **Práctica:** 
    * Recolección automática rápida o magnética de drops (monedas/energía vuelan hacia el jugador).
    * Efectos de partículas (VFX) jugosos y números de daño pop-up en la UI tras cada golpe.
## 5. Herramientas de Editor Propias (Custom Editor) 🛠️
* **Regla:** El contenido repetitivo no se arma a mano, se genera.
* **Práctica:** Crearemos una ventana de Editor personalizada en Unity (`EditorWindow`) donde el usuario pueda simplemente asignar el prefab base de la isla, la lista de monstruos, el boss, y las decoraciones. Al darle a "Generar", el script armará la isla automáticamente para poder producir miles de islas sin esfuerzo manual.

## 6. Optimización por Chunking y Culling (Mundo Abierto Masivo) 🚪
* **Regla:** Solo existe lo que el jugador puede ver o alcanzar.
* **Práctica:** En lugar de crear muchas escenas separadas, podemos tener 100+ islas en una sola escena. Para mantener el rendimiento perfecto en móviles, implementaremos un sistema de **Distance Culling**. Solo las islas que estén dentro de un radio específico alrededor del jugador estarán activas (`SetActive(true)`). A medida que el jugador avance por los puentes o portales, las islas lejanas se apagarán automáticamente y las nuevas se encenderán, liberando memoria y procesamiento instantáneamente.

## 7. Economía de la Dopamina (Upgrades) 🧲
* **Práctica:** La recolección magnética es nativa para generar dopamina, pero el **Rango de Magnetismo** será una estadística escalable (comprable/mejorable por el jugador) para dar sensación de poder y progresión.

## 8. Arquitectura Multijugador (Servidor Dedicado) 🌐
* **Regla:** Nunca confíes en el cliente (Autoridad del Servidor).
* **Práctica:** Usamos **Unity Netcode for GameObjects (NGO) v2.0+**. Toda la lógica que afecte la economía (oro, drops, inventario) o la salud debe ejecutarse exclusivamente en el Servidor (`IsServer`). El cliente solo recibe confirmaciones visuales (ClientRpc) y envía peticiones. Los servidores se diseñarán para correr en modalidad *Headless* (sin gráficos) para poder alojarse en un VPS económico.

