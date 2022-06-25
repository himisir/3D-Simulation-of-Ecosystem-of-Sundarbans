
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringBehavior : MonoBehaviour
{
    public float maxSpeed, maxForce;
    Vector3 acceleration, velocity, location, startPosition;
    float wonderTime;
    public float _wonderTime;
    public float visionRadius;
    Vector2 randomDirection;
    void Start()
    {
        acceleration = Vector3.zero;
        velocity = Vector3.zero;
        location = transform.position;
        startPosition = transform.position;
        randomDirection = Random.insideUnitSphere.normalized;
    }
    void Update()
    {
        // Wander();
    }
    void ApplySteering()
    {
        velocity = Vector3.ClampMagnitude(velocity + acceleration, maxSpeed);
        location += velocity * Time.deltaTime;
        acceleration = Vector3.zero;
        LookAtTarget();
        transform.position = location;
    }
    void LookAtTarget()
    {
        var direction = location - transform.position;
        direction = direction.normalized;
        var rotZ = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);
    }
    void Steer(Vector3 targetPosition)
    {
        var direction = targetPosition - location;
        direction = direction.normalized;
        var force = direction * maxSpeed;
        var steer = Vector3.ClampMagnitude(force - velocity, maxForce);
        ApplyForce(acceleration);
    }
    void ApplyForce(Vector3 force)
    {
        acceleration += force;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Prey")
        {
            Steer(other.transform.position);
            ApplySteering();
        }
    }

    void Wander()
    {
        wonderTime -= Time.deltaTime;
        var centre = (Vector2)transform.position + (Vector2)transform.up * visionRadius;
        if (wonderTime <= 0)
        {
            wonderTime = _wonderTime;
            randomDirection = Random.insideUnitSphere.normalized;
        }
        var target = centre + randomDirection * visionRadius;
        Steer(target);
        ApplySteering();
        Debug.DrawRay(centre, (Vector2)target - centre, Color.green);

    }
}
