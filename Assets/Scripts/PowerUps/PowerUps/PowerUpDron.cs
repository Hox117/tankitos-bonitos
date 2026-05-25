using Complete;
using Unity.Netcode;
using UnityEngine;

namespace TankGame
{
    public class PowerUpDron : BasePowerUp
    {
        [Header("Prefab del Dron")]
        public GameObject prefabDron;

        [Header("Spawn")]
        public float radioSpawn = 20f;
        public float alturaSpawn = 5f;
        public int intentosMaximos = 20;

        public override bool AplicarEfecto(GameObject objetivo)
        {
            if (prefabDron == null)
            {
                Debug.LogError("[PowerUpDron] No hay prefab de dron asignado.");
                return false;
            }

            // Leemos el radioAtaque directamente del prefab del dron
            float radioSeguridad = 3f; // valor por defecto si no encuentra el componente
            DroneMovement movimientoDron = prefabDron.GetComponent<DroneMovement>();
            if (movimientoDron != null)
                radioSeguridad = movimientoDron.radioAtaque;

            Vector3 posicionSpawn;
            if (!BuscarPuntoLibre(radioSeguridad, out posicionSpawn))
            {
                Debug.LogWarning("[PowerUpDron] No se encontró punto libre, usando posición por defecto.");
                Vector2 aleatorioFallback = Random.insideUnitCircle * radioSpawn;
                posicionSpawn = new Vector3(aleatorioFallback.x, alturaSpawn, aleatorioFallback.y);
            }

            GameObject instanciaDron = Instantiate(prefabDron, posicionSpawn, Quaternion.identity);
            NetworkObject netObj = instanciaDron.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                netObj.Spawn();
            }
            else
            {
                Debug.LogError("[PowerUpDron] El prefab del dron no tiene NetworkObject.");
                Destroy(instanciaDron);
                return false;
            }

            return true;
        }

        private bool BuscarPuntoLibre(float radioSeguridad, out Vector3 puntoLibre)
        {
            for (int i = 0; i < intentosMaximos; i++)
            {
                Vector2 aleatorio = Random.insideUnitCircle * radioSpawn;
                Vector3 candidato = new Vector3(aleatorio.x, alturaSpawn, aleatorio.y);

                // Comprobamos que ningún tanque esté dentro del radio de ataque del dron
                Collider[] colisiones = Physics.OverlapSphere(candidato, radioSeguridad);
                bool hayTanque = false;

                foreach (Collider col in colisiones)
                {
                    if (col.GetComponent<TankHealth>() != null)
                    {
                        hayTanque = true;
                        break;
                    }
                }

                if (!hayTanque)
                {
                    puntoLibre = candidato;
                    return true;
                }
            }

            puntoLibre = Vector3.zero;
            return false;
        }
    }
}