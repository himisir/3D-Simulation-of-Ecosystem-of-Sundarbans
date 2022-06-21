
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class PreyStateMachine : MonoBehaviour
{
    public enum State
    {
        Idle, Roam, Flee, Drink, Breed, Dead
    }
    NavMeshAgent agent;
    State currentState = State.Idle;
    Animator machineState;
    public bool isHungry, isThirsty, isBreed,
        isDead, isFlee, isRoam;
    void SwitchMachineState(State state)
    {
        machineState.SetBool("isRoam", state == State.Roam);
        machineState.SetBool("isDanger", state == State.Flee);
        machineState.SetBool("isThirsty", state == State.Drink);
        machineState.SetBool("isBreed", state == State.Breed);
        machineState.SetBool("isDead", state == State.Dead);
    }

    void Start()
    {
        machineState = GetComponent<Animator>();
        SwitchMachineState(State.Idle);
        agent = GetComponent<NavMeshAgent>();
        //currentState = State.Idle;
    }

    public float wanderRange;
    IEnumerator Wander()
    {
        //Eat animation is played
        yield return new WaitForSeconds(UnityEngine.Random.Range(3, 5));
        agent.SetDestination(RandomPosition(wanderRange));
    }
    bool Percentage(float percent)
    {
        return UnityEngine.Random.Range(0, 100) < percent;
    }
    bool PercentTest(float input)
    {

        float value = UnityEngine.Random.Range(input, 101);
        value = ((int)value);
        Debug.Log(input + " " + value);
        if (value == 100)
        {
            return true;
        }
        return false;
    }

    float timer = 5;
    public float dayDuration = 5;
    public float timeSinceLastDrink = 1;
    public float age;
    public float timeSinceLastMate = 0;
    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            age++;
            timeSinceLastDrink++;
            timeSinceLastMate++;
            timer = dayDuration;
        }
        if (age >= 10)
        {
            isDead = true;
        }
        //thirst = Mathf.Clamp(thirst, 0, 100);
        thirst = (timeSinceLastDrink / maxDaysWithoutDrink) * 100;
        //if (PercentTest(thirst)) isThirsty = true;
        if (timeSinceLastMate >= 10)
        {
            isBreed = true;
            timeSinceLastMate = 0;
        }

        StateManager();
        if (isRoam)
        {
            SwitchMachineState(State.Roam);
        }
        if (isFlee)
        {
            SwitchMachineState(State.Flee);
        }
        if (isHungry)
        {
            SwitchMachineState(State.Drink);
        }
        if (isBreed)
        {
            SwitchMachineState(State.Breed);
            isBreed = false;
        }
        if (isDead)
        {
            SwitchMachineState(State.Dead);
        }
        if (isFlee)
        {
            SwitchMachineState(State.Flee);
        }
        if (isThirsty)
        {
            SwitchMachineState(State.Drink);
        }
    }
    public float fleeDistance;
    public float thirst = 0;
    public float maxDaysWithoutDrink;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Predator")
        {
            predator = other.gameObject.transform;
            if (Vector3.Distance(transform.position, other.transform.position) < fleeDistance)
            {
                isFlee = true;
            }
            else
            {
                isFlee = false;
            }
        }
    }



    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Predator")
        {
            isDead = true;
        }
        if (collision.gameObject.tag == "Water")
        {
            Debug.Log("Water found");
            isThirsty = false;
            timeSinceLastDrink = 1;
        }
    }


    Vector3 target;
    Transform predator = null;
    void StateManager()
    {
        if (machineState.GetNextAnimatorStateInfo(0).IsName("Idle"))
        {
            Eat();
            Debug.Log("Idle");
        }
        if (machineState.GetNextAnimatorStateInfo(0).IsName("Walk"))
        {
            Walk();
            Debug.Log("Walk");
        }
        if (machineState.GetNextAnimatorStateInfo(0).IsName("Flee"))
        {
            Flee();
            Debug.Log("Flee");
        }
        if (machineState.GetNextAnimatorStateInfo(0).IsName("Drink"))
        {
            Drink();
            Debug.Log("Drink");
        }
        if (machineState.GetNextAnimatorStateInfo(0).IsName("Breed"))
        {
            Breed();
            Debug.Log("Breed");
        }
        if (machineState.GetNextAnimatorStateInfo(0).IsName("Dead"))
        {
            Debug.Log("Dead");
            Dead();
        }
        if (machineState.GetNextAnimatorStateInfo(0).IsName("Roam Direction"))
        {
            target = RandomPosition(wanderRange);
            Debug.Log("Roam Direction");
        }
        if (machineState.GetNextAnimatorStateInfo(0).IsName("Flee Direction"))
        {
            if (predator != null)
            {
                target = FleeDirection(predator.position);
            }
            Debug.Log("Flee Direction");
        }
        if (machineState.GetNextAnimatorStateInfo(0).IsName("Water Direction"))
        {
            Debug.Log("Water Direction");
        }
    }
    public Transform waterSource;
    void WaterDirection()
    {
        target = waterSource.position;
    }

    void Roam()
    {

    }
    void Eat()
    {
        //Idle animation is played
    }
    public float fleeRange, fleeSpeed, speedFactor;
    public bool pathOutdated;
    Vector3 FleeDirection(Vector3 _fleeFrom)
    {

        Vector3 runTo = transform.position + ((_fleeFrom - transform.position).normalized * fleeRange);
        return runTo;



    }
    void Flee()
    {
        agent.SetDestination(FleeDirection(target));
        //run animation is played
    }
    void Walk()
    {
        agent.SetDestination(target);
        //Walk animation is played
    }

    void Drink()
    {
        agent.SetDestination(waterSource.position);
        //idle animation is played
    }
    void Breed()
    {
        Instantiate(this.gameObject, transform.position, transform.rotation);
        //Instantiate(Resources.Load("Prey"), transform.position, Quaternion.identity);
    }

    void Dead()
    {
        Destroy(this.gameObject);
    }
    Vector3 RandomPosition(float range)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * range;
        randomDirection += transform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, range, -1);
        return navHit.position;
    }

}
