using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NativeWebSocket;

public class ChatManager : MonoBehaviour
{
    [Header("Configuración WebSocket")]
    [SerializeField] private string webSocketUrl = "ws://localhost:8080";
    [SerializeField] private float reconnectDelay = 3f;
    [SerializeField] private bool conectarAlIniciar = false;

    [Header("Referencias UI")]
    // Área de mensajes
    [SerializeField] private TMP_Text outputChat;
    [SerializeField] private ScrollRect scrollViewChat;

    // Controles de mensajes
    [SerializeField] private TMP_InputField inputMensaje;
    [SerializeField] private Button botonEnviar;

    // Cambio de nombre
    [SerializeField] private TMP_InputField inputNombre;
    [SerializeField] private Button botonCambiarNombre;

    // Controles de conexión
    [SerializeField] private Button botonConectar;
    [SerializeField] private Button botonDesconectar;
    [SerializeField] private Button botonReconectar;
    [SerializeField] private TMP_Text estadoConexion;

    // Paneles
    [SerializeField] private GameObject panelChat;
    [SerializeField] private GameObject panelConexion;
    [SerializeField] private GameObject panelReconexion;

    private WebSocket websocket;
    private bool intentandoReconectar = false;
    private string nombreUsuario = "Usuario";
    private Queue<string> mensajesRecibidos = new Queue<string>();
    private bool esNecesarioProcesarMensajes = false;

    [Serializable]
    public class Mensaje
    {
        public string tipo;
        public string contenido;
        public string usuario;
        public string timestamp;
    }

    private void Start()
    {
        // Configurar listeners para todos los botones
        botonEnviar.onClick.AddListener(EnviarMensaje);

        if (botonConectar != null)
            botonConectar.onClick.AddListener(IniciarConexion);

        if (botonDesconectar != null)
            botonDesconectar.onClick.AddListener(CerrarConexion);

        if (botonCambiarNombre != null)
            botonCambiarNombre.onClick.AddListener(CambiarNombre);

        // Listener para Enter en campo de texto
        inputMensaje.onEndEdit.AddListener(delegate { VerificarEnterPresionado(); });

        // Configurar UI inicial
        ActualizarEstadoUI(false);

        // Forzar mostrar el panel del chat al iniciar
        if (panelChat != null)
            panelChat.SetActive(true);

        if (conectarAlIniciar)
        {
            IniciarConexion();
        }

        // Conectar automáticamente si está configurado
        if (conectarAlIniciar)
        {
            IniciarConexion();
        }
    }

    private void Update()
    {
        // Procesar mensajes recibidos
        if (esNecesarioProcesarMensajes)
        {
            ProcesarMensajesRecibidos();
            esNecesarioProcesarMensajes = false;
        }

        #if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
        #endif

    }

    private void OnDestroy()
    {
        CerrarConexion();
    }

    private void OnApplicationQuit()
    {
        CerrarConexion();
    }

    // Método para iniciar conexión (llamado por el botón Conectar)
    public void IniciarConexion()
    {
        MostrarMensaje("<color=#888888><i>Iniciando conexión al servidor...</i></color>\n");
        IniciarWebSocket();
    }

    private async void IniciarWebSocket()
    {
        // Cerrar conexión previa si existe
        if (websocket != null)
        {
            await websocket.Close();
        }

        // Crear nueva conexión
        websocket = new WebSocket(webSocketUrl);

        // Configurar handlers de eventos
        websocket.OnOpen += () => {
            Debug.Log("Conexión establecida!");
            ActualizarEstadoUI(true);
            intentandoReconectar = false;
            //MostrarMensaje("<color=#888888><i>Conexión establecida con el servidor.</i></color>\n");
        };

        websocket.OnMessage += (bytes) => {
            string mensaje = System.Text.Encoding.UTF8.GetString(bytes);
            mensajesRecibidos.Enqueue(mensaje);
            esNecesarioProcesarMensajes = true;
        };

        websocket.OnClose += (closeCode) => {
            Debug.Log($"Conexión cerrada con código: {closeCode}");
            ActualizarEstadoUI(false);
            MostrarMensaje("<color=#888888><i>Conexión cerrada.</i></color>\n");

            if (!intentandoReconectar)
            {
                intentandoReconectar = true;
                StartCoroutine(ReconectarAutomaticamente());
            }
        };

        websocket.OnError += (errorMsg) => {
            Debug.LogError($"Error WebSocket: {errorMsg}");
            ActualizarEstadoUI(false);
            MostrarMensaje($"<color=red><i>Error de conexión: {errorMsg}</i></color>\n");
        };

        // Intentar conectar
        try
        {
            await websocket.Connect();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al conectar: {e.Message}");
            ActualizarEstadoUI(false);
            MostrarMensaje($"<color=red><i>Error al conectar: {e.Message}</i></color>\n");

            if (!intentandoReconectar)
            {
                intentandoReconectar = true;
                StartCoroutine(ReconectarAutomaticamente());
            }
        }
    }

private void ProcesarMensajesRecibidos()
{
    while (mensajesRecibidos.Count > 0)
    {
        string mensajeJson = mensajesRecibidos.Dequeue();
        try
        {
            Mensaje mensaje = JsonUtility.FromJson<Mensaje>(mensajeJson);

            if (mensaje.tipo == "mensaje")
            {
                try
                {
                    DateTime tiempo = DateTime.Parse(mensaje.timestamp);
                    string textoMensaje = $"[{tiempo:HH:mm:ss}] <b>{mensaje.usuario}:</b> {mensaje.contenido}\n";
                    MostrarMensaje(textoMensaje);
                }
                catch
                {
                    // Si hay error al parsear la fecha, mostrar mensaje sin timestamp
                    string textoMensaje = $"<b>{mensaje.usuario}:</b> {mensaje.contenido}\n";
                    MostrarMensaje(textoMensaje);
                }
            }

            // OMITIR mensaje.tipo == "sistema" o "error"

        }
        catch (Exception e)
        {
            Debug.LogError($"Error al procesar mensaje: {e.Message}. Mensaje: {mensajeJson}");
            MostrarMensaje($"<color=red><i>Error al procesar mensaje</i></color>\n");
        }
    }
}

    private void MostrarMensaje(string mensaje)
    {
        // Añadir mensaje al área de chat
        outputChat.text += mensaje;

        // Forzar actualización y scroll al final
        Canvas.ForceUpdateCanvases();
        if (scrollViewChat != null)
        {
            scrollViewChat.verticalNormalizedPosition = 0f;
        }
    }

    private void ActualizarEstadoUI(bool conectado)
    {
        // Actualizar indicador de estado
        if (estadoConexion != null)
        {
            estadoConexion.text = conectado ? "Conectado" : "Desconectado";
            estadoConexion.color = conectado ? Color.green : Color.red;
        }

        // Actualizar visibilidad de paneles
        if (panelReconexion != null)
        {
            panelReconexion.SetActive(!conectado);
        }

        if (panelChat != null)
        {
            panelChat.SetActive(conectado);
        }

        if (panelConexion != null)
        {
            // Mostrar el panel de conexión solo cuando estamos desconectados
            // y no estamos intentando reconectar automáticamente
            panelConexion.SetActive(!conectado && !intentandoReconectar);
        }

        // Habilitar/deshabilitar controles de chat
        if (botonEnviar != null)
        {
            botonEnviar.interactable = conectado;
        }

        if (inputMensaje != null)
        {
            inputMensaje.interactable = conectado;
        }

        // Habilitar/deshabilitar cambio de nombre
        if (botonCambiarNombre != null)
        {
            botonCambiarNombre.interactable = conectado;
        }

        if (inputNombre != null)
        {
            inputNombre.interactable = conectado;
        }

        // Habilitar/deshabilitar botones de conexión
        if (botonConectar != null)
        {
            botonConectar.interactable = !conectado && !intentandoReconectar;
        }

        if (botonDesconectar != null)
        {
            botonDesconectar.interactable = conectado;
        }
    }

    public async void EnviarMensaje()
    {
        if (string.IsNullOrEmpty(inputMensaje.text) || websocket == null ||
            websocket.State != WebSocketState.Open)
        {
            return;
        }

        try
        {
            Mensaje mensaje = new Mensaje
            {
                tipo = "mensaje",
                contenido = inputMensaje.text,
                usuario = nombreUsuario,
                timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
            };

            string mensajeJson = JsonUtility.ToJson(mensaje);
            await websocket.SendText(mensajeJson);

            inputMensaje.text = "";
            inputMensaje.ActivateInputField();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al enviar mensaje: {e.Message}");
            MostrarMensaje("<color=red><i>Error al enviar mensaje</i></color>\n");
        }
    }

    // Método para cambiar el nombre usando el comando /nick
    public async void CambiarNombre()
    {
        if (string.IsNullOrEmpty(inputNombre.text) || websocket == null ||
            websocket.State != WebSocketState.Open)
        {
            return;
        }

        try
        {
            string nuevoNombre = inputNombre.text.Trim();

            // Comprobar longitud mínima
            if (nuevoNombre.Length < 3)
            {
                MostrarMensaje("<color=red><i>El nombre debe tener al menos 3 caracteres</i></color>\n");
                return;
            }

            Mensaje mensaje = new Mensaje
            {
                tipo = "mensaje",
                contenido = "/nick " + nuevoNombre,
                usuario = nombreUsuario,
                timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
            };

            string mensajeJson = JsonUtility.ToJson(mensaje);
            await websocket.SendText(mensajeJson);

            // Actualizar el nombre local para futuros mensajes
            nombreUsuario = nuevoNombre;

            inputNombre.text = "";
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al cambiar nombre: {e.Message}");
            MostrarMensaje("<color=red><i>Error al cambiar nombre</i></color>\n");
        }
    }

    private void VerificarEnterPresionado()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            EnviarMensaje();
        }
    }

    public void ReconectarManualmente()
    {
        StopAllCoroutines();
        intentandoReconectar = false;
        MostrarMensaje("<color=#888888><i>Intentando reconexión manual...</i></color>\n");
        IniciarWebSocket();
    }

    private IEnumerator ReconectarAutomaticamente()
    {
        int intentos = 0;

        while (websocket == null || websocket.State != WebSocketState.Open)
        {
            intentos++;
            Debug.Log($"Intento de reconexión #{intentos} en {reconnectDelay} segundos...");

            if (estadoConexion != null)
            {
                estadoConexion.text = $"Reconectando ({intentos})...";
            }

            MostrarMensaje($"<color=#888888><i>Intento de reconexión #{intentos} en {reconnectDelay} segundos...</i></color>\n");

            yield return new WaitForSeconds(reconnectDelay);

            if ((websocket == null || websocket.State != WebSocketState.Connecting) && intentandoReconectar)
            {
                Debug.Log("Ejecutando reconexión automática...");
                IniciarWebSocket();
            }

            // Si ya hemos intentado 5 veces, esperar más tiempo
            if (intentos >= 5)
            {
                yield return new WaitForSeconds(reconnectDelay);
            }
        }

        intentandoReconectar = false;
    }

    public async void CerrarConexion()
    {
        if (websocket != null)
        {
            // Detener cualquier intento de reconexión
            StopAllCoroutines();
            intentandoReconectar = false;

            // Cerrar conexión si está abierta
            if (websocket.State == WebSocketState.Open)
            {
                MostrarMensaje("<color=#888888><i>Cerrando conexión...</i></color>\n");
                await websocket.Close();
            }

            websocket = null;
            ActualizarEstadoUI(false);
        }
    }
}