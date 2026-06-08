using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerController : NetworkBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;

    private Rigidbody rb;
    private Animator animator;
    private Vector3 movementInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // PROTECCIÓN DE RED: Si este clon no es mi jugador local, ignoro sus inputs.
        if (!isLocalPlayer) return;

        // Leer inputs crudos del teclado para mayor precisión
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        movementInput = new Vector3(moveX, 0f, moveZ).normalized;

        // Actualizamos el Animator localmente usando el parámetro definido en la Fase 2
        animator.SetFloat("Speed", movementInput.magnitude);
    }

    void FixedUpdate()
    {
        // PROTECCIÓN DE RED: Las físicas solo las calcula el cliente dueño
        if (!isLocalPlayer) return;

        MovePlayer();
        RotatePlayer();
    }

    private void MovePlayer()
    {
        if (movementInput.magnitude >= 0.1f)
        {
            // Usamos MovePosition para mover el Rigidbody de forma segura
            Vector3 targetPosition = rb.position + movementInput * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
    }

    private void RotatePlayer()
    {
        if (movementInput.magnitude >= 0.1f)
        {
            // Rotación suave del personaje hacia la dirección del movimiento
            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}