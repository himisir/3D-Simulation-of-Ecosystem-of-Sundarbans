using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyScript : MonoBehaviour
{
    Transform target, obstacle;
    SphereCollider vision;
    public float visionRadius;
    public float dangerRadius;
    public float speed;
    public float rotationSpeed;
    public float maxSpeed;
    public float maxVelocity;
    public float maxForce;
    public float slowDistance;
    float mass;
    Vector3 velocity;
    Rigidbody rb;

    void Start()
    {
        randomVelocity = Random.onUnitSphere;
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
    public bool flag;
    Vector3 Steer()
    {
        var direction = target.position - transform.position;
        var desiredVelocity = direction.normalized;
        var distance = Vector3.Distance(transform.position, target.position);
        if (distance < slowDistance)
        {
            desiredVelocity = direction.normalized * maxVelocity * (distance / slowDistance);
        }
        else desiredVelocity = direction.normalized * maxVelocity;
        var steering = desiredVelocity - velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering /= mass;
        velocity = Vector3.ClampMagnitude(velocity + steering, maxVelocity);
        return velocity;
    }
    Vector3 randomVelocity;
    Vector3 Wander()
    {
        ;
        var desiredVelocity = GetWanderForce();
        desiredVelocity = desiredVelocity.normalized * maxVelocity;
        var steeringForce = desiredVelocity - velocity;

        steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
        steeringForce /= mass;
        velocity = Vector3.ClampMagnitude(velocity + steeringForce, maxVelocity);
        return velocity;
    }
    float turnChance = 0.005f;
    Vector3 GetWanderForce()
    {
        var wanderForce = Vector3.zero;
        if (transform.position.magnitude > visionRadius)
        {
            var direction = transform.position - Vector3.zero; //target= Vector3.zero;
            wanderForce = velocity.normalized + direction.normalized;

        }
        else if (Random.value < turnChance)
        {
            wanderForce = GetRandomForce();
        }
        return wanderForce;
    }
    Vector3 GetRandomForce()
    {
        var circleCentre = velocity.normalized;
        var randomPoint = Random.insideUnitSphere;
        var displacement = new Vector3(randomPoint.x, randomPoint.y) * visionRadius;
        displacement = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * displacement;
        
        


        var randomForce = circleCentre + displacement;
        return randomForce;
    }

    Vector3 Avoid()
    {

        var ahead = transform.position + velocity.normalized * visionRadius;
        var avoidance = ahead - transform.position;
        avoidance = avoidance.normalized;
        avoidance = Vector3.ClampMagnitude(avoidance, maxForce);
        avoidance /= mass;
        return avoidance * maxVelocity;

    }
    Vector3 Evade()
    {
        float distance = Vector3.Distance(target.position, transform.position);
        float ahead = distance / 10;
        var futurePosition = target.position + velocity * ahead;
        return futurePosition;
    }


    void Flee()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, -1);
        }
    }
    public bool flag2;

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Predator")
        {
            flag2 = false;
            if (Vector3.Distance(transform.position, other.transform.position) < dangerRadius)
            {
                flag2 = true;
                target = other.transform;
                if (flag)
                {
                    transform.LookAt(target);
                    transform.position += Steer() * Time.deltaTime;
                }
                else
                {
                    transform.LookAt(transform.position - Steer());
                    transform.position -= Steer() * Time.deltaTime;
                }
            }
        }

    }


}
