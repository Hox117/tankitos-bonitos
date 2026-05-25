using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace TankGame
{
    public class DroneSpawner : NetworkBehaviour
    {
        // Llama a este método estático desde el PowerUp para crear el dron
        public static void SpawnDron(Vector3 posicion)
        {
            GameObject dron = new GameObject("Dron");

            // Geometría visual: esfera simple
            GameObject cuerpo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cuerpo.transform.SetParent(dron.transform);
            cuerpo.transform.localScale = Vector3.one * 0.6f;
            cuerpo.transform.localPosition = Vector3.zero;

            // Color distintivo
            Renderer render = cuerpo.GetComponent<Renderer>();
            if (render != null)
                render.material.color = new Color(0.2f, 0.8f, 1f);

            // Quitamos el collider del cuerpo visual para no interferir
            Collider colCuerpo = cuerpo.GetComponent<Collider>();
            if (colCuerpo != null) Destroy(colCuerpo);

            // Collider propio del dron
            SphereCollider col = dron.AddComponent<SphereCollider>();
            col.radius = 0.5f;
            col.isTrigger = true;

            // Componentes de red y movimiento
            dron.AddComponent<NetworkObject>();
            dron.AddComponent<NetworkTransform>();
            dron.AddComponent<DroneMovement>();

            // Posición inicial
            posicion.y = 5f;
            dron.transform.position = posicion;

            // Spawneamos en red
            NetworkObject netObj = dron.GetComponent<NetworkObject>();
            netObj.Spawn();
        }
    }
}