using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TankGame
{
    [RequireComponent(typeof(BoxCollider))]
    public class PowerUpSpawnZone : NetworkBehaviour
    {
        [Header("Pool de Power-Ups")]
        public GameObject[] prefabsPowerUp;

        [Header("Configuración del Spawn")]
        public float intervaloSpawn = 10f;
        public int maxItemsEnZona = 5;
        public float radioLibre = 1.5f;
        public LayerMask mascaraObstaculos;

        private BoxCollider colisionZona;
        private List<GameObject> itemsSpawneados = new List<GameObject>();
        private List<GameObject> minitanquesSpawneados = new List<GameObject>();

        private void Awake()
        {
            colisionZona = GetComponent<BoxCollider>();
            colisionZona.isTrigger = true;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                LimpiarZona();
                StartCoroutine(RutinaDeSpawn());
            }
        }

        public void LimpiarZona()
        {
            if (!IsServer) return;

            foreach (GameObject item in itemsSpawneados)
            {
                if (item != null)
                {
                    NetworkObject netObj = item.GetComponent<NetworkObject>();
                    if (netObj != null && netObj.IsSpawned) netObj.Despawn();
                }
            }
            itemsSpawneados.Clear();

            foreach (GameObject mini in minitanquesSpawneados)
            {
                if (mini != null)
                {
                    NetworkObject netObj = mini.GetComponent<NetworkObject>();
                    if (netObj != null && netObj.IsSpawned) netObj.Despawn();
                }
            }
            minitanquesSpawneados.Clear();
        }

        private IEnumerator RutinaDeSpawn()
        {
            while (true)
            {
                yield return new WaitForSeconds(intervaloSpawn);

                itemsSpawneados.RemoveAll(item => item == null);
                minitanquesSpawneados.RemoveAll(mini => mini == null);

                if (itemsSpawneados.Count < maxItemsEnZona && prefabsPowerUp.Length > 0)
                {
                    IntentarSpawnPowerUp();
                }
            }
        }

        private void IntentarSpawnPowerUp()
        {
            for (int i = 0; i < 10; i++)
            {
                Vector3 puntoAleatorio = PuntoAleatorioEnBounds(colisionZona.bounds);

                if (!Physics.CheckSphere(puntoAleatorio, radioLibre, mascaraObstaculos))
                {
                    GameObject prefabElegido = prefabsPowerUp[Random.Range(0, prefabsPowerUp.Length)];
                    GameObject instancia = Instantiate(prefabElegido, puntoAleatorio, Quaternion.identity);
                    itemsSpawneados.Add(instancia);

                    NetworkObject netObj = instancia.GetComponent<NetworkObject>();
                    if (netObj != null) netObj.Spawn();

                    break;
                }
            }
        }

        public void RegistrarMinitanque(GameObject minitanque)
        {
            if (minitanque != null) minitanquesSpawneados.Add(minitanque);
        }

        private Vector3 PuntoAleatorioEnBounds(Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        private void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null) return;
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawCube(transform.position, box.size);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, box.size);
        }
    }
}