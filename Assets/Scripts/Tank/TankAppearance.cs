using Unity.Netcode;
using UnityEngine;

public class TankAppearance : NetworkBehaviour
{
    private NetworkVariable<Color> colorSincronizado = new NetworkVariable<Color>(Color.blue);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            colorSincronizado.Value = OwnerClientId == 0
                ? (Color)new Color32(222, 49, 99, 255)
                : (Color)new Color32(170, 51, 106, 255);
        }

        AplicarColor(colorSincronizado.Value);
        colorSincronizado.OnValueChanged += (colorAnterior, colorNuevo) => AplicarColor(colorNuevo);
    }

    private void AplicarColor(Color color)
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers) r.material.color = color;
    }
}