using TankGame;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TankGame
{
    public class GameUIManager : NetworkBehaviour
    {
        public static GameUIManager Instance;

        [Header("Elementos UI")]
        public GameObject panelFinPartida;
        public TextMeshProUGUI textoMensaje;
        public Button botonLobby;

        [Header("UI Temporizador")]
        public TextMeshProUGUI textoTiempo;

        [Header("Configuración Temporizador")]
        public float duracionPartida = 180f;

        [Header("Nombres de Escena")]
        public string nombreEscenaLobby = "Lobby";

        private NetworkVariable<float> tiempoRestante = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private bool temporizadorActivo = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            if (panelFinPartida != null) panelFinPartida.SetActive(false);
            if (botonLobby != null) botonLobby.onClick.AddListener(VolverAlLobby);

            if (IsServer)
            {
                tiempoRestante.Value = duracionPartida;
                temporizadorActivo = true;
            }

            tiempoRestante.OnValueChanged += AlCambiarTiempo;
            ActualizarUITiempo(tiempoRestante.Value);

            NetworkManager.Singleton.OnClientDisconnectCallback += AlDesconectarseCliente;
        }

        public override void OnNetworkDespawn()
        {
            tiempoRestante.OnValueChanged -= AlCambiarTiempo;

            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback -= AlDesconectarseCliente;
        }

        private void AlDesconectarseCliente(ulong clientId)
        {
            if (!IsServer && (clientId == NetworkManager.Singleton.LocalClientId || clientId == 0))
            {
                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadScene(nombreEscenaLobby);
            }
        }

        private void Update()
        {
            if (!IsServer || !temporizadorActivo) return;

            tiempoRestante.Value -= Time.deltaTime;

            if (tiempoRestante.Value <= 0f)
            {
                tiempoRestante.Value = 0f;
                temporizadorActivo = false;
                ComprobarFinDePartida();
            }
        }

        private void AlCambiarTiempo(float valorAnterior, float valorNuevo)
        {
            ActualizarUITiempo(valorNuevo);
        }

        private void ActualizarUITiempo(float segundos)
        {
            if (textoTiempo == null) return;

            int minutos = Mathf.FloorToInt(segundos / 60f);
            int segs = Mathf.FloorToInt(segundos % 60f);
            textoTiempo.text = $"{minutos:00}:{segs:00}";

            textoTiempo.color = segundos <= 30f ? Color.red : Color.white;
        }

        public void DetenerTemporizador()
        {
            if (!IsServer) return;
            temporizadorActivo = false;
        }

        public void ComprobarFinDePartida()
        {
            if (!IsServer) return;

            DetenerTemporizador();

            TankHealth[] tanques = FindObjectsByType<TankHealth>(FindObjectsSortMode.None);
            TankHealth ganador = null;
            int vidasMax = -999;

            foreach (TankHealth tanque in tanques)
            {
                if (tanque.m_CurrentLives.Value > vidasMax)
                    vidasMax = tanque.m_CurrentLives.Value;
            }

            foreach (TankHealth tanque in tanques)
            {
                if (tanque.m_CurrentLives.Value == vidasMax)
                    ganador = tanque;
            }

            string mensaje = ganador != null
                ? $"¡EL TANQUE {ganador.NetworkObjectId} GANA LA PARTIDA!"
                : "¡EMPATE!";

            MostrarPanelFinClientRpc(mensaje);
        }

        [ClientRpc]
        private void MostrarPanelFinClientRpc(string mensaje)
        {
            if (textoMensaje != null) textoMensaje.text = mensaje;
            if (panelFinPartida != null) panelFinPartida.SetActive(true);
        }

        private void VolverAlLobby()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();

            SceneManager.LoadScene(nombreEscenaLobby);
        }
    }
}