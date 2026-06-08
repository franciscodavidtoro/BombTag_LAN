using UnityEngine;
using Mirror;

public class BombPasser : NetworkBehaviour
{
    [Header("Referencias Visuales y Animación")]
    public GameObject bombaVisual;
    public Animator anim;
    public NetworkAnimator netAnim;

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetBombVisual(false);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (GameManager.instance != null)
        {
            GameManager.instance.AddPlayer(this.gameObject);
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (GameManager.instance != null)
        {
            GameManager.instance.RemovePlayer(this.gameObject);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || GameManager.instance == null || !GameManager.instance.gameActive) return;

        if (other.CompareTag("Player"))
        {
            if (GameManager.instance.currentCarrier == this.gameObject)
            {
                // Disparamos la animación del golpe con el nombre exacto de la Fase 2
                if (netAnim != null)
                {
                    netAnim.SetTrigger("TrigPush");
                }

                GameManager.instance.PassBomb(other.gameObject);
            }
        }
    }

    public void SetBombVisual(bool isCarrying)
    {
        if (bombaVisual != null)
        {
            bombaVisual.SetActive(isCarrying);
        }

        // Activamos la animación de pánico con el nombre exacto de la Fase 2
        if (anim != null)
        {
            anim.SetBool("HasBomb", isCarrying);
        }
    }
}