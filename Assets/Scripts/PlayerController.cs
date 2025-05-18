using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int maxJumps = 2;

    public Transform visualTransform;
    private Rigidbody2D rb;
    private Animator animator;

    private int jumpCount = 0;
    private float horizontalInput = 0f;
    private bool isGrounded = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
            isGrounded = false;
        }

        // Actualizar animaciones
        animator.SetBool("isRun", horizontalInput != 0);//&& isGrounded); // Correr solo si está en el suelo
        animator.SetBool("isJump", !isGrounded);
    }

    void FixedUpdate()
    {
        // Movimiento horizontal
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        if (horizontalInput != 0)
        {
            // Ajustar la dirección del sprite
            visualTransform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Restaurar saltos al tocar el suelo
        if (collision.gameObject.CompareTag("Tilemap"))
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Detectar cuando el personaje deja de estar en el suelo
        if (collision.gameObject.CompareTag("Tilemap"))
        {
            isGrounded = false;
        }
    }
}
