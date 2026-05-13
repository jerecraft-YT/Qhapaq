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

    [Header("player config")]
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float jumpForce = 10.0f;
    //esto es privado porque se cambiara por codigo y no es necesario en el editor
    private bool estaEnSuelo = false;

    [Header("rayForGroundConfig")]
    //esta seria la mascara que detectaran los raycast, en este caso seria el suelo o ground
    public LayerMask mask;
    public float rayGroundDistance = 0.1f;
    public float distanceGroundRay = 0.35f;

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
        //cambia la velocidad del jugador a la de la direccion multiplicada por la velocidad y mantenemos la vertical
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

        animationController();

        DetectGround();
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
        if (hitLeft || hitMidle || hitRigth) estaEnSuelo = true;

        //debug para ver si los rayos se lazan bien y poder visualizarlos (es una tonteria porque al final son tan pequeños que ni se ven pero por si acaso :p)
        Debug.DrawLine(transform.position, hitMidle ? hitMidle.point : transform.position - new Vector3(0.0f, rayGroundDistance, 0.0f), Color.yellow);
        Debug.DrawLine(new Vector2(transform.position.x - distanceGroundRay, transform.position.y), hitLeft ? hitLeft.point : new Vector2(transform.position.x - distanceGroundRay, transform.position.y) - new Vector2(0.0f, rayGroundDistance), Color.yellow);
        Debug.DrawLine(new Vector2(transform.position.x + distanceGroundRay, transform.position.y), hitRigth ? hitRigth.point : new Vector2(transform.position.x + distanceGroundRay, transform.position.y) - new Vector2(0.0f, rayGroundDistance), Color.yellow);
    }

    private void animationController()
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
            sprite.flipX = direction.x != 1;
        }
    }

    void OnJump()
    {
        if (estaEnSuelo) rb.AddForce(Vector2.up * jumpForce);
    }

    //esto son punteros, sirve para acceder a las variables desde otros scripts sin hacerlas publicas
    //y que luego aparezcan en el inspector cuando no las necesitamos ahi
    public bool EstaEnSuelo => estaEnSuelo;
    public Rigidbody2D playerRb => rb;
}
