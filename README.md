# Chat en Línea - Aplicación de Comunicación en Tiempo Real

WebGL URL: https://matias-marcelo.github.io/Chat_UDP_WebSocket/

Este proyecto implementa dos sistemas de chat en tiempo real desarrollados en Unity:
1. **Chat UDP**: Sistema de comunicación cliente-servidor utilizando el protocolo UDP mediante Unity Transport.
2. **Chat WebSocket**: Sistema de comunicación a través de WebSockets que se integra con un backend Node.js.
   
En Node.js (servidor):
Usamos la librería ws:
`npm install ws`

## 📋 Tabla de Contenidos

- [Características](#características)
- [Arquitectura](#arquitectura)
- [Componentes Principales](#componentes-principales)
  - [Chat UDP](#chat-udp)
  - [Chat WebSocket](#chat-websocket)
- [Sistema de Autenticación](#sistema-de-autenticación)
- [Comandos Disponibles](#comandos-disponibles)
- [Cómo Ejecutar](#cómo-ejecutar)
- [Requisitos](#requisitos)

## ✨ Características

- **Autenticación de usuarios**: Sistema de login y registro para identificar usuarios.
- **Chat en tiempo real**: Comunicación instantánea entre usuarios.
- **Comandos de chat**: Sistema de comandos para realizar acciones específicas.
- **Múltiples protocolos**: Implementación mediante UDP y WebSockets.
- **Interfaz de usuario intuitiva**: Diseño sencillo para facilitar la comunicación.
- **Indicadores de estado**: Visualización del estado de conexión al servidor.
- **Cambio de nombre de usuario**: Posibilidad de personalizar la identidad durante la sesión.

## 🏗️ Arquitectura

El proyecto se divide en dos sistemas de comunicación independientes:

### Chat UDP
Utiliza la biblioteca Unity Networking Transport para establecer comunicación mediante UDP entre:
- Un servidor central que coordina los mensajes
- Múltiples clientes que se conectan al servidor

### Chat WebSocket
Se comunica con un servidor Node.js mediante WebSockets para:
- Enviar y recibir mensajes en tiempo real
- Gestionar nombres de usuario y conexiones
- Proporcionar características adicionales a través de comandos

## 🧩 Componentes Principales

### Chat UDP

- **Server.cs**: Implementa el servidor UDP que:
  - Acepta conexiones de clientes
  - Procesa y distribuye mensajes
  - Gestiona el registro de usuarios
  - Ejecuta comandos del servidor

- **Client.cs**: Implementa el cliente que:
  - Se conecta al servidor
  - Envía y recibe mensajes
  - Procesa comandos locales
  - Actualiza la interfaz de usuario

- **CommandHandler.cs**: Sistema centralizado de comandos que:
  - Define comandos para servidor y cliente
  - Procesa la entrada de comandos
  - Ejecuta la lógica asociada a cada comando

- **NetworkTypes.cs**: Define los tipos de mensajes y su serialización:
  - Estructura de mensajes
  - Tipos de mensajes (Chat, Command, Register, etc.)
  - Métodos de serialización y deserialización

- **ChatUI.cs**: Gestiona la interfaz de usuario para el chat UDP:
  - Muestra mensajes recibidos
  - Procesa la entrada del usuario
  - Gestiona la conexión con el servidor o cliente

### Chat WebSocket

- **ChatUI.cs**: Implementa la interfaz y lógica para el chat WebSocket:
  - Establece conexión con el servidor WebSocket
  - Gestiona mensajes entrantes y salientes
  - Actualiza la interfaz de usuario
  - Implementa reconexión automática
  - Procesa comandos específicos para WebSocket

## 🔑 Sistema de Autenticación

- **LoginManager.cs**: Gestiona la autenticación de usuarios:
  - Registro de nuevos usuarios
  - Inicio de sesión de usuarios existentes
  - Validación de datos de entrada
  - Comunicación con API backend para autenticación
  - Almacenamiento de datos de usuario en PlayerPrefs

> **¡IMPORTANTE!** Para acceder al canvas de menú, es necesario completar el proceso de login. Los usuarios predeterminados son user "a" y user "b". Esta autenticación es obligatoria para poder utilizar cualquiera de los sistemas de chat.

## 💬 Comandos Disponibles

### Comandos del Chat UDP

**Comandos del cliente:**
- `/ayuda` - Muestra la lista de comandos disponibles
- `/nombre <nuevo_nombre>` - Cambia el nombre de usuario

## 🚀 Cómo Ejecutar

1. **Inicio de sesión:**
   - Al iniciar la aplicación, deberás hacer login con uno de los usuarios predeterminados:
     - Usuarios disponibles: "a" o "b"
   - **NOTA:** Este paso es obligatorio para acceder al menú principal y utilizar los sistemas de chat

2. **Configuración del entorno:**
   - Asegúrate de tener Unity 2021.3 o superior
   - Para el Chat WebSocket, necesitas un servidor Node.js en ejecución

3. **Para el Chat UDP:**
   - Una vez en el menú principal, selecciona "Iniciar servidor" para crear un servidor
   - Otros usuarios pueden conectarse al servidor seleccionando "Iniciar cliente"

4. **Para el Chat WebSocket:**
   - Asegúrate de que el servidor WebSocket esté en ejecución (por defecto en ws://localhost:8080)
   - Accede a la interfaz del Chat WebSocket desde el menú principal
   - Conecta al servidor utilizando el botón "Conectar"

## 📋 Requisitos

- Unity 2021.3 o superior
- .NET Framework 4.x
- Para el Chat WebSocket: Node.js y un servidor WebSocket compatible

