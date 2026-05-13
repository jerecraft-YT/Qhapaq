using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector2 finalPosition;
    private Vector2 followPosition;
    [Header("Base Config")]
    public Transform targetPosition;
    public Vector2 offsetDeSeguimiento;
    public bool lockX;
    public bool lockY;
    public bool useLimits = true;
    public float limitXLeft = 0.0f;
    public float limitXRigth = 100.0f;
    public float limitYDown = 0.0f;
    public float limitYUp = 100.0f;
    [Header("ControlDeVelocidad")]
    public float velocityXIzquierda = 5.0f;
    public float velocityXDerecha = 5.0f;
    public float velocityYSuperior = 5.0f;
    public float velocityYInferior = 5.0f;

    //esto es solo si seguira a un jugador
    [Header("Player Config")]
    public PlayerController player;
    [Header("seguir en Y si?")]
    public bool EstaEnSuelo = true;
    public bool EstaCayendo = false;
    public bool EstaSaltando = false;
    public bool PosicionEsMenorQueLaCamara = true;
    public bool PosicionEsMayorQueLaCamara = false;

    private void Start()
    {
        followPosition = targetPosition.position;
        finalPosition = followPosition + offsetDeSeguimiento;
    }

    void LateUpdate()
    {
        UpdateTarget();

        transform.position = new Vector3(finalPosition.x , finalPosition.y , -1.0f);
    }

    void UpdateTarget()
    {
        Vector2 posicionTarget = targetPosition.position;

        UpdateFollowX(posicionTarget.x);

        if (player != null)
        {
            if (player.EstaEnSuelo && EstaEnSuelo)
            {
                UpdateFollowY(posicionTarget.y);
            }
            if (player.playerRb.linearVelocity.y > 0 && EstaSaltando)
            {
                UpdateFollowY(posicionTarget.y);
            }
            if (player.playerRb.linearVelocity.y < 0 && EstaCayendo)
            {
                UpdateFollowY(posicionTarget.y);
            }
            if (posicionTarget.y + offsetDeSeguimiento.y > transform.position.y && PosicionEsMayorQueLaCamara)
            {
                UpdateFollowY(posicionTarget.y);
            }
            if (posicionTarget.y + offsetDeSeguimiento.y < transform.position.y && PosicionEsMenorQueLaCamara)
            {
                UpdateFollowY(posicionTarget.y);
            }
        }
        else
        {
            UpdateFollowY(posicionTarget.y);
        }

        FollowX();
        FollowY();
    }

    void UpdateFollowX(float x)
    {
        if (useLimits)
        {
            followPosition.x = Mathf.Clamp(x, limitXLeft, limitXRigth);
            return;
        }

        followPosition.x = x;
    }
    void UpdateFollowY(float y)
    {
        if (useLimits)
        {
            followPosition.y = Mathf.Clamp(y, limitYDown, limitYUp);
            return;
        }

        followPosition.y = y;
    }

    void FollowX()
    {
        if (lockX) return;

        float objectiveX = followPosition.x + offsetDeSeguimiento.x;

        finalPosition.x = Mathf.Lerp(finalPosition.x, objectiveX, (objectiveX < finalPosition.x ? velocityXIzquierda : velocityXDerecha) * Time.deltaTime);
    }

    void FollowY()
    {
        if (lockY) return;

        float objectiveY = followPosition.y + offsetDeSeguimiento.y;

        finalPosition.y = Mathf.Lerp(finalPosition.y, objectiveY, (objectiveY < finalPosition.y ? velocityYInferior : velocityYSuperior) * Time.deltaTime);
    }
}
