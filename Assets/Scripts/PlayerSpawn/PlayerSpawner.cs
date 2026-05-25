using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private Transform[] puntosDeSalida;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Mezclamos los puntos de salida antes de asignarlos
            List<Transform> puntosmezclados = new List<Transform>(puntosDeSalida);
            MezclarLista(puntosmezclados);

            int index = 0;
            foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
            {
                GameObject tanque = cliente.PlayerObject.gameObject;
                Transform punto = puntosmezclados[index % puntosmezclados.Count];

                tanque.transform.position = punto.position;
                tanque.transform.rotation = punto.rotation;

                index++;
            }
        }
    }

    private void MezclarLista(List<Transform> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Transform temp = lista[i];
            lista[i] = lista[j];
            lista[j] = temp;
        }
    }
}