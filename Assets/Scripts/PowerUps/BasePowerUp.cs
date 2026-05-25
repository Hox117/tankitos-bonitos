using TankGame;
using Unity.Netcode;
using UnityEngine;

namespace Complete
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Collider))]
    public abstract class BasePowerUp : NetworkBehaviour, IPowerUpEffect
    {
        [Header("Configuración Visual")]
        public float velocidadRotacion = 50f;
        public float velocidadFlotacion = 0.5f;
        public float amplitudFlotacion = 0.2f;

        private Vector3 posicionInicial;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void Start()
        {
            posicionInicial = transform.position;
        }

        private void Update()
        {
            // Animación local en cada cliente
            transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime, Space.World);

            Vector3 pos = posicionInicial;
            pos.y += Mathf.Sin(Time.time * Mathf.PI * velocidadFlotacion) * amplitudFlotacion;
            transform.position = pos;
        }

        private void OnTriggerEnter(Collider otro)
        {
            // Solo el servidor gestiona la recogida
            if (!IsServer) return;

            if (otro.CompareTag("Player"))
            {
                bool recogido = AplicarEfecto(otro.gameObject);

                if (recogido)
                {
                    GetComponent<NetworkObject>().Despawn();
                }
            }
        }

        // Cada PowerUp hijo implementa su propio efecto
        public abstract bool AplicarEfecto(GameObject objetivo);
    }
}