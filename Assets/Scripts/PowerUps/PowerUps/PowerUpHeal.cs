using Complete;
using UnityEngine;

namespace TankGame
{
    public class PowerUpHeal : BasePowerUp
    {
        [Header("Configuración de Cura")]
        public float cantidadCura = 50f;

        public override bool AplicarEfecto(GameObject objetivo)
        {
            TankHealth vida = objetivo.GetComponent<TankHealth>();

            if (vida != null)
            {
                if (vida.m_CurrentHealth.Value >= vida.m_StartingHealth) return false;

                float vidaNueva = vida.m_CurrentHealth.Value + cantidadCura;
                vida.m_CurrentHealth.Value = Mathf.Min(vidaNueva, vida.m_StartingHealth);

                Debug.Log("[PowerUp] Tanque curado.");
                return true;
            }
            return false;
        }
    }
}