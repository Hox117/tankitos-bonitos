using Complete;
using System;
using UnityEngine;

namespace TankGame
{
    [Serializable]
    public class TankManager
    {
        [HideInInspector] public Color colorJugador;
        [HideInInspector] public int numeroJugador;
        [HideInInspector] public GameObject instancia;
        [HideInInspector] public int victorias;

        public void Configurar()
        {
            // El color lo gestiona TankMovement por red
        }

        public void DesactivarControl()
        {
            instancia.GetComponent<TankMovement>().enabled = false;
            instancia.GetComponent<TankShooting>().enabled = false;
            instancia.GetComponentInChildren<Canvas>().gameObject.SetActive(false);
        }

        public void ActivarControl()
        {
            // El OnNetworkSpawn del tanque refuerza que solo el dueño pueda controlar
            instancia.GetComponent<TankMovement>().enabled = true;
            instancia.GetComponent<TankShooting>().enabled = true;
            instancia.GetComponentInChildren<Canvas>().gameObject.SetActive(true);
        }

        public void Reiniciar()
        {
            // La posición la maneja el Spawner de red
            instancia.SetActive(false);
            instancia.SetActive(true);
        }
    }
}