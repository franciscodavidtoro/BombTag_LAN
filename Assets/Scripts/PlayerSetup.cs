using UnityEngine;
using Mirror; // Obligatorio para trabajar con componentes de red

// Cambiamos MonoBehaviour por NetworkBehaviour para acceder a isLocalPlayer
public class PlayerSetup : NetworkBehaviour
{
    [Header("Aislamiento de Red")]
    [Tooltip("Coloca aquí los scripts (movimiento, combate) y cámaras que NO deben activarse en clones de otros jugadores.")]
    public Behaviour[] componentsToDisable;

    void Start()
    {
        // Si el objeto instanciado NO es el jugador que controla el cliente actual
        if (!isLocalPlayer)
        {
            DisableNonLocalComponents();
        }
    }

    private void DisableNonLocalComponents()
    {
        // Recorremos el arreglo apagando cada componente asignado
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            if (componentsToDisable[i] != null)
            {
                componentsToDisable[i].enabled = false;
            }
        }
    }
}