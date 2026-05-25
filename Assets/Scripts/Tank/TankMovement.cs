using Unity.Netcode;
using UnityEngine;

namespace TankGame
{
    public class TankMovement : NetworkBehaviour
    {
        public float velocidad = 12f;
        public float velocidadGiro = 180f;
        public AudioSource audioMovimiento;
        public AudioClip sonidoRalenti;
        public AudioClip sonidoMoviendo;
        public float rangoPitch = 0.2f;

        private Rigidbody rb;
        private float inputMovimiento;
        private float inputGiro;
        private float pitchOriginal;
        private NetworkVariable<Color> colorRed = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            pitchOriginal = audioMovimiento.pitch;

        }

      

        private void Update()
        {
            

            inputMovimiento = Input.GetAxis("Vertical");
            inputGiro = Input.GetAxis("Horizontal");
            AudioMotor();
        }

        private void FixedUpdate()
        {
            
            Mover();
            Girar();
        }

        private void Mover()
        {
            Vector3 movimiento = transform.forward * inputMovimiento * velocidad * Time.deltaTime;
            rb.MovePosition(rb.position + movimiento);
        }

        private void Girar()
        {
            float giro = inputGiro * velocidadGiro * Time.deltaTime;
            Quaternion rotacionGiro = Quaternion.Euler(0f, giro, 0f);
            rb.MoveRotation(rb.rotation * rotacionGiro);
        }

        private void AudioMotor() { /* Lógica de audio */ }
    }
}