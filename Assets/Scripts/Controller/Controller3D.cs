using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller3D : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private float skinWidth = 0.5f;
    [SerializeField] private float groundCheckDistance;

    [Header("Camera settings")]
    [SerializeField] private bool isFPS;
    [SerializeField] Vector3 cameraOffsetTPS;
    [SerializeField] Vector3 cameraOffsetFPS;
    [SerializeField] float smoothFactorQuick = 0.23f;
    [SerializeField] float smoothFactorSlow = 0.05f;
    [SerializeField] float radius = 1.0f;
    private Vector3 smoothedPos;
    private Vector3 topPos;
    private Vector2 cameraRotation;
    private Camera mainCam;
    private Vector3 offset;

    [Header("Physics")]
    private CapsuleCollider capsuleCollider;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] RaycastHit groundHit;
    private Vector3 upperPoint;
    private Vector3 lowerPoint;


    [Header("Input")]
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float minXrotation = 1f;
    [SerializeField] private float maxXrotation = 1f;
    private Vector3 input;
    private float rotationX;
    private float rotationY;

    [Header("States")]
    [SerializeField] private StateMachine stateMachine;
    [SerializeField] private State[] states;


    [Header("PhysicsBody")]
    private PhysicsBody body;
    public PhysicsBody Body => body;

    private void OnEnable()
    {
        cameraOffsetFPS = mainCam.transform.localPosition;
    }

    private void Awake()
    {
        stateMachine = new StateMachine(states, this);
        groundCheckDistance = 10 * skinWidth;

        capsuleCollider = GetComponent<CapsuleCollider>();
        body = GetComponent<PhysicsBody>();

        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        InputHandling();
        stateMachine.UpdateStates();
    }

    private void InputHandling()
    {
        input = Vector3.right * Input.GetAxisRaw("Horizontal") + Vector3.forward * Input.GetAxisRaw("Vertical");
        cameraRotation.y += Input.GetAxisRaw("MouseX") * mouseSensitivity;
        cameraRotation.x -= Input.GetAxisRaw("MouseY") * mouseSensitivity;
        cameraRotation.x = Mathf.Clamp(cameraRotation.x, minXrotation, maxXrotation);
        input = mainCam.transform.rotation * input;
        input = Vector3.ProjectOnPlane(input, Body.GroundHit.normal).normalized;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            isFPS = !isFPS;
        }
    }

    private void UpdateCamera()
    {
        mainCam.transform.rotation = Quaternion.Euler(cameraRotation.x, cameraRotation.y, 0.0f);

        if (isFPS)
        {
            mainCam.transform.localPosition = cameraOffsetFPS;
        }
        else{
            topPos = transform.position + upperPoint;
            offset = mainCam.transform.rotation * cameraOffsetTPS;
            Vector3 direction = offset - topPos;
            Debug.DrawLine(transform.position, transform.position + offset, Color.red);

            Vector3 newPos;
            if (Physics.SphereCast(transform.position + upperPoint, radius, direction.normalized, out RaycastHit hit, direction.magnitude))
            {
                newPos = cameraOffsetTPS.normalized * hit.distance;
            }
            else
            {
                newPos = cameraOffsetTPS;
            }

            smoothedPos = Vector3.Lerp(smoothedPos, newPos, Time.deltaTime * (hit.collider ? smoothFactorQuick : smoothFactorSlow));

            mainCam.transform.position = topPos + mainCam.transform.rotation * smoothedPos;
        }
    }

    private void LateUpdate()
    {
        UpdateCamera();
    }

    private RaycastHit IsGrounded()
    {
        Physics.CapsuleCast(upperPoint, lowerPoint, capsuleCollider.radius, Vector3.down, out RaycastHit hit, groundCheckDistance + skinWidth, obstacleLayer);
        return hit;
    }

    public Vector3 GetInput() { return input; }


    private void OnDrawGizmos()
    {
        if (mainCam)
            Gizmos.DrawWireSphere(mainCam.transform.position, radius);
    }
}
