<div align="center">

# 🐱☕ Cat Cafe ☕🐱

**Videojuego de gestión con minijuegos**

*Trabajo de Fin de Grado · 2º DAM · Curso 2025-2026*

![Unity](https://img.shields.io/badge/Unity-2022.3_LTS-000000?logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-.NET-512BD4?logo=csharp&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-embebida-003B57?logo=sqlite&logoColor=white)
![Plataformas](https://img.shields.io/badge/Windows%20%7C%20Linux-builds-8B5E3C)
![Licencia](https://img.shields.io/badge/Licencia-MIT-green)
![Estado](https://img.shields.io/badge/Estado-completado-brightgreen)

🐾 🐈 🐾 🐈‍⬛ 🐾 🐈 🐾

</div>

---

## 📖 Descripción

Cat Cafe es un videojuego 2D de gestión ambientado en una cafetería temática de gatos 🐈. El jugador atiende a los clientes, prepara bebidas y comida, administra su dinero y rescata gatos superando una serie de minijuegos. Cada gato rescatado aporta una bonificación pasiva a la cafetería.

El proyecto está desarrollado con Unity y C#, utiliza arte en píxel art y guarda la actividad de la partida en una base de datos SQLite embebida, combinada con PlayerPrefs para el progreso rápido.

Se trata del Trabajo de Fin de Grado del ciclo de Desarrollo de Aplicaciones Multiplataforma (DAM), en el Colegio Santa Ana y San Rafael (Madrid). Su objetivo es demostrar la aplicación práctica de la programación orientada a objetos, el diseño de bases de datos relacionales, el desarrollo de interfaces y el uso de patrones de diseño.

## ✨ Funcionalidades

* 🧑‍🍳 **Sistema de clientes:** aparecen de forma automática, piden productos aleatorios y tienen una barra de paciencia. Si se agota, se van sin pagar y se aplica una penalización.
* ☕ **Preparación de productos** desde la cafetera (bebidas) y la cocina (comida), con tiempo de preparación (cooldown) visible mediante una barra animada.
* 🔓 **Desbloqueo progresivo** de productos con las monedas ganadas. Té y zumo están disponibles desde el principio.
* 🎮 **Seis minijuegos** de géneros distintos, conectados mediante un flujo progresivo desde el tablón de gatos.
* 😺 **Tres gatos rescatables** con bonificaciones pasivas distintas.
* 💾 **Guardado dual** con PlayerPrefs y SQLite, menú de pausa con `ESC` y diálogos con efecto máquina de escribir.
* 🎵 Menú principal con fondo parallax, música de ambiente y escena de créditos al completar el último minijuego.
* 🥚 Detalles extra, como gatos que se enfadan si se les acaricia demasiadas veces.

## 🎮 Minijuegos

El orden es fijo: cada minijuego se desbloquea al ganar el anterior. Las recompensas alternan entre gatos y monedas.

| # | Minijuego | Descripción | Recompensa |
|:-:|---|---|---|
| 1 | 🐭 **Caza Ratones** | Clic sobre los ratones antes de que escapen | 🐈‍⬛ Gato: Milo |
| 2 | 🧠 **Encuentra la tecnología** | Memoria con logos de tecnologías | 🪙 Monedas |
| 3 | 🎲 **Dados de Davy Jones** | Combate por turnos con apuestas | 🐈 Gato: Lira |
| 4 | 🍦 **Atrapa los helados** | Se mueve la cesta con `A` y `D` | 🪙 Monedas |
| 5 | 🐛 **BuscaBugs** | Buscaminas 8x8 donde los bugs sustituyen a las minas | 🐱 Gato: Alex |
| 6 | 👨‍💻 **¿Quién quiere ser programador?** | Quiz de Java de 10 preguntas con código incompleto | 🪙 Monedas + créditos |

```
☕ Cafetería ──► 📋 Tablón de gatos ──► 🎮 Minijuego ──► ☕ Vuelta a la cafetería
```

Se gane o se pierda, el jugador siempre vuelve a la cafetería y decide cuándo seguir con el siguiente minijuego.

## 🐾 Gatos y bonificaciones

| Bonificación | Efecto |
|---|---|
| 💰 Multiplicador de ganancias | Los pedidos reportan más monedas |
| 😌 Mayor paciencia | Los clientes tardan más en irse |
| 🚶 Mayor ritmo de aparición | Llegan clientes con más frecuencia |

## 🛠️ Tecnologías

| Área | Herramientas |
|---|---|
| **Motor y lenguaje** | Unity 2022.3 LTS · C# (.NET) |
| **Base de datos** | SQLite embebida · sqlite-net |
| **Interfaz y arte** | TextMeshPro (fuente ComicNeue Bold) · Píxel art con Aseprite |
| **Desarrollo** | Visual Studio Community · Git · Git LFS · GitHub · DB Browser for SQLite · draw.io |

Además se emplean el patrón **Singleton** con `DontDestroyOnLoad` para los gestores globales y **ScriptableObjects** para los datos de productos y gatos.

## 🗄️ Base de datos

La base de datos se guarda en un único archivo (`catcafe.db`) y consta de 8 tablas: `JUGADOR`, `PRODUCTOS`, `PEDIDOS`, `MINIJUEGO`, `JUEGA`, `MJ_SALVACION`, `MJ_RECADO` y `GATO`. Registra los pedidos completados o fallidos, los minijuegos jugados y los gatos rescatados. Todas las operaciones se gestionan desde `GestorBBDD.cs`.

## 💻 Requisitos

| Requisito | Mínimo |
|---|---|
| Sistema operativo | Windows 10 o superior (x64) |
| RAM | 4 GB |
| Gráficos | Compatible con DirectX 11 |
| Disco | 500 MB libres |
| Conexión | No necesita internet ni software adicional |

## 🚀 Ejecución

1. Descarga el contenido del repositorio o el ZIP del juego.
2. Extrae los archivos en cualquier carpeta.
3. Ejecuta `TFG_FINAL.exe` (Windows). También se incluye una build para Linux (`TfgFinalLinix.x86_64`).

El juego arranca en modo ventana (1280x768) directamente en el menú principal. No necesita instalación ni permisos de administrador.

### 🧰 Abrir el proyecto en Unity

1. Clona el repositorio. Como se usa Git LFS, instálalo antes de clonar:
   ```bash
   git lfs install
   git clone https://github.com/pablomoradev/CatCafe-TFG.git
   ```
2. Abre la carpeta desde Unity Hub con la versión 2022.3 LTS del editor.
3. Abre la escena del menú principal y pulsa Play.

## 📁 Estructura del repositorio

<details>
<summary>Ver estructura</summary>

* `Assets/`: escenas, scripts, sprites, audio y base de datos del juego.
* `Packages/`: dependencias de Unity.
* `ProjectSettings/`: configuración del proyecto.
* `TFG_FINAL.exe` y `TfgFinalLinix.x86_64`: builds del juego para Windows y Linux.
* `Instrucciones de despliegue.txt`: requisitos y pasos para ejecutar el juego.
* `UbicacionProyecto.txt`: enlace a la carpeta del proyecto en Google Drive.
* `CREDITS.txt`: créditos de los recursos de terceros.
* `LICENSE.txt`: licencia del proyecto.

Scripts principales: `GameManager`, `GestorClientes`, `GestorProductos`, `GestorMinijuegos`, `GestorGatos` y `GestorBBDD`.

</details>

## 🚧 Estado del proyecto

✅ Completado como Trabajo de Fin de Grado.

Posibles mejoras futuras: más tipos de gatos y de clientes, un sistema de logros, niveles de dificultad seleccionables, guardado en la nube y versiones para móvil.

## 👤 Autor

**Pablo Mora López** · tutorizado por Alejandro Jiménez Vitoria.

## 📄 Licencia

Este proyecto se distribuye bajo la licencia MIT. Consulte el archivo [LICENSE](LICENSE.txt) para más información. Los recursos gráficos y sonoros de terceros conservan su licencia original (CC0 o CC-BY) y se detallan en [CREDITS.txt](CREDITS.txt).

<div align="center">

🐾 *Hecho con cariño (y mucho café) por Pablo Mora López* 🐾

</div>
