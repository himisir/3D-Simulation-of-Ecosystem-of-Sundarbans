using System.IO.IsolatedStorage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Predator : MonoBehaviour
{


    public enum AnimationState
    {
        Idle,
        Walking,
        Running,
        Attacking,
        Dead
    }
    public enum State
    {
        Hungry,
        Thirsty,
        Breed,
        Chase,
        Flee,
        Idle,
        Dead,
        Home,
        Roam
    }
    AnimationState animationState = AnimationState.Idle;
    State currentState = State.Idle;
    // [Header("Red Bool")]
    public bool isFoodFound;
    public bool isWaterFound;

    //[Header("Components and References")]
    Animator animator;
    NavMeshAgent agent;
    SphereCollider visionSphere;
    Transform origin, waterSource;
    Vector3 target;

    // [Header("Predator Properties")]
    public float age;
    public string gender; //male/female
    public float hunger;
    public float thirst;
    public float walkingSpeed = 10f;
    float summerSpeedFactor = 1.5f;
    float rainSpeedFactor = 0.5f;
    public float runningSpeed = 20f;
    public float visionRadius = 10f;
    public float roamDistance = 15;
    float breedFactor = 1;
    float speedFactor = 1;

    // [Header("Predator Booleans")]
    [SerializeField]
    bool isPregnant;
    [SerializeField]
    bool isHungry;
    [SerializeField]
    bool isThirsty;
    [SerializeField]
    bool isDead;
    [SerializeField]
    bool isMate;
    [SerializeField]
    bool isBreedNow;
    [SerializeField]

    bool isAdult;
    [SerializeField]
    bool unfortunateDeath;
    [SerializeField]
    bool isChase;
    [SerializeField]
    bool isRoam;
    [SerializeField]
    bool isStuck;


    //[Header("Consumption and Reproduction")]
    [SerializeField]
    float timeSinceLastMeal = 1;
    [SerializeField]
    float timeSinceLastDrink = 1;
    [SerializeField]
    float timeSinceLastMate = 1;

    //[Header("Date from sheet")]

    [SerializeField]
    float lifeTime;
    [SerializeField]
    float maxDaysWithoutFood;
    [SerializeField]
    float maxDaysWithoutDrink;
    [SerializeField]
    float pregnancyPeriod;
    ////////////////////////////////

    //[Header("Memory Properties")]
    int memorySize = 5;
    List<Vector3> lastMateFoundPositions = new List<Vector3>();
    List<Vector3> lastFoodFoundPositions = new List<Vector3>();
    List<Vector3> lastWaterFoundPositions = new List<Vector3>();


    public static event Action<GameObject> OnDeath;
    public static event Action OnSpawn;

    void Start()
    {
        visionSphere = GetComponent<SphereCollider>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        int random = UnityEngine.Random.Range(0, 2);
        gender = (random == 1) ? "male" : "female";
        currentState = State.Idle;
        SimulationManager.Initialize += Initialization;
        SimulationManager.AgeCounter += CalenderSystem;
        SimulationManager.Origin += LocalRegionManager;
        SimulationManager.NewBornStat += ParameterInitializeForNewBreed;

    }
    private void OnDestroy()
    {
        SimulationManager.AgeCounter -= CalenderSystem;
        SimulationManager.Initialize -= Initialization;
        SimulationManager.Origin -= LocalRegionManager;
        SimulationManager.NewBornStat -= ParameterInitializeForNewBreed;
    }
    public void ParameterInitializeForNewBreed()
    {
        age = 0;
        hunger = 1;
        thirst = 1;
        isPregnant = false;
        isHungry = false;
        isDead = false;
        isBreedNow = false;
        isAdult = false;
        unfortunateDeath = false;
        timeSinceLastMate = 1;
        timeSinceLastDrink = UnityEngine.Random.Range(1, maxDaysWithoutDrink);
        timeSinceLastMeal = 1;

    }
    public void Initialization()
    {
        hunger = 1;
        thirst = 1;
        timeSinceLastMeal = UnityEngine.Random.Range(0, maxDaysWithoutFood);
        timeSinceLastDrink = UnityEngine.Random.Range(0, maxDaysWithoutDrink);
        maxDaysWithoutFood = UnityEngine.Random.Range(maxDaysWithoutFood - 1, maxDaysWithoutFood + 3);
        maxDaysWithoutDrink = UnityEngine.Random.Range(maxDaysWithoutDrink, maxDaysWithoutDrink + 3);
        if (gender == "female")
        {
            isPregnant = true;
            timeSinceLastMate = UnityEngine.Random.Range(pregnancyPeriod - 1, pregnancyPeriod);
        }
    }
    //Action Event Subscriber
    void LocalRegionManager(GameObject tigerOrigin, GameObject deerOrigin, GameObject _waterSource)
    {
        origin = tigerOrigin.transform;
        waterSource = _waterSource.transform;
    }

    //Action Event Subscriber
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
    }
    void VisionCheck()
    {
        visionSphere.radius = visionRadius;
        visionSphere.isTrigger = true;
    }
    void Update()
    {
        if (Vector3.Distance(transform.position, waterSource.position) <= 5)
        {
            isThirsty = false;
            thirst = 1;
            isChase = false;
        }
        IsStuck();
        VisionCheck();
        Engine();
        StateCheck();
        SwitchState(currentState);
        SwitchAnimation(animationState);
    }

    void Engine()
    {
        if (timeSinceLastMate > pregnancyPeriod)
        {
            isBreedNow = true;
        }
        if (timeSinceLastMeal > maxDaysWithoutFood / 2)
        {
            isHungry = true;
        }

        if (timeSinceLastDrink > maxDaysWithoutDrink / 2)
        {
            isThirsty = true;
        }
        ////////////////////////////////
        ///Check if the predator is dead
        if (age >= lifeTime)
        {
            Debug.Log("Died at age: " + age);
            isDead = true;
        }
        else if (timeSinceLastMeal >= maxDaysWithoutFood)
        {
            Debug.Log("Died of Hunger: " + timeSinceLastMeal);
            isDead = true;
        }
        else if (timeSinceLastDrink >= maxDaysWithoutDrink)
        {
            Debug.Log("Died of Thirst: " + timeSinceLastDrink);
            isDead = true;
        }
        else if (unfortunateDeath)
        {
            Debug.Log("Died of Unfortunate Death: " + unfortunateDeath);
            isDead = true;
        }

    }
    void SwitchState(State currentState)
    {
        switch (currentState)
        {
            case State.Chase:
                Chase(target);
                break;
            case State.Roam:
                Roam();
                break;
            case State.Idle:
                Idle();
                break;
        }
    }

    void StateCheck()
    {
        if (isDead)
        {
            Death();
        }
        else if (isThirsty)
        {
            Chase(waterSource.position);
        }
        else if (isBreedNow)
        {
            isBreedNow = false;
            Breed();
        }
        else if (isChase)
        {
            currentState = State.Chase;
        }
        else if (isHungry)
        {
            currentState = State.Roam;
        }
        else
        {
            currentState = State.Idle;
        }
        if (isStuck)
        {
            isStuck = false;
            RandomSearch(transform.position, roamDistance);
            currentState = State.Roam;
        }
    }

    float timer = 0.5f;
    void Idle()
    {

        if (timer > 0)
        {
            animationState = AnimationState.Walking;
            timer -= Time.deltaTime;
        }
        else
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                timer = UnityEngine.Random.Range(1f, 6f);
                animationState = AnimationState.Idle;
                float range = UnityEngine.Random.Range(roamDistance, roamDistance * 5);
                agent.SetDestination(RandomSearch(transform.position, range));
            }
            else
            {
                animationState = AnimationState.Walking;
                agent.SetDestination(agent.destination);
            }

        }

    }

    void IsStuck()
    {
        float stuckTime = 2f;
        while (stuckTime > 0)
        {
            if (!(agent.pathPending && agent.velocity.sqrMagnitude <= 0.1f))
            {
                return;
            }
            stuckTime -= Time.deltaTime;
        }
        isStuck = true;
    }
    ///////////////////////////////////////////////////////////////////////////////////////
    void HungerThirst()
    {
        hunger += (timeSinceLastMeal * breedFactor / maxDaysWithoutFood) * 100;
        if (hunger >= 100)
        {
            hunger = 100;
        }
        thirst += (timeSinceLastDrink * breedFactor / maxDaysWithoutDrink) * 100;
        if (thirst >= 100)
        {
            thirst = 100;
        }
    }

    void Roam()
    {
        if (!DestinationReached()) return;
        agent.speed = walkingSpeed * speedFactor;
        animationState = AnimationState.Walking;
        agent.SetDestination(RandomSearch(transform.position, roamDistance));
    }
    void Chase(Vector3 targetPos)
    {
        agent.speed = runningSpeed * speedFactor;
        animationState = AnimationState.Running;
        agent.SetDestination(targetPos);
    }
    void FindWater()
    {
        target = waterSource.position;
        isChase = true;
        Debug.Log("FindWater");
    }
    public void Death()
    {
        Debug.Log("Predator is dead");
        OnDeath(this.gameObject);
    }
    public void Breed()
    {
        agent.speed = 0;
        isPregnant = false;
        isBreedNow = false;
        OnSpawn();
    }
    ////////////////////////////////////////////////////////////////
    /// Switch the animation to the given state
    void SwitchAnimation(AnimationState currentState)
    {
        animator.SetBool("isWalking", currentState == AnimationState.Walking);
        animator.SetBool("isRunning", currentState == AnimationState.Running);
        animator.SetBool("isAttacking", currentState == AnimationState.Attacking);
    }

    void AddToList(List<Vector3> list, Vector3 pos)
    {
        if (list.Count < memorySize)
        {
            list.Add(pos);
        }
        else
        {
            list.RemoveAt(0);
            list.Add(pos);
        }
    }

    ////////////////////////////////

    Vector3 RandomSearch(Vector3 currentPos, float radius)
    {
        if (agent == null) return transform.position;
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += currentPos;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, radius, -1);
        return navHit.position;
    }
    bool DestinationReached()
    {
        if (agent == null) return false;
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void CheckTask()
    {
        SummerStat();
        RainStat();
    }
    void SummerStat()
    {
        speedFactor = summerSpeedFactor;
    }
    void RainStat()
    {
        speedFactor = rainSpeedFactor;
    }


    ///Triggers and Collisions //Lets "false" every bool after the triggered Method() is called
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Water" && isThirsty)
        {
            target = other.gameObject.transform.position;
            isWaterFound = true;
        }
        else if (other.gameObject.tag == "Prey" && isHungry)
        {
            target = other.gameObject.transform.position;
            isChase = true;

        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Water" && isThirsty)
        {
            target = other.gameObject.transform.position;
            isWaterFound = true;
        }
        else if (other.gameObject.tag == "Prey" && isHungry)
        {
            target = other.gameObject.transform.position;
            isChase = true;

        }

    }
    //Might face issues with the conditions in the if statements
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            isChase = false;
            isWaterFound = false;
        }
        if (other.gameObject.tag == "Prey")
        {
            isChase = false;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Water")
        {
            isThirsty = false;
            isWaterFound = false;
            thirst = 1;
            timeSinceLastDrink = 1;
            isChase = false;
        }
        if (collision.gameObject.tag == "Prey")
        {
            isFoodFound = false;
            hunger = 1;
            timeSinceLastMeal = 1;
            isHungry = false;
            isChase = false;
            // currentState = State.Idle;
        }

    }
}


