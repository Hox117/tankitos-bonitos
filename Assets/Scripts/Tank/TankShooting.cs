using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace TankGame
{
    public class TankShooting : NetworkBehaviour
    {
        public int numeroJugador = 1;
        public GameObject prefabBala;
        public Transform puntoDisparo;
        public Slider sliderApuntado;
        public AudioSource audioDisparo;
        public AudioClip sonidoCarga;
        public AudioClip sonidoDisparo;
        public float fuerzaMinima = 15f;
        public float fuerzaMaxima = 30f;
        public float tiempoCargaMax = 0.75f;

        private string botonDisparo;
        private float fuerzaActual;
        private float velocidadCarga;
        private bool disparado;

        private void OnEnable()
        {
            fuerzaActual = fuerzaMinima;
            if (sliderApuntado != null) sliderApuntado.value = fuerzaMinima;
        }

        private void Start()
        {
            botonDisparo = "Fire" + numeroJugador;
            velocidadCarga = (fuerzaMaxima - fuerzaMinima) / tiempoCargaMax;
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (sliderApuntado != null) sliderApuntado.value = fuerzaMinima;

            if (fuerzaActual >= fuerzaMaxima && !disparado)
            {
                fuerzaActual = fuerzaMaxima;
                Disparar();
            }
            else if (Input.GetButtonDown(botonDisparo))
            {
                disparado = false;
                fuerzaActual = fuerzaMinima;

                if (audioDisparo != null)
                {
                    audioDisparo.clip = sonidoCarga;
                    audioDisparo.Play();
                }
            }
            else if (Input.GetButton(botonDisparo) && !disparado)
            {
                fuerzaActual += velocidadCarga * Time.deltaTime;
                if (sliderApuntado != null) sliderApuntado.value = fuerzaActual;
            }
            else if (Input.GetButtonUp(botonDisparo) && !disparado)
            {
                Disparar();
            }
        }

        private void Disparar()
        {
            disparado = true;
            DispararServerRpc(fuerzaActual);
            fuerzaActual = fuerzaMinima;
        }

        [ServerRpc]
        private void DispararServerRpc(float fuerza)
        {
            if (prefabBala == null)
            {
                
                return;
            }
            if (puntoDisparo == null)
            {
                
                return;
            }

            GameObject instanciaBala = Instantiate(prefabBala, puntoDisparo.position, puntoDisparo.rotation);

            Rigidbody rbBala = instanciaBala.GetComponent<Rigidbody>();
            if (rbBala != null)
            {
                rbBala.isKinematic = false;
                rbBala.velocity = fuerza * puntoDisparo.forward;
            }
            

            NetworkObject netObj = instanciaBala.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
            

            ReproducirAudioDisparoClientRpc();
        }

        [ClientRpc]
        private void ReproducirAudioDisparoClientRpc()
        {
            if (audioDisparo != null)
            {
                audioDisparo.clip = sonidoDisparo;
                audioDisparo.Play();
            }
        }
    }
}