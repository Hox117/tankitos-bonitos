using UnityEngine;

namespace TankGame
{
    public class UIDirectionControl : MonoBehaviour
    {
        

        private Quaternion rotacionInicial;

        private void Start()
        {
            rotacionInicial = transform.parent.localRotation;
        }

        private void Update()
        {
            
                transform.rotation = rotacionInicial;
        }
    }
}