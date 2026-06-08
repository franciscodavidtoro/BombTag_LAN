using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [Header("Estado del Juego")]
    [SyncVar] public bool gameActive = false;
    [SyncVar] public float timer = 15f;

    [Header("Ajustes de Reglas")]
    public float passCooldown = 1.5f; // Segundos de inmunidad tras pasar la bomba
    private float lastPassTime = 0f;  // Registro interno de tiempo

    // SyncVar con un 'hook' que se dispara en todos los clientes cuando cambia el portador
    [SyncVar(hook = nameof(OnBombCarrierChanged))]
    public GameObject currentCarrier;

    // Lista exclusiva del servidor para rastrear jugadores vivos
    public List<GameObject> activePlayers = new List<GameObject>();

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Update()
    {
        // Solo el Servidor administra el temporizador
        if (!isServer || !gameActive) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            ExplodeBomb();
        }
    }

    [Server]
    public void AddPlayer(GameObject player)
    {
        activePlayers.Add(player);
        // Inicio de ronda automatizado con mínimo 2 usuarios
        if (activePlayers.Count >= 2 && !gameActive)
        {
            Invoke(nameof(StartRound), 3f); // Pequeña pausa antes de iniciar
        }
    }

    [Server]
    public void RemovePlayer(GameObject player)
    {
        activePlayers.Remove(player);
        CheckWinCondition();
    }

    [Server]
    public void StartRound()
    {
        if (activePlayers.Count < 2) return;

        timer = 15f;
        gameActive = true;

        // Selección aleatoria del portador de la bomba
        int randomIndex = Random.Range(0, activePlayers.Count);
        currentCarrier = activePlayers[randomIndex];

        RpcGameMessage("¡La ronda ha comenzado! ¡Huye de la bomba!");
    }

    // Método autoritativo de traspaso
    [Server]
    public void PassBomb(GameObject newCarrier)
    {
        if (!gameActive) return;

        // BARRERA DE COOLDOWN: Previene el ping-pong instantáneo
        if (Time.time < lastPassTime + passCooldown) return;

        lastPassTime = Time.time; // Guardamos el momento exacto del pase
        currentCarrier = newCarrier;
        timer = 15f;
        RpcGameMessage("¡La bomba ha cambiado de dueño!");
    }

    [Server]
    private void ExplodeBomb()
    {
        gameActive = false;
        GameObject loser = currentCarrier;
        currentCarrier = null; // Quitamos la bomba para limpiar el estado

        // Notificar a los clientes para efectos visuales
        RpcExplosionEffect(loser.transform.position);

        // Eliminación del jugador
        activePlayers.Remove(loser);
        NetworkServer.Destroy(loser);

        CheckWinCondition();
    }

    [Server]
    private void CheckWinCondition()
    {
        if (activePlayers.Count == 1)
        {
            RpcGameMessage($"¡Ganador: {activePlayers[0].name}!");
            // Reiniciar juego si hay más jugadores o esperar a que se unan
            Invoke(nameof(StartRound), 5f);
        }
    }

    // El Hook: Se ejecuta localmente en cada cliente cuando 'currentCarrier' cambia en el servidor
    private void OnBombCarrierChanged(GameObject oldCarrier, GameObject newCarrier)
    {
        if (oldCarrier != null)
        {
            oldCarrier.GetComponent<BombPasser>().SetBombVisual(false);
        }

        if (newCarrier != null)
        {
            newCarrier.GetComponent<BombPasser>().SetBombVisual(true);
        }
    }

    // RPCs: Del Servidor hacia todos los Clientes
    [ClientRpc]
    private void RpcExplosionEffect(Vector3 position)
    {
        // Aquí puedes instanciar partículas de explosión en un futuro
        Debug.Log($"¡BOOOOM! Explosión ocurrida en {position}");
    }

    [ClientRpc]
    private void RpcGameMessage(string message)
    {
        // En el futuro, esto actualizará el UI de la pantalla
        Debug.Log($"[JUEGO]: {message}");
    }
}