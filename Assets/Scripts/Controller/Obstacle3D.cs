using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Obstacle3D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float maxDistance = 1f;
    [SerializeField] private Transform destinationPoint;
    [SerializeField] Vector3 direction;
    [SerializeField] private Vector3 velocity;

    public Vector3 Velocity => velocity;

    // Update is called once per frame
    void Update()
    {
        UpdateVelocity();
        transform.position += velocity;
    }

    private void UpdateVelocity()
    {
        if (Vector3.Distance(transform.position, destinationPoint.position) >= maxDistance)
        {
            velocity = Vector3.zero;
            moveSpeed = -moveSpeed;
        }

        velocity = direction * moveSpeed * Time.deltaTime;
    }
}
