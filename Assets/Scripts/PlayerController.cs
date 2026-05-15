using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // los header son para que se vea bonito en el editor y sea mas facil de entender a que pertenece cada grupo de variables
    
    // tambien es bueno que las variables que no vayas a modificar desde el inspector las hagas privadas

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;

    // direccion del movimiento al tocar una tecla
    private Vector2 direction;

    [Header("Player Config")]
    [SerializeField] private float speed = 6.0f;
    [SerializeField] private float baseGravityScale = 3.0f;

    [Header("Jump Config")]
    [SerializeField] private float jumpForce = 13.0f;
    [SerializeField] private float maxCoyoteTime = 0.075f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    private float coyoteTime = 0.0f;
    private float jumpBuffer = 0.0f;
    //esto es privado porque se cambiara por codigo y no es necesario en el editor
    private bool estaEnSuelo = false;

    [Header("ray Config")]
    //esta seria la mascara que detectaran los raycast, en este caso seria el suelo o ground
    public LayerMask mask;
    [Header("Ray For Ground Config")]
    public float rayGroundDistance = 0.1f;
    public float distanceGroundRay = 0.35f;

    [Header("Ray For Top Config")]
    public float rayTopDistance = 0.2f;
    public float distanceMidleTopRay = 0.1f;
    public float distanceTopRay = 0.35f;
    public float alturaTopRay = 1.5f;

    [Header("Correcion Esquinas Config")]
    public float fuerzaCorreccionEsquina = 0.05f;

    [Header("Wall Jump Config")]
    public float fuerzaWallJump = 10.0f;
    public float velocidadEnPared = 2.0f;
    public float velocidadReduccionVelocidad = 10.0f;
    private bool haciendoWallJump = false;
    public float altoDetectorWallJump = 1.35f;
    public float anchoDetectorWallJump = 0.05f;
    public float detectorWallJumpDistance = 0.35f;
    public float alturaDetectorWallJump = 0.69f;
    private bool wallJumpDerIzq = false;
    private float extraVelocityX = 0.0f;
    public float desaceleracionWallJump = 10.0f;

    void Start()
    {
        // se usa getcomponent para buscar el componente que quieras en el objeto actual y que luego lo puedas usar
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    //se usa fixed update porque es mejor para trabajar con fisicas en general
    void FixedUpdate()
    {
        AnimationController();

        DetectGround();

        CoyoteTime();

        DetectTop();

        WallJumpController();

        JumpBuffer();

        //cambia la velocidad del jugador a la de la direccion multiplicada por la velocidad y mantenemos la vertical
        rb.linearVelocity = new Vector2(direction.x * speed + extraVelocityX, rb.linearVelocity.y);
    }

    private void CoyoteTime()
    {
        if (!estaEnSuelo) coyoteTime = MathF.Max(coyoteTime - Time.deltaTime, 0);
    }

    private void DetectGround()
    {
        if (rb.linearVelocityY != 0) estaEnSuelo = false;

        // si la cosa que hara el if es solo una entonces es mas practico ponerlo al costado en vez de poner las llaves y desperdiciar 3 lineas
        // en este caso si ya esta en el suelo no es necesario volverlo a comprobar de momento
        if (estaEnSuelo) return;
        // como dato el return sirve para salir de la funcion sin continuar con el codigo que esta debajo por si quieres ahorrar recursos y eso

        // se lanzan 3 rayos para detectar si el jugador esta en el suelo
        RaycastHit2D hitMidle = Physics2D.Raycast(transform.position, Vector2.down, rayGroundDistance, mask);
        RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(transform.position.x - distanceGroundRay, transform.position.y),Vector2.down,rayGroundDistance,mask);
        RaycastHit2D hitRigth = Physics2D.Raycast(new Vector2(transform.position.x + distanceGroundRay, transform.position.y), Vector2.down, rayGroundDistance, mask);
        
        // si alguno de estos choca significa que esta en el suelo
        if (hitLeft || hitMidle || hitRigth)
        {
            estaEnSuelo = true;
            coyoteTime = maxCoyoteTime;
        }

        //debug para ver si los rayos se lazan bien y poder visualizarlos (es una tonteria porque al final son tan pequeños que ni se ven pero por si acaso :p)
        Debug.DrawLine(transform.position, hitMidle ? hitMidle.point : transform.position - new Vector3(0.0f, rayGroundDistance, 0.0f), Color.yellow);
        Debug.DrawLine(new Vector2(transform.position.x - distanceGroundRay, transform.position.y), hitLeft ? hitLeft.point : new Vector2(transform.position.x - distanceGroundRay, transform.position.y) - new Vector2(0.0f, rayGroundDistance), Color.yellow);
        Debug.DrawLine(new Vector2(transform.position.x + distanceGroundRay, transform.position.y), hitRigth ? hitRigth.point : new Vector2(transform.position.x + distanceGroundRay, transform.position.y) - new Vector2(0.0f, rayGroundDistance), Color.yellow);
    }

    private void DetectTop()
    {
        if (estaEnSuelo || rb.linearVelocity.y < 0) return;

        RaycastHit2D hitMidleLeft = Physics2D.Raycast(new Vector2(transform.position.x - distanceMidleTopRay, transform.position.y + alturaTopRay), Vector2.up, rayTopDistance, mask);
        RaycastHit2D hitMidleRigth = Physics2D.Raycast(new Vector2(transform.position.x + distanceMidleTopRay, transform.position.y + alturaTopRay), Vector2.up, rayTopDistance, mask);
        RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(transform.position.x - distanceTopRay, transform.position.y + alturaTopRay), Vector2.up, rayTopDistance, mask);
        RaycastHit2D hitRigth = Physics2D.Raycast(new Vector2(transform.position.x + distanceTopRay, transform.position.y + alturaTopRay), Vector2.up, rayTopDistance, mask);
        
        if (!hitMidleLeft && !hitMidleRigth)
        {
            if (hitLeft && !hitRigth)
            {
                for (int i = 0; i < 10; i++)
                {
                    RaycastHit2D hitLeftTemp = Physics2D.Raycast(new Vector2(transform.position.x - distanceTopRay, transform.position.y + alturaTopRay), Vector2.up, rayTopDistance, mask);

                    if (hitLeftTemp)
                    {
                        transform.position = new Vector3(transform.position.x + fuerzaCorreccionEsquina, transform.position.y, transform.position.z);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (!hitLeft && hitRigth)
            {
                for (int i = 0; i < 10; i++)
                {
                    RaycastHit2D hitRigthTemp = Physics2D.Raycast(new Vector2(transform.position.x + distanceTopRay, transform.position.y + alturaTopRay), Vector2.up, rayTopDistance, mask);

                    if (hitRigthTemp)
                    {
                        transform.position = new Vector3(transform.position.x - fuerzaCorreccionEsquina, transform.position.y, transform.position.z);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        //debug para ver si los rayos se lazan bien y poder visualizarlos (es una tonteria porque al final son tan pequeños que ni se ven pero por si acaso :p)
        Debug.DrawLine(new Vector2(transform.position.x - distanceMidleTopRay, transform.position.y + alturaTopRay), hitMidleLeft ? hitMidleLeft.point : new Vector2(transform.position.x - distanceMidleTopRay, transform.position.y + alturaTopRay + rayTopDistance), Color.yellow);
        Debug.DrawLine(new Vector2(transform.position.x + distanceMidleTopRay, transform.position.y + alturaTopRay), hitMidleLeft ? hitMidleLeft.point : new Vector2(transform.position.x + distanceMidleTopRay, transform.position.y + alturaTopRay + rayTopDistance), Color.yellow);
        Debug.DrawLine(new Vector2(transform.position.x - distanceTopRay, transform.position.y + alturaTopRay), hitLeft ? hitLeft.point : new Vector2(transform.position.x - distanceTopRay, transform.position.y + alturaTopRay + rayTopDistance), Color.yellow);
        Debug.DrawLine(new Vector2(transform.position.x + distanceTopRay, transform.position.y + alturaTopRay), hitRigth ? hitRigth.point : new Vector2(transform.position.x + distanceTopRay, transform.position.y + alturaTopRay + rayTopDistance), Color.yellow);
    }

    private void WallJumpController()
    {
        extraVelocityX = Mathf.Lerp(extraVelocityX, 0.0f, desaceleracionWallJump * Time.deltaTime);

        if (estaEnSuelo) extraVelocityX = 0.0f;

        RaycastHit2D hitLeft = Physics2D.BoxCast(new Vector2(transform.position.x - detectorWallJumpDistance, transform.position.y + alturaDetectorWallJump), new Vector2(anchoDetectorWallJump,altoDetectorWallJump),0.0f,Vector2.left,0.0f,mask);
        RaycastHit2D hitRigth = Physics2D.BoxCast(new Vector2(transform.position.x + detectorWallJumpDistance, transform.position.y + alturaDetectorWallJump), new Vector2(anchoDetectorWallJump, altoDetectorWallJump), 0.0f, Vector2.right, 0.0f, mask);

        if ((hitLeft || hitRigth) && rb.linearVelocity.y < 0)
        {
            wallJumpDerIzq = hitRigth ? true : false;

            haciendoWallJump = true;
            estaEnSuelo = true;
            rb.gravityScale = 0.0f;
        }
        else
        {
            haciendoWallJump = false;
            rb.gravityScale = baseGravityScale;
        }

        if (haciendoWallJump)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity,new Vector2(rb.linearVelocity.x, velocidadEnPared), velocidadReduccionVelocidad * Time.deltaTime);
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector2(transform.position.x - detectorWallJumpDistance, transform.position.y + alturaDetectorWallJump), new Vector2(anchoDetectorWallJump, altoDetectorWallJump));
        Gizmos.DrawWireCube(new Vector2(transform.position.x + detectorWallJumpDistance, transform.position.y + alturaDetectorWallJump), new Vector2(anchoDetectorWallJump, altoDetectorWallJump));
    }

    private void AnimationController()
    {
        animator.SetBool("EstaMoviendose", rb.linearVelocity.x != 0);
    }

    private void OnMove(InputValue input)
    {
        direction = input.Get<Vector2>();

        //esto sirve para girar el sprite del jugador y se comprueba si es diferente a 0 que
        //seria si esta quieto porque si no su escala seria 0 y no se veria

        if (direction.x != 0)
        {
            sprite.flipX = direction.x < 0;
        }
    }

    void JumpBuffer()
    {
        if (jumpBuffer > 0)
        {
            jumpBuffer = MathF.Max(jumpBuffer - Time.deltaTime, 0);

            if (estaEnSuelo)
            {
                Jump();
            }
        }
    }

    void Jump()
    {
        if (haciendoWallJump) extraVelocityX = wallJumpDerIzq ? -fuerzaWallJump : fuerzaWallJump;
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        //rb.AddForce(Vector2.up * jumpForce);
        coyoteTime = 0;
        jumpBuffer = 0;
    }

    void OnJump()
    {
        if (estaEnSuelo || coyoteTime > 0)
        {
            Jump();
        }
        else
        {
            jumpBuffer = jumpBufferTime;
        }
    }

    //esto son punteros, sirve para acceder a las variables desde otros scripts sin hacerlas publicas
    //y que luego aparezcan en el inspector cuando no las necesitamos ahi
    public bool EstaEnSuelo => estaEnSuelo;
    public Rigidbody2D playerRb => rb;
}
