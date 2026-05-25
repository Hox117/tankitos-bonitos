using Complete;
using Unity.Netcode;
using UnityEngine;

namespace TankGame
{
    public class ShellExplosion : MonoBehaviour
    {
        public LayerMask capaTanques;
        public ParticleSystem particulasExplosion;

        public float dañoMaximo = 100f;
        public float fuerzaExplosion = 0f;
        public float tiempoDeVida = 2f;
        public float radioExplosion = 5f;

        private bool Explotado = false;

        private void Start()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Destroy(gameObject, tiempoDeVida);
            }
        }

        private void OnTriggerEnter(Collider otro)
        {
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer) return;

            Collider[] colisiones = Physics.OverlapSphere(transform.position, radioExplosion, capaTanques);

            for (int i = 0; i < colisiones.Length; i++)
            {
                Rigidbody rb = colisiones[i].GetComponent<Rigidbody>();
                if (rb == null) continue;

                TankHealth vida = rb.GetComponent<TankHealth>();
                if (vida == null) continue;

                float daño = CalcularDaño(rb.position);
                vida.TakeDamage(daño);
            }

            Destroy(gameObject);
        }

        // Se ejecuta en todos los clientes cuando el objeto se destruye
        private void OnDestroy()
        {
            if (Explotado) return;
            Explotado = true;

            if (particulasExplosion != null)
            {
                particulasExplosion.transform.parent = null;
                particulasExplosion.Play();
                Destroy(particulasExplosion.gameObject, particulasExplosion.main.duration);
            }
        }

        private float CalcularDaño(Vector3 posicionObjetivo)
        {
            Vector3 distanciaVector = posicionObjetivo - transform.position;
            float distancia = distanciaVector.magnitude;
            float distanciaRelativa = (radioExplosion - distancia) / radioExplosion;
            float daño = Mathf.Max(0f, distanciaRelativa * dañoMaximo);
            return daño;
        }
    }
}