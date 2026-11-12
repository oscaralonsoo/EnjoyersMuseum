# EnjoyersMuseum 🏛️

**Museo de Realidad Mixta – Meta Quest**

Proyecto desarrollado para la asignatura durante el curso 2026-2027 en el CITM (Universitat Politècnica de Catalunya).

## 👥 Equipo

- Oscar Alonso
- Mario Torrents
- David Subirats
- Toni Llovera
- Javier Gómez

## 🎮 Descripción

**EnjoyersMuseum** es una experiencia de realidad mixta que reúne los diferentes videojuegos desarrollados por el grupo en un único museo virtual.

Gracias al *Passthrough* de Meta Quest, el jugador comienza en su entorno real, donde encuentra un portal y varias gemas. Cada gema representa uno de los proyectos del grupo.

El jugador puede coger una gema utilizando los mandos o sus propias manos y colocarla en el soporte del portal. Al hacerlo, el portal se abre y permite acceder al mundo virtual correspondiente.

Una vez dentro, el jugador puede explorar o jugar a los diferentes proyectos y regresar al museo atravesando nuevamente el portal.

## 🛠️ Tecnologías y funcionalidades

El proyecto utiliza Unity y el Meta XR SDK, incluyendo los siguientes Building Blocks:

- **Camera Rig e Interaction Rig:** configuración base para Meta Quest.
- **Passthrough:** visualización del entorno real.
- **Grab Interaction:** agarrar y mover objetos.
- **Distance Grab:** agarrar objetos a distancia.
- **Snap Interaction:** colocar las gemas en el soporte del portal.
- **Hand Tracking:** interacción utilizando las manos.
- **Teleport:** locomoción dentro de los mundos virtuales.
- **Ray Interaction:** interacción mediante rayos.
- **Controller Buttons Mapper:** configuración de los botones de los mandos.

## ✨ Características principales

- **Portal de realidad mixta:** permite ver los mundos virtuales a través de un portal integrado en el entorno real.
- **Sistema de gemas:** cada gema está asociada a un mundo mediante un identificador (`WorldId`).
- **PortalActivator:** gestiona la apertura y el cierre del portal según la gema colocada.
- **PortalCrossing:** detecta cuándo el jugador atraviesa el portal y cambia entre realidad mixta e inmersión virtual.
- **Gem Snap Shader:** shader de cristal con facetas, reflejos y cambios de color para indicar dónde colocar las gemas.
- **PhysicsGrabTransformer:** sistema de agarre basado en físicas que evita que los objetos atraviesen otros colliders.

## 🚀 Instalación y ejecución

1. Clonar o descargar el repositorio.
2. Abrir el proyecto desde Unity Hub.
3. Abrir la escena principal del museo.
4. Configurar el proyecto para Meta Quest con el Meta XR SDK.
5. Conectar el visor Meta Quest y ejecutar la experiencia.

**Nota:** Es necesario disponer de un visor Meta Quest compatible para disfrutar de la experiencia completa de realidad mixta.

## 🎯 Objetivo

El objetivo es explorar el museo, descubrir todos los proyectos del grupo y visitar cada uno de sus mundos mediante el sistema de portales y gemas.

## 🎓 Información académica

- **Proyecto:** Activity 2 - Meta XR Building Blocks
- **Curso:** 2026-2027
- **Centro:** Universitat Politècnica de Catalunya
- **Grado:** CITM
