using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // los header son para que se vea bonito en el editor y sea mas facil de entender a que pertenece cada grupo de variables
    
    // tambien es bueno que las variables que no vayas a modificar desde el inspector las hagas privadas

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;
    private BounceController bounceController;
    private BoxCollider2D boxCollider;

    private Vector2 centerBoxCollider;
    private Vector2 extendsBoxCollider;

    // direccion del movimiento al tocar una tecla
    private Vector2 direction;

    [Header("Player Config")]
    [SerializeField] private float speed = 6.0f;
    [SerializeField] private float baseGravityScale = 3.0f;
    private float gravityScale = 0.0f;
    private float velocidadY = 0.0f;


    [Header("Jump Config")]
    [SerializeField] private float jumpForce = 13.0f;
    [SerializeField] private float maxCoyoteTime = 0.075f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float toleranciaVelocidad = 6.0f;
    [SerializeField] private float minJumpVelocity = 5.0f;
    private float coyoteTime = 0.0f;
    private float jumpBuffer = 0.0f;
    //esto es privado porque se cambiara por codigo y no es necesario en el editor
    private bool estaEnSuelo = false;

    [Header("ray Config")]
    //esta seria la mascara que detectaran los raycast, en este caso seria el suelo o ground
    public LayerMask mask;
    [Header("Ray For Ground Config")]
    [SerializeField] private float rayGroundDistance = 0.1f;

    [Header("Ray For Top Config")]
    [SerializeField] private float rayTopDistance = 0.2f;
    [SerializeField] private float distanceMidleTopRay = 0.1f;
    [SerializeField] private float distanceTopRay = 0.35f;
    [SerializeField] private float alturaTopRay = 1.5f;

    [Header("Correcion Esquinas Config")]
    [SerializeField] private float fuerzaCorreccionEsquina = 0.05f;

    [Header("Wall Jump Config")]
    [SerializeField] private float fuerzaWallJump = 10.0f;
    [SerializeField] private float velocidadEnPared = -1.0f;
    [SerializeField] private float velocidadReduccionVelocidad = 10.0f;
    [SerializeField] private float altoDetectorWallJump = 1.35f;
    [SerializeField] private float anchoDetectorWallJump = 0.05f;
    [SerializeField] private float desaceleracionWallJump = 10.0f;
    private bool haciendoWallJump = false;
    private bool wallJumpDerIzq = false;
    private bool puedeCancelarExtraVelocity = false;
    private float extraVelocityX = 0.0f;

    void Start()
    {
        // se usa getcomponent para buscar el componente que quieras en el objeto actual y que luego lo puedas usar
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        bounceController = GetComponent<BounceController>();
        boxCollider = GetComponent<BoxCollider2D>();
    
        gravityScale = baseGravityScale;
    }

    void GetBounds()
    {
        centerBoxCollider = boxCollider.bounds.center;
        extendsBoxCollider = boxCollider.bounds.extents;
    }

    //se usa fixed update porque es mejor para trabajar con fisicas en general
    void FixedUpdate()
    {
        velocidadY -= 9.81f * Time.deltaTime * gravityScale;

        GetBounds();

        AnimationController();

        DetectGround();

        CoyoteTime();

        DetectTop();

        WallJumpController();

        JumpBuffer();

        //cambia la velocidad del jugador a la de la direccion multiplicada por la velocidad y mantenemos la vertical
        rb.linearVelocity = new Vector2(direction.x * speed + extraVelocityX + bounceController.velocidadRepeler.x,velocidadY + bounceController.velocidadRepeler.y);
    }

    private void DetectGround()
    {
        if (rb.linearVelocityY != 0) estaEnSuelo = false;

        // si la cosa que hara el if es solo una entonces es mas practico ponerlo al costado en vez de poner las llaves y desperdiciar 3 lineas
        // en este caso si ya esta en el suelo no es necesario volverlo a comprobar de momento

        // como dato el return sirve para salir de la funcion sin continuar con el codigo que esta debajo por si quieres ahorrar recursos y eso

        // se lanzan 3 rayos para detectar si el jugador esta en el suelo
        RaycastHit2D hitMidle = Physics2D.Raycast(transform.position, Vector2.down, rayGroundDistance, mask);
        RaycastHit2D hitLeft = Physics2D.Raycast(centerBoxCollider - extendsBoxCollider, Vector2.down,rayGroundDistance,mask);
        RaycastHit2D hitRigth = Physics2D.Raycast(centerBoxCollider + new Vector2(extendsBoxCollider.x, -extendsBoxCollider.y), Vector2.down, rayGroundDistance, mask);
        
        // si alguno de estos choca significa que esta en el suelo
        if (hitLeft || hitMidle || hitRigth)
        {
            estaEnSuelo = true;
            coyoteTime = maxCoyoteTime;
            velocidadY = velocidadY < 0.0f ? 0.0f : velocidadY;
        }
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
        else
        {
            velocidadY = velocidadY > 0.0f ? 0.0f : velocidadY;
        }

        //debug para ver si los rayos se lazan bien y poder visualizarlos (es una tonteria porque al final son tan pequeños que ni se ven pero por si acaso :p)
        Debug.DrawLine(new Vector2(transform.position.x - distanceMidleTopRay, transform.position.y + alturaTopRay), hitMidleLeft ? hitMidleLeft.point : new Vector2(transform.position.x - distanceMidleTopRay, transform.position.y + alturaTopRay + rayTopDistance), Color.yellow);
        Debug.DrawLine(new Vector2(transform.position.x + distanceMidleTopRay, transform.position.y + alturaTopRay), hitMidleLeft ? hitMidleLeft.point : new Vector2(transform.position.x + distanceMidleTopRay, transform.position.y + alturaTopRay + rayTopDistance), Color.yellow);
        Debug.DrawLine(new Vector2(transform.position.x - distanceTopRay, transform.position.y + alturaTopRay), hitLeft ? hitLeft.point : new Vector2(transform.position.x - distanceTopRay, transform.position.y + alturaTopRay + rayTopDistance), Color.yellow);
        Debug.DrawLine(new Vector2(transform.position.x + distanceTopRay, transform.position.y + alturaTopRay), hitRigth ? hitRigth.point : new Vector2(transform.position.x + distanceTopRay, transform.position.y + alturaTopRay + rayTopDistance), Color.yellow);
    }

    private void OnDrawGizmos()
    {
        if (boxCollider != null)
        {

            Gizmos.DrawLine(transform.position, transform.position - new Vector3(0.0f, rayGroundDistance, 0.0f));
            Gizmos.DrawLine(centerBoxCollider - extendsBoxCollider, centerBoxCollider - extendsBoxCollider - new Vector2(0.0f, rayGroundDistance));
            Gizmos.DrawLine(centerBoxCollider + new Vector2(extendsBoxCollider.x, -extendsBoxCollider.y), centerBoxCollider + new Vector2(extendsBoxCollider.x, -extendsBoxCollider.y) - new Vector2(0.0f, rayGroundDistance));

            //gizmo de walljump
            Gizmos.DrawWireCube(new Vector2(transform.position.x - extendsBoxCollider.x , centerBoxCollider.y), new Vector2(anchoDetectorWallJump, altoDetectorWallJump));
            Gizmos.DrawWireCube(new Vector2(transform.position.x + extendsBoxCollider.x, centerBoxCollider.y), new Vector2(anchoDetectorWallJump, altoDetectorWallJump));
        }
    }

    private void AnimationController()
    {
        animator.SetBool("EstaMoviendose", rb.linearVelocity.x != 0);
    }

    public void OnMove(InputValue input)
    {
        direction = input.Get<Vector2>();

        //esto sirve para girar el sprite del jugador y se comprueba si es diferente a 0 que
        //seria si esta quieto porque si no su escala seria 0 y no se veria

        if (direction.x != 0)
        {
            sprite.flipX = direction.x < 0;
        }
    }

    private void WallJumpController()
    {
        Vector2 boxSize = new(anchoDetectorWallJump, altoDetectorWallJump);

        Collider2D hitLeft = Physics2D.OverlapBox(new(transform.position.x - extendsBoxCollider.x, centerBoxCollider.y), boxSize, 0, mask);
        Collider2D hitRigth = Physics2D.OverlapBox(new(transform.position.x + extendsBoxCollider.x, centerBoxCollider.y), boxSize, 0, mask);

        extraVelocityX = Mathf.Lerp(extraVelocityX, 0.0f, desaceleracionWallJump * Time.deltaTime);

        if (estaEnSuelo && extraVelocityX != 0) extraVelocityX = 0.0f;

        bool moviendoHaciaPared = wallJumpDerIzq ? 
            (rb.linearVelocity.x >= 0 && direction.x == 1) :
            (rb.linearVelocity.x <= 0 && direction.x ==-1) ;

        if (moviendoHaciaPared && !puedeCancelarExtraVelocity && !haciendoWallJump)
        {
            puedeCancelarExtraVelocity = true;
            extraVelocityX = 0.0f;
        }

        bool hayPared = (hitLeft || hitRigth);

        if (!hayPared)
        {
            if (coyoteTime <= 0) haciendoWallJump = false;

            if (!haciendoWallJump || velocidadY > 0.0f) gravityScale = baseGravityScale;
        }

        if (hayPared && direction.x != 0)
        {
            wallJumpDerIzq = hitRigth ? true : false;
            puedeCancelarExtraVelocity = false;
            haciendoWallJump = true;
            coyoteTime = maxCoyoteTime;
        }

        //wall slide controll
        if (haciendoWallJump && velocidadY < 0.0f)
        {
            gravityScale = 0.0f;
            velocidadY = Mathf.Lerp(velocidadY, velocidadEnPared, velocidadReduccionVelocidad * Time.deltaTime);
        }
    }

    private void JumpBuffer()
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

    private void CoyoteTime()
    {
        if (!estaEnSuelo) coyoteTime = MathF.Max(coyoteTime - Time.deltaTime, 0);
    }

    private void Jump()
    {
        velocidadY = jumpForce;
        coyoteTime = 0;
        jumpBuffer = 0;

        if (haciendoWallJump)
        {
            extraVelocityX = wallJumpDerIzq ? -fuerzaWallJump : fuerzaWallJump;
        }
    }

    public void OnJump(InputValue input)
    {
        if (input.isPressed)
        {
            if (estaEnSuelo || coyoteTime > 0) Jump();
            else jumpBuffer = jumpBufferTime;
        }
        else
        {
            velocidadY = velocidadY < toleranciaVelocidad ? velocidadY : minJumpVelocity;
        }
    }

    //esto son punteros, sirve para acceder a las variables desde otros scripts sin hacerlas publicas
    //y que luego aparezcan en el inspector cuando no las necesitamos ahi
    public bool EstaEnSuelo => estaEnSuelo;
    public Rigidbody2D PlayerRb => rb;

    public float VelocidadY
    {
        set
        {
            velocidadY = value;
        }
    }


}
