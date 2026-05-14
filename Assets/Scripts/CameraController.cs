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
    [Header("Shake Config")]
    public float forceShake = 1.0f;
    public float timeShake = 1.0f;
    public float velocityShake = 10.0f;
    public float updateShakeEvery = 0.1f;
    public float timeUpdateShake = 0.1f;
    private Vector2 targetShake;
    private Vector2 posicionShake;
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

    private void LateUpdate()
    {
        UpdateTarget();

        transform.position = new Vector3(finalPosition.x + posicionShake.x , finalPosition.y + posicionShake.y , -1.0f);
    }

    private void UpdateTarget()
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

        ControllShake();

        FollowX();
        FollowY();
    }

    private void UpdateFollowX(float x)
    {
        if (lockX) return;

        if (useLimits)
        {
            followPosition.x = Mathf.Clamp(x, limitXLeft, limitXRigth);
            return;
        }

        followPosition.x = x;
    }
    private void UpdateFollowY(float y)
    {
        if (lockY) return;

        if (useLimits)
        {
            followPosition.y = Mathf.Clamp(y, limitYDown, limitYUp);
            return;
        }

        followPosition.y = y;
    }

    private void FollowX()
    {

        float objectiveX = followPosition.x + offsetDeSeguimiento.x;

        finalPosition.x = Mathf.Lerp(finalPosition.x, objectiveX, (objectiveX < finalPosition.x ? velocityXIzquierda : velocityXDerecha) * Time.deltaTime);
    

    }

    private void FollowY()
    {

        float objectiveY = followPosition.y + offsetDeSeguimiento.y;

        finalPosition.y = Mathf.Lerp(finalPosition.y, objectiveY, (objectiveY < finalPosition.y ? velocityYInferior : velocityYSuperior) * Time.deltaTime);
        

    }

    private void ControllShake()
    {
        timeShake = Mathf.Max(timeShake - Time.deltaTime,0.0f);

        if (timeShake > 0)
        {
            timeUpdateShake = Mathf.Max(timeUpdateShake - Time.deltaTime, 0.0f);

            if (timeUpdateShake <= 0)
            {
                timeUpdateShake = updateShakeEvery;

                targetShake = new Vector2(Random.Range(-forceShake,forceShake), Random.Range(-forceShake, forceShake));
            }
        }
        else
        {
            targetShake = Vector2.zero;
        }

        posicionShake = Vector2.Lerp(posicionShake,targetShake,velocityShake * Time.deltaTime);

    }

    public void Shake(float ForceShake, float DurationShake,float UpdateEvery)
    {
        forceShake = ForceShake;
        timeShake = DurationShake;
        updateShakeEvery = UpdateEvery;
        Shake(forceShake, timeShake, updateShakeEvery, velocityShake);
    }
    public void Shake(float ForceShake, float DurationShake, float UpdateEvery, float VelocityShake)
    {
        timeUpdateShake = 0.0f;

        forceShake = ForceShake;
        timeShake = DurationShake;
        velocityShake = VelocityShake;
        updateShakeEvery = UpdateEvery;
    }
}
