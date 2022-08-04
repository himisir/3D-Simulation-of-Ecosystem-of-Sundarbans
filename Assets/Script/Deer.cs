/*
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Deer : MonoBehaviour
{
    public enum AIState { Idle, Walking, Eating, Running }
    public AIState currentState = AIState.Idle;
    public int awarenessArea = 15; //How far the deer should detect the enemy
    public float walkingSpeed = 3.5f;
    public float runningSpeed = 7f;
    public Animator animator;

    //Trigger collider that represents the awareness area
    SphereCollider c;
    //NavMesh Agent
    NavMeshAgent agent;

    bool switchAction = false;
    float actionTimer = 0; //Timer duration till the next action
    Transform enemy;
    public float range = 20; //How far the Deer have to run to resume the usual activities
    float multiplier = 1;
    bool reverseFlee = false; //In case the AI is stuck, send it to one of the original Idle points

    //Detect NavMesh edges to detect whether the AI is stuck
    Vector3 closestEdge;
    float distanceToEdge;
    float distance; //Squared distance to the enemy
    //How long the AI has been near the edge of NavMesh, if too long, send it to one of the random previousIdlePoints
    float timeStuck = 0;
    //Store previous idle points for reference
    List<Vector3> previousIdlePoints = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0;
        agent.autoBraking = true;

        c = gameObject.AddComponent<SphereCollider>();
        c.isTrigger = true;
        c.radius = awarenessArea;

        //Initialize the AI state
        currentState = AIState.Idle;
        actionTimer = Random.Range(0.1f, 2.0f);
        SwitchAnimationState(currentState);
    }

    // Update is called once per frame
    void Update()
    {
        //Wait for the next course of action
        if (actionTimer > 0)
        {
            actionTimer -= Time.deltaTime;
        }
        else
        {
            switchAction = true;
        }

        if (currentState == AIState.Idle)
        {
            if (switchAction)
            {
                if (enemy)
                {
                    //Run away
                    agent.SetDestination(RandomNavSphere(transform.position, Random.Range(1, 2.4f)));
                    currentState = AIState.Running;
                    SwitchAnimationState(currentState);
                }
                else
                {
                    //No enemies nearby, start eating
                    actionTimer = Random.Range(14, 22);

                    currentState = AIState.Eating;
                    SwitchAnimationState(currentState);

                    //Keep last 5 Idle positions for future reference
                    previousIdlePoints.Add(transform.position);
                    if (previousIdlePoints.Count > 5)
                    {
                        previousIdlePoints.RemoveAt(0);
                    }
                }
            }
        }
        else if (currentState == AIState.Walking)
        {
            //Set NavMesh Agent Speed
            agent.speed = walkingSpeed;

            // Check if we've reached the destination
            if (DoneReachingDestination())
            {
                currentState = AIState.Idle;
            }
        }
        else if (currentState == AIState.Eating)
        {
            if (switchAction)
            {
                //Wait for current animation to finish playing
                if (!animator || animator.GetCurrentAnimatorStateInfo(0).normalizedTime - Mathf.Floor(animator.GetCurrentAnimatorStateInfo(0).normalizedTime) > 0.99f)
                {
                    //Walk to another random destination
                    agent.destination = RandomNavSphere(transform.position, Random.Range(3, 7));
                    currentState = AIState.Walking;
                    SwitchAnimationState(currentState);
                }
            }
        }
        else if (currentState == AIState.Running)
        {
            //Set NavMesh Agent Speed
            agent.speed = runningSpeed;

            //Run away
            if (enemy)
            {
                if (reverseFlee)
                {
                    if (DoneReachingDestination() && timeStuck < 0)
                    {
                        reverseFlee = false;
                    }
                    else
                    {
                        timeStuck -= Time.deltaTime;
                    }
                }
                else
                {
                    Vector3 runTo = transform.position + ((transform.position - enemy.position) * multiplier);
                    distance = (transform.position - enemy.position).sqrMagnitude;

                    //Find the closest NavMesh edge
                    NavMeshHit hit;
                    if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
                    {
                        closestEdge = hit.position;
                        distanceToEdge = hit.distance;
                        //Debug.DrawLine(transform.position, closestEdge, Color.red);
                    }

                    if (distanceToEdge < 1f)
                    {
                        if (timeStuck > 1.5f)
                        {
                            if (previousIdlePoints.Count > 0)
                            {
                                runTo = previousIdlePoints[Random.Range(0, previousIdlePoints.Count - 1)];
                                reverseFlee = true;
                            }
                        }
                        else
                        {
                            timeStuck += Time.deltaTime;
                        }
                    }

                    if (distance < range * range)
                    {
                        agent.SetDestination(runTo);
                    }
                    else
                    {
                        enemy = null;
                    }
                }

                //Temporarily switch to Idle if the Agent stopped
                if (agent.velocity.sqrMagnitude < 0.1f * 0.1f)
                {
                    SwitchAnimationState(AIState.Idle);
                }
                else
                {
                    SwitchAnimationState(AIState.Running);
                }
            }
            else
            {
                //Check if we've reached the destination then stop running
                if (DoneReachingDestination())
                {
                    actionTimer = Random.Range(1.4f, 3.4f);
                    currentState = AIState.Eating;
                    SwitchAnimationState(AIState.Idle);
                }
            }
        }

        switchAction = false;
    }

    bool DoneReachingDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    //Done reaching the Destination
                    return true;
                }
            }
        }

        return false;
    }

    void SwitchAnimationState(AIState state)
    {
        //Animation control
        if (animator)
        {
            animator.SetBool("isEating", state == AIState.Eating);
            animator.SetBool("isRunning", state == AIState.Running);
            animator.SetBool("isWalking", state == AIState.Walking);
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float distance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;

        randomDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas);

        return navHit.position;
    }

    void OnTriggerEnter(Collider other)
    {
        //Make sure the Player instance has a tag "Player"
        if (!other.CompareTag("Predator"))
            return;

        enemy = other.transform;

        actionTimer = Random.Range(0.24f, 0.8f);
        currentState = AIState.Idle;
        SwitchAnimationState(currentState);
    }
}*/