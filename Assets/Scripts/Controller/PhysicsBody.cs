using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CapsuleCollider))]
public class PhysicsBody : MonoBehaviour
{
    [Header("Properties of body")]
    [SerializeField] private float skinWidth = 0.5f;
    [SerializeField] private float groundCheckDistance;

    [Header("Physics")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float airResistance = 0.3f;
    [SerializeField] private float decelerationFactor = 1f;
    [SerializeField] private float accelerationFactor = 1f;
    [SerializeField] private float staticFrictionCoefficient = 1f;
    [SerializeField] private float kineticFrictionCoefficient = 1f;
    [SerializeField] private LayerMask obstacleLayer;

    private CapsuleCollider capsuleCollider;
    private RaycastHit groundHit;
    private Vector3 velocity;
    private Vector3 upperPoint;
    private Vector3 lowerPoint;
    private Vector3 gravitationForce;

    public Vector3 Velocity
    {
        set => velocity = value;
        get => velocity;
    }

    public bool Grounded => groundHit.collider != null;
    public RaycastHit GroundHit => groundHit;

    private void Awake()
    {
        groundCheckDistance = 10 * skinWidth;
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        groundHit = IsGrounded();
        ApplyGravity();
        ApplyAirResistance();
        UpdateVelocity();

        transform.position += velocity * Time.deltaTime;
    }

    private void UpdateVelocity()
    {
        upperPoint = transform.position + capsuleCollider.center + Vector3.up * (capsuleCollider.height / 2 - capsuleCollider.radius);
        lowerPoint = transform.position + capsuleCollider.center + Vector3.down * (capsuleCollider.height / 2 - capsuleCollider.radius);

        Obstacle3D obstacle = null;
        if (Grounded)
        {
            obstacle = groundHit.transform.GetComponent<Obstacle3D>();
        }


        if (Physics.CapsuleCast(upperPoint, lowerPoint, capsuleCollider.radius, velocity.normalized, out RaycastHit hit, velocity.magnitude * Time.deltaTime + skinWidth, obstacleLayer))
        {

            Physics.CapsuleCast(upperPoint, lowerPoint, capsuleCollider.radius, -hit.normal, out RaycastHit normalHit, velocity.magnitude * Time.deltaTime + skinWidth, obstacleLayer);

            if (obstacle != null)
            {
                transform.position += obstacle.Velocity;
            }
            else
            {
                transform.position += -normalHit.normal * (normalHit.distance - skinWidth);
            }

            Vector3 normalForce;
            if (obstacle)
                normalForce = PhysicsUtils.CalculateNormalForce(velocity + obstacle.Velocity, hit.normal);
            else
                normalForce = PhysicsUtils.CalculateNormalForce(velocity, normalHit.normal);

            velocity += normalForce;
            ApplyFriction(normalForce);
        }

        ResolveCollision();
    }

    private void ResolveCollision()
    {
        Collider[] colliderHits = Physics.OverlapCapsule(upperPoint, lowerPoint, capsuleCollider.radius, obstacleLayer);

        foreach (Collider col in colliderHits)
        {
            if (Physics.ComputePenetration(capsuleCollider, transform.position, transform.rotation, col, col.transform.position, col.transform.rotation, out Vector3 direction, out float distance))
            {
                Vector3 separationVector = direction * distance;
                transform.position += separationVector + separationVector.normalized * distance;

                velocity += PhysicsUtils.CalculateNormalForce(velocity, separationVector.normalized);
            }
        }
    }

    private void ApplyGravity()
    {
        gravitationForce = Vector3.down * gravity * Time.deltaTime;
        velocity += gravitationForce;
    }

    private void ApplyFriction(Vector3 normalForce)
    {
        if (velocity.magnitude < normalForce.magnitude * staticFrictionCoefficient)
        {
            velocity = Vector3.zero;
        }
        else
        {
            velocity -= velocity.normalized * normalForce.magnitude * kineticFrictionCoefficient;
        }
    }

    private void ApplyAirResistance()
    {
        velocity *= Mathf.Pow(airResistance, Time.deltaTime);
    }

    private RaycastHit IsGrounded()
    {
        Physics.CapsuleCast(upperPoint, lowerPoint, capsuleCollider.radius, Vector3.down, out RaycastHit hit, groundCheckDistance + skinWidth, obstacleLayer);
        return hit;
    }

    public void Accelerate(Vector3 input, float maxSpeed)
    {
        velocity += input * accelerationFactor * Time.deltaTime;
        if (velocity.magnitude > maxSpeed)
        {
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        }
    }

    public void Decelerate()
    {
        Vector3 projection = new Vector3(velocity.x, 0.0f, velocity.z).normalized;
        Vector3 deceleration = projection * decelerationFactor * Time.deltaTime;

        if (!(deceleration.magnitude > Mathf.Abs(projection.magnitude)))
        {
            velocity -= deceleration;
        }
        else
        {
            velocity.x = 0.0f;
            velocity.z = 0.0f;
        }
    }

    public void AddForce(Vector3 force)
    {
        velocity += force;
    }
}
