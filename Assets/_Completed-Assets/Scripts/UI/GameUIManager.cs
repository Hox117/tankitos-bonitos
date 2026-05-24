using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Complete
{
    public class GameUIManager : NetworkBehaviour
    {
        public static GameUIManager Instance;

        [Header("UI Elements")]
        public GameObject m_EndGamePanel;
        public TextMeshProUGUI m_MessageText;
        public Button m_LobbyButton;

        [Header("Timer UI")]
        public TextMeshProUGUI m_TimerText;         // Texto que muestra el tiempo en pantalla

        [Header("Timer Settings")]
        public float m_MatchDuration = 180f;        // Duración de la partida en segundos

        [Header("Scene Names")]
        public string m_LobbySceneName = "Lobby";

        // NetworkVariable para que todos los clientes vean el mismo tiempo
        private NetworkVariable<float> m_TimeRemaining = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private bool m_TimerRunning = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            if (m_EndGamePanel != null) m_EndGamePanel.SetActive(false);
            if (m_LobbyButton != null) m_LobbyButton.onClick.AddListener(PressLobbyButton);

            if (IsServer)
            {
                m_TimeRemaining.Value = m_MatchDuration;
                m_TimerRunning = true;
            }

            m_TimeRemaining.OnValueChanged += OnTimerChanged;
            UpdateTimerUI(m_TimeRemaining.Value);

            // Detecta cuando el cliente se desconecta (por ejemplo si el host se va)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            m_TimeRemaining.OnValueChanged -= OnTimerChanged;

            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        private void OnClientDisconnected(ulong clientId)
        {
            // En el cliente: si se desconecta su propio ID o el servidor (clientId 0 = host)
            if (!IsServer && (clientId == NetworkManager.Singleton.LocalClientId || clientId == 0))
            {
                // El host se fue, llevamos al cliente al lobby directamente
                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadScene(m_LobbySceneName);
            }
        }
        private void Update()
        {
            // Solo el servidor descuenta el tiempo
            if (!IsServer || !m_TimerRunning) return;

            m_TimeRemaining.Value -= Time.deltaTime;

            if (m_TimeRemaining.Value <= 0f)
            {
                m_TimeRemaining.Value = 0f;
                m_TimerRunning = false;
                CheckGameOver();
            }
        }

        // Se llama automáticamente en todos los clientes cuando cambia el NetworkVariable
        private void OnTimerChanged(float oldValue, float newValue)
        {
            UpdateTimerUI(newValue);
        }

        private void UpdateTimerUI(float seconds)
        {
            if (m_TimerText == null) return;

            int mins = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            m_TimerText.text = $"{mins:00}:{secs:00}";

            // Se pone rojo cuando quedan menos de 30 segundos
            m_TimerText.color = seconds <= 30f ? Color.red : Color.white;
        }

        // Llámalo desde TankHealth cuando un tanque muera, por si termina antes del tiempo
        public void StopTimer()
        {
            if (!IsServer) return;
            m_TimerRunning = false;
        }

        public void CheckGameOver()
        {
            if (!IsServer) return;

            StopTimer();

            TankHealth[] allTanks = FindObjectsByType<TankHealth>(FindObjectsSortMode.None);
            TankHealth winner = null;
            int tankLives = -999;
            foreach (TankHealth tank in allTanks)
            {
                if (tank.m_CurrentLives.Value > tankLives)  // > en vez de 
                    tankLives = tank.m_CurrentLives.Value;
            }
            foreach (TankHealth tank in allTanks)
            {
                if (tank.m_CurrentLives.Value == tankLives)
                    winner = tank;
            }

            string message = winner != null
                ? $"¡EL TANQUE {winner.NetworkObjectId} GANA LA PARTIDA!"
                : "¡EMPATE!";

            ShowEndGameMenuClientRpc(message);
        }

        [ClientRpc]
        private void ShowEndGameMenuClientRpc(string winMessage)
        {
            if (m_MessageText != null) m_MessageText.text = winMessage;
            if (m_EndGamePanel != null) m_EndGamePanel.SetActive(true);
        }

        private void PressLobbyButton()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();

            SceneManager.LoadScene(m_LobbySceneName);
        }
    }
}