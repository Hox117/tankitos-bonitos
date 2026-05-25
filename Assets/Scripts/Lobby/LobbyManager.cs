using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("Botones del menú")]
    [SerializeField] private Button btnCrearPartida;
    [SerializeField] private Button btnUnirse;
    [SerializeField] private Button btnIniciar;

    private void Awake()
    {
        btnIniciar.gameObject.SetActive(false);

        btnCrearPartida.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            AlConectarse();
        });

        btnUnirse.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            AlConectarse();
        });

        btnIniciar.onClick.AddListener(() => {
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("_Complete-Game", LoadSceneMode.Single);
            }
        });
    }

    private void AlConectarse()
    {
        btnCrearPartida.gameObject.SetActive(false);
        btnUnirse.gameObject.SetActive(false);

        // Si soy Host, me suscribo al evento de cuando alguien se une
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += AlUnirseJugador;
        }
    }

    private void AlUnirseJugador(ulong clientId)
    {
        // Ignoramos nuestra propia conexión como Host
        if (clientId == NetworkManager.Singleton.LocalClientId) return;

        // Al menos un jugador se unió, activamos el botón
        btnIniciar.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        // Siempre desuscribirse para evitar memory leaks
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= AlUnirseJugador;
    }
}