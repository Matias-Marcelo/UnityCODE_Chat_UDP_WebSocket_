const WebSocket = require('ws');
const port = 8080;
const wss = new WebSocket.Server({ port });

// Contador para asignar IDs a los usuarios
let nextUserId = 1;
// Almacenar información de usuarios conectados
const usuarios = new Map();

wss.on('connection', (ws) => {
  // Asignar ID al nuevo cliente
  const userId = nextUserId++;
  const username = `Usuario${userId}`;
  
  // Guardar información del usuario
  usuarios.set(ws, {
    id: userId,
    username: username
  });
  
  console.log(`Cliente conectado: ${username} (ID: ${userId})`);
  
  // Enviar mensaje de bienvenida al nuevo usuario
  ws.send(JSON.stringify({
    tipo: "sistema",
    contenido: "Bienvenido al chat. Usa /nick [nombre] para cambiar tu nombre."
  }));
  
  // Anunciar a todos que un nuevo usuario se conectó
  broadcastMessage({
    tipo: "sistema",
    contenido: `${username} se ha unido al chat.`
  });
  
  // Manejar mensajes recibidos
  ws.on('message', (data) => {
    try {
      const mensaje = JSON.parse(data);
      console.log("Mensaje recibido del cliente:", mensaje);  // <- Agrega esta línea
      const usuario = usuarios.get(ws);
      
      // Manejar comandos
      if (mensaje.contenido.startsWith('/')) {
        handleCommand(ws, mensaje.contenido);
        return;
      }
      
      // Enviar mensaje a todos
      broadcastMessage({
        tipo: "mensaje",
        usuario: usuario.username,
        contenido: mensaje.contenido,
        timestamp: new Date().toISOString()
      });
      
    } catch (error) {
      console.error("Error al procesar mensaje:", error);
      ws.send(JSON.stringify({
        tipo: "error",
        contenido: "Error al procesar tu mensaje"
      }));
    }
  });
  
  // Manejar desconexión
  ws.on('close', () => {
    const usuario = usuarios.get(ws);
    if (usuario) {
      console.log(`Cliente desconectado: ${usuario.username}`);
      broadcastMessage({
        tipo: "sistema",
        contenido: `${usuario.username} ha abandonado el chat.`
      });
      usuarios.delete(ws);
    }
  });
  
  // Manejar errores
  ws.on('error', (error) => {
    console.error(`Error con cliente ${usuarios.get(ws)?.username || 'desconocido'}:`, error);
  });
});

// Función para enviar mensaje a todos los clientes
function broadcastMessage(mensaje) {
  const mensajeString = JSON.stringify(mensaje);
  wss.clients.forEach(client => {
    if (client.readyState === WebSocket.OPEN) {
      client.send(mensajeString);
    }
  });
}

// Función para manejar comandos de chat
function handleCommand(ws, comando) {
  const usuario = usuarios.get(ws);
  
  if (comando.startsWith('/nick ')) {
    const nuevoNombre = comando.substring(6).trim();
    if (nuevoNombre.length < 3) {
      ws.send(JSON.stringify({
        tipo: "sistema",
        contenido: "El nombre debe tener al menos 3 caracteres."
      }));
      return;
    }
    
    const nombreAnterior = usuario.username;
    usuario.username = nuevoNombre;
    
    ws.send(JSON.stringify({
      tipo: "sistema",
      contenido: `Has cambiado tu nombre a ${nuevoNombre}.`
    }));
    
    broadcastMessage({
      tipo: "sistema",
      contenido: `${nombreAnterior} ahora se llama ${nuevoNombre}.`
    });
  } else {
    ws.send(JSON.stringify({
      tipo: "sistema",
      contenido: "Comando desconocido. Comandos disponibles: /nick [nombre]"
    }));
  }
}

console.log(`Servidor WebSocket funcionando en ws://localhost:${port}`);