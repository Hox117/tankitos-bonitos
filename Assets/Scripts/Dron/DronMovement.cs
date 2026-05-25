using Unity.Netcode;
using UnityEngine;

namespace TankGame
{
    public class DroneMovement : NetworkBehaviour
    {
        [Header("Movimiento")]
        public float velocidad = 5f;
        public float radioDeambulacion = 20f;
        public float alturaVuelo = 5f;
        public float distanciaLlegada = 1f;

        [Header("Torreta")]
        public GameObject prefabBala;
        public float radioAtaque = 15f;
        public float cadenciaDisparo = 2f;
        public float fuerzaBala = 20f;

        [Header("Vida del Dron")]
        public float tiempoVida = 30f;
        public GameObject prefabImplosion; // Opcional: efecto de partículas al morir

        private Vector3 puntoObjetivo;
        private float tiempoUltimoDisparo;
        private float tiempoNacimiento;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    alturaVuelo,
                    transform.position.z
                );
                GenerarNuevoPunto();
                tiempoNacimiento = Time.time;
            }
        }

        private void Update()
        {
            if (!IsServer) return;

            // Comprobamos si ha llegado la hora de implotar
            if (Time.time >= tiempoNacimiento + tiempoVida)
            {
                Implotar();
                return;
            }

            TankHealth tanqueCercano = BuscarTanqueCercano();

            if (tanqueCercano != null)
            {
                ApuntarA(tanqueCercano.transform.position);

                if (Time.time >= tiempoUltimoDisparo + cadenciaDisparo)
                {
                    Disparar(tanqueCercano.transform.position);
                    tiempoUltimoDisparo = Time.time;
                }
            }
            else
            {
                MoverHaciaPunto();
            }
        }

        private void Implotar()
        {
            ReproducirImplosionClientRpc(transform.position);
            GetComponent<NetworkObject>().Despawn();
        }

        [ClientRpc]
        private void ReproducirImplosionClientRpc(Vector3 posicion)
        {
            if (prefabImplosion != null)
            {
                GameObject efecto = Instantiate(prefabImplosion, posicion, Quaternion.identity);
                ParticleSystem particulas = efecto.GetComponent<ParticleSystem>();
                if (particulas != null)
                {
                    particulas.Play();
                    Destroy(efecto, particulas.main.duration);
                }
            }
        }

        private void MoverHaciaPunto()
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                puntoObjetivo,
                velocidad * Time.deltaTime
            );

            Vector3 direccion = puntoObjetivo - transform.position;
            if (direccion != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direccion);

            if (Vector3.Distance(transform.position, puntoObjetivo) <= distanciaLlegada)
                GenerarNuevoPunto();
        }

        private void ApuntarA(Vector3 posicionObjetivo)
        {
            Vector3 direccion = posicionObjetivo - transform.position;
            if (direccion != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direccion);
        }

        private void Disparar(Vector3 posicionObjetivo)
        {
            if (prefabBala == null)
            {
                Debug.LogError("[DroneMovement] No hay prefab de bala asignado.");
                return;
            }

            Vector3 direccion = (posicionObjetivo - transform.position).normalized;
            GameObject instanciaBala = Instantiate(prefabBala, transform.position, Quaternion.LookRotation(direccion));

            Rigidbody rbBala = instanciaBala.GetComponent<Rigidbody>();
            if (rbBala != null)
            {
                rbBala.isKinematic = false;
                rbBala.velocity = direccion * fuerzaBala;
            }

            NetworkObject netObj = instanciaBala.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
            else
            {
                Debug.LogError("[DroneMovement] El prefab de bala no tiene NetworkObject.");
                Destroy(instanciaBala);
            }
        }

        private TankHealth BuscarTanqueCercano()
        {
            Collider[] colisiones = Physics.OverlapSphere(transform.position, radioAtaque);
            TankHealth tanqueCercano = null;
            float distanciaMinima = float.MaxValue;

            foreach (Collider col in colisiones)
            {
                TankHealth vida = col.GetComponent<TankHealth>();
                if (vida == null) continue;

                float distancia = Vector3.Distance(transform.position, col.transform.position);
                if (distancia < distanciaMinima)
                {
                    distanciaMinima = distancia;
                    tanqueCercano = vida;
                }
            }

            return tanqueCercano;
        }

        private void GenerarNuevoPunto()
        {
            Vector2 puntoAleatorio = Random.insideUnitCircle * radioDeambulacion;
            puntoObjetivo = new Vector3(puntoAleatorio.x, alturaVuelo, puntoAleatorio.y);
        }
    }
}