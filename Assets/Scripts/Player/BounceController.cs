using UnityEngine;

public class BounceController : MonoBehaviour
{
    private PlayerController playerController;
    public float fuerzaX = 60.0f;
    public float fuerzaY = 15.0f;
    public Vector2 velocidadRepeler = Vector2.zero;
    public float desaceleracion = 9.81f;
    public float offsetY = 0.6f;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }
    
    private void Update()
    {
        velocidadRepeler = velocidadRepeler != Vector2.zero ? Vector2.Lerp(velocidadRepeler, Vector2.zero, desaceleracion * Time.deltaTime) : velocidadRepeler;

        if (velocidadRepeler.x > -0.1f && velocidadRepeler.x < 0.1f && velocidadRepeler.x != 0.0f) velocidadRepeler = new Vector2(0.0f, 0.0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "pelotita")
        {
            Vector2 pelota = collision.transform.position;
  
            Vector2 valorNormalizado = new Vector2(transform.position.x - pelota.x, transform.position.y - pelota.y + offsetY);

            float angulo = Mathf.Atan2(valorNormalizado.y, valorNormalizado.x);

            velocidadRepeler = new Vector2(Mathf.Cos(angulo) * fuerzaX, Mathf.Sin(angulo) * fuerzaY);
            playerController.VelocidadY = velocidadRepeler.y;
        }
    }
}
