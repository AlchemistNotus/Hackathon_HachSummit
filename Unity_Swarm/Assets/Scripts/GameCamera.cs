using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GameCamera : MonoBehaviour
{
    public static GameCamera Instance;

    [Header("Position")]
    public Vector3 MinOffsetFromTarget;
    public Vector3 MaxOffsetFromTarget;
    public float Rotate;

    [Header("Fly speed")]
    public float MinSpeed;
    public float MaxSpeed;
    public float DistanceForMaxSpeed;
    public AnimationCurve SpeedByDistance;

    [Header("WorldParameters")]
    public float WorldPlaneHeight;

    [Header("DebugParameters")]
    public Transform Target;

    
    public Camera Camera;

    public enum MainCameraStates
    {
        FollowCharacters,
        FollowTargetTransform,
        Stay
    }

    public MainCameraStates State;

    private float _zoomK;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            if (Target != null)
                UpdateCameraMovements(Target.position);

            return;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < -0.01)
        {
            _zoomK = Mathf.Clamp(_zoomK + 0.1f, 0, 1);
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0.01)
        {
            _zoomK = Mathf.Clamp(_zoomK - 0.1f, 0, 1);
        }

        switch (State)
        {
            case MainCameraStates.FollowCharacters:
                FollowCharacters();
                break;
            case  MainCameraStates.FollowTargetTransform:
                UpdateCameraMovements(Target.position);
                break;
        }

        RayCastPlayerCickes();
    }

    private void RayCastPlayerCickes()
    {
        if (GameController.Instance.State != GameController.GameStates.Play)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, WorldPlaneHeight, 0));

            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            Vector3 positionOnPlane = Vector3.zero;
            float rayDistance;
            if (groundPlane.Raycast(ray, out rayDistance))
            {
                positionOnPlane = ray.GetPoint(rayDistance);
                ClickedOnGround(positionOnPlane);
            }
        }
    }

    void ClickedOnGround(Vector3 clickPosition)
    {
        GameController.Instance.ClickedOnGround(clickPosition);
    }

    void FollowCharacters()
    {
        if (GameController.Instance.State != GameController.GameStates.Play)
            return;

        Vector3 positionSum = Vector3.zero;
        var allControlledCharacters = GameController.GetAllControledCharacters();
        foreach (var controledCharacter in allControlledCharacters)
        {
            positionSum += controledCharacter.transform.position;
        }

        Vector3 targetPosition = positionSum;

        if (allControlledCharacters.Count > 0)
            targetPosition /= allControlledCharacters.Count;

        UpdateCameraMovements(targetPosition);
    }

    void UpdateCameraMovements(Vector3 targetVector3)
    {
        Vector3 OffsetFromTarget = Vector3.Lerp(MinOffsetFromTarget, MaxOffsetFromTarget, _zoomK);
        Vector3 rotatedOffsetFromTarhet = OffsetFromTarget;
        rotatedOffsetFromTarhet.z = OffsetFromTarget.x * Mathf.Sin(Rotate * Mathf.Rad2Deg);
        rotatedOffsetFromTarhet.x = OffsetFromTarget.x * Mathf.Cos(Rotate * Mathf.Rad2Deg);

        Vector3 targetCameraPosition = targetVector3 + rotatedOffsetFromTarhet;
        Vector3 relativePos = targetVector3 - targetCameraPosition;
        Quaternion targetCameraRotation = Quaternion.LookRotation(relativePos);
        transform.rotation = targetCameraRotation;

        if (Application.isPlaying)
        {
            Vector3 distanceToTarget = transform.position - targetCameraPosition;
            float normalizedDistance = Mathf.Clamp01( distanceToTarget.magnitude/DistanceForMaxSpeed );
            float flySpeed = MinSpeed + (MaxSpeed - MinSpeed)*SpeedByDistance.Evaluate(normalizedDistance);

            float distanceForFrame = Time.deltaTime * flySpeed;
            transform.position = Vector3.MoveTowards(transform.position, targetCameraPosition, distanceForFrame);
        }
        else
        {
            transform.position = targetCameraPosition;
        }
    }
}
