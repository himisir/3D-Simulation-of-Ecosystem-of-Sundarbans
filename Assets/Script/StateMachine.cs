using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public float movSpeed;
    public float rotSpeed = 100f;
    private bool isWandering = false;
    private bool isRotL = false;
    private bool isRotR = false;
    private bool isWalking = false;
    float lookRange = 5f;


    public enum MachineState
    {
        Idle,
        Walking,
        Running,
        Dead
    }
    MachineState state = MachineState.Idle;
/*
    void LocalRegionManager(GameObject tigerOrigin, GameObject deerOrigin, GameObject _waterSource)
    {
        origin = tigerOrigin.transform;
        waterSource = _waterSource.transform;
    }

public void CalenderSystem()
    {
        age++;
        timeSinceLastDrink++;
        timeSinceLastMeal++;
        HungerThirst();
        if (isPregnant)
        {
            timeSinceLastMate++;
        }
        if (hasBreed)
        {
            timeSinceLastBreed++;
        }
    }
    void VisionCheck()
    {
        visionSphere.radius = visionRadius;
        visionSphere.isTrigger = true;
    }

    void SwitchState(MachineState newState)
    {
        switch (newState)
        {
            case MachineState.Idle:
                break;
            case MachineState.Walking:
                break;
            case MachineState.Running:
                break;
            case MachineState.Dead:
                break;
        }
    }
    void Idle()
    {

    }
    void Walking()
    {

    }
    void Running()
    {

    }
    void Dead()
    {

    }

    void Chase(Vector3 _target, TargetType _targetType)
    {
        Debug.Log("Chase");
        agent.speed = runningSpeed * speedFactor;
        agent.SetDestination(_target);
        if (!isFoodFound || !isWaterFound)
        {
            currentState = State.Idle;
        }
    }
    void FindFood()
    {
        if (isFoodFound)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Roam;
        }
        Debug.Log("FindFood");

    }
    void FindWater()
    {
        if (isWaterFound)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Roam;
        }
        Debug.Log("FindWater");
    }


    private void Update()
    {


    }
*/
    //Run in runtime
    void WanderingHelper()
    {
        if (isWandering == false)
        {
            StartCoroutine(Wandering());
            StartCoroutine(Rotation());
        }

        if (isWalking == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.forward * 100f, Time.deltaTime * movSpeed);
        }
        if (isRotR == true)
        {
            transform.Rotate(transform.up * Time.deltaTime * rotSpeed);
        }
        if (isRotL == true)
        {
            transform.Rotate(transform.up * Time.deltaTime * -rotSpeed);
        }
    }

    IEnumerator Rotation()
    {
        float rotTime = Mathf.PerlinNoise(UnityEngine.Random.Range(0.1f, 1), UnityEngine.Random.Range(0, 100f));
        float rotWait = Mathf.PerlinNoise(UnityEngine.Random.Range(1, 3), UnityEngine.Random.Range(0, 100f));
        float rotateLR = UnityEngine.Random.Range(1, 3);
        yield return new WaitForSeconds(rotWait);
        if (rotateLR == 1)
        {
            isRotR = true;
            yield return new WaitForSeconds(rotTime);
            isRotR = false;
        }
        if (rotateLR == 2)
        {
            isRotL = true;
            yield return new WaitForSeconds(rotTime);
            isRotL = false;
        }
    }

    IEnumerator Wandering()
    {

        float rotTime = Mathf.PerlinNoise(Random.Range(0.1f, 1), Random.Range(0, 100f));
        float rotWait = Mathf.PerlinNoise(Random.Range(1, 3), Random.Range(0, 100f));
        float rotateLR = Random.Range(1, 3);
        float walkWait = Mathf.PerlinNoise(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 100f));
        float walkTime = Mathf.PerlinNoise(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 100f));

        isWandering = true;
        yield return new WaitForSeconds(walkWait);

        yield return new WaitForSeconds(rotWait);
        if (rotateLR == 1)
        {
            isRotR = true;
            yield return new WaitForSeconds(rotTime);
            isRotR = false;
        }
        if (rotateLR == 2)
        {
            isRotL = true;
            yield return new WaitForSeconds(rotTime);
            isRotL = false;
        }
        isWalking = true;
        yield return new WaitForSeconds(walkTime);
        isWalking = false;
        isWandering = false;

    }

}
