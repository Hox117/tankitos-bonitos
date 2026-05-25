using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TankGame
{
    public class TankHealth : NetworkBehaviour
    {
        public float m_StartingHealth = 100f;
        public Slider barraDeVida;
        public Image imagenRelleno;
        public Color colorVidaLlena = Color.green;
        public Color colorVidaVacia = Color.red;
        public GameObject prefabExplosion;

        [Header("Display de Vidas")]
        public TextMeshProUGUI textoVidas;

        public NetworkVariable<float> m_CurrentHealth = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> m_CurrentLives = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private bool muerto;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            m_CurrentHealth.OnValueChanged += AlCambiarVida;
            m_CurrentLives.OnValueChanged += AlCambiarVidas;

            if (IsServer)
            {
                m_CurrentHealth.Value = m_StartingHealth;
                m_CurrentLives.Value = 0;
                muerto = false;
            }

            ActualizarBarraDeVida(m_CurrentHealth.Value);
            ActualizarTextoVidas(m_CurrentLives.Value);
        }

        public override void OnNetworkDespawn()
        {
            m_CurrentHealth.OnValueChanged -= AlCambiarVida;
            m_CurrentLives.OnValueChanged -= AlCambiarVidas;
        }

        private void AlCambiarVida(float vidaAnterior, float vidaNueva)
        {
            ActualizarBarraDeVida(vidaNueva);
        }

        private void AlCambiarVidas(int anterior, int nuevo)
        {
            ActualizarTextoVidas(nuevo);
        }

        private void ActualizarTextoVidas(int vidas)
        {
            if (textoVidas != null)
                textoVidas.text = "Bajas: " + vidas.ToString();
        }

        public void TakeDamage(float cantidad)
        {
            if (!IsServer || muerto) return;

            m_CurrentHealth.Value -= cantidad;

            if (m_CurrentHealth.Value <= 0f)
            {
                AlMorir();
            }
        }

        private void ActualizarBarraDeVida(float vida)
        {
            if (barraDeVida != null) barraDeVida.value = vida;
            if (imagenRelleno != null) imagenRelleno.color = Color.Lerp(colorVidaVacia, colorVidaLlena, vida / m_StartingHealth);
        }

        private void AlMorir()
        {
            muerto = true;
            ReproducirExplosionClientRpc(transform.position);
            m_CurrentLives.Value++;
            Respawn();
        }

        [ClientRpc]
        private void ReproducirExplosionClientRpc(Vector3 posicion)
        {
            if (prefabExplosion != null)
            {
                GameObject instanciaExplosion = Instantiate(prefabExplosion, posicion, Quaternion.identity);
                ParticleSystem particulas = instanciaExplosion.GetComponent<ParticleSystem>();
                AudioSource audio = instanciaExplosion.GetComponent<AudioSource>();

                if (particulas != null)
                {
                    particulas.Play();
                    Destroy(instanciaExplosion, particulas.main.duration);
                }

                if (audio != null) audio.Play();
            }
        }

        public void Respawn()
        {
            if (!IsServer) return;

            m_CurrentHealth.Value = m_StartingHealth;
            muerto = false;

            Vector3 posicionDestino = Vector3.zero;
            Quaternion rotacionDestino = Quaternion.identity;

            string nombreSpawn = (OwnerClientId == 0) ? "SpawnPoint1" : "SpawnPoint2";
            GameObject objSpawn = GameObject.Find(nombreSpawn);

            if (objSpawn != null)
            {
                posicionDestino = objSpawn.transform.position;
                rotacionDestino = objSpawn.transform.rotation;
            }
            else
            {
                GameObject spawner = GameObject.Find("PlayerSpawner");
                if (spawner != null)
                {
                    Transform spawnAlternativo = spawner.transform.Find(nombreSpawn);
                    if (spawnAlternativo != null)
                    {
                        posicionDestino = spawnAlternativo.position;
                        rotacionDestino = spawnAlternativo.rotation;
                    }
                }
                else
                {
                    Debug.LogError($"[TankHealth] No se encuentra '{nombreSpawn}' en la escena.");
                }
            }

            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            transform.position = posicionDestino;
            transform.rotation = rotacionDestino;

            if (rb != null) rb.isKinematic = false;

            ReiniciarClientRpc(posicionDestino, rotacionDestino);
        }

        [ClientRpc]
        private void ReiniciarClientRpc(Vector3 posicion, Quaternion rotacion)
        {
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            transform.position = posicion;
            transform.rotation = rotacion;

            ActivarTanque(true);
            ActualizarBarraDeVida(m_StartingHealth);

            if (rb != null) rb.isKinematic = false;
        }

        private void ActivarTanque(bool activo)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true)) r.enabled = activo;
            foreach (Projector p in GetComponentsInChildren<Projector>(true)) p.enabled = activo;
            foreach (Collider c in GetComponentsInChildren<Collider>(true)) c.enabled = activo;

            TankMovement movimiento = GetComponent<TankMovement>();
            if (movimiento != null) movimiento.enabled = activo;

            TankShooting disparo = GetComponent<TankShooting>();
            if (disparo != null) disparo.enabled = activo;

            Canvas canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null) canvas.gameObject.SetActive(activo);
        }
    }
}