using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorScript : MonoBehaviour
{
    Transform target;
    SphereCollider vision;
    public float visionRadius;
    public float dangerRadius;
    public float speed;
    public float rotationSpeed;
    public float maxSpeed;
    public float maxVelocity;
    public float maxForce;
    float mass;
    Vector3 velocity;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mass = rb.mass;
        velocity = Vector3.zero;
        vision = GetComponent<SphereCollider>();
        vision.isTrigger = true;
    }
    void VisionCheck()
    {
        vision.radius = visionRadius;
    }
    void Update()
    {
        VisionCheck();
    }
    void Steer()
    {
        var direction = target.position - transform.position;
        var desiredVelocity = direction.normalized * maxVelocity;
        var steering = desiredVelocity - velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering /= mass;
        velocity = Vector3.ClampMagnitude(velocity + steering, maxVelocity);
        transform.position += velocity * Time.deltaTime;
        transform.forward = velocity.normalized;

    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Prey")
        {
            target = other.transform;
            Steer();
        }
    }
}
