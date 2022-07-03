
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
    public static event Action<GameObject> OnKill;
    public static event Action<GameObject> OnSpawn;

    void Start()
    {

        visionSphere = GetComponent<SphereCollider>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        origin = GameObject.Find("Tiger Origin").transform;
        //waterSource = GameObject.Find("Water Source Tiger").transform;
        agent.speed = walkingSpeed;
        currentState = State.Idle;
        SimulationManager.Initialize += Initialization;
        SimulationManager.AgeCounter += CalenderSystem;
        //SimulationManager.Origin += LocalRegionManager;
        SimulationManager.NewBornStat += ParameterInitializeForNewBreed;

    }
    private void OnDestroy()
    {
        SimulationManager.AgeCounter -= CalenderSystem;
        SimulationManager.Initialize -= Initialization;
        //SimulationManager.Origin -= LocalRegionManager;
        SimulationManager.NewBornStat -= ParameterInitializeForNewBreed;
    }
    public void ParameterInitializeForNewBreed()
    {
        int random = UnityEngine.Random.Range(0, 2);
        gender = (random == 1) ? "male" : "female";
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
        int random = UnityEngine.Random.Range(0, 3);
        gender = (random == 1) ? "male" : "female";
        hunger = 0;
        thirst = 0;
        timeSinceLastMeal = UnityEngine.Random.Range(0, maxDaysWithoutFood);
        timeSinceLastDrink = UnityEngine.Random.Range(0, maxDaysWithoutDrink);
        maxDaysWithoutFood = UnityEngine.Random.Range(maxDaysWithoutFood - 1, maxDaysWithoutFood + 3);
        maxDaysWithoutDrink = UnityEngine.Random.Range(maxDaysWithoutDrink, maxDaysWithoutDrink + 3);
        if (gender == "female")
        {
            isPregnant = true;
            timeSinceLastMate = UnityEngine.Random.Range(pregnancyPeriod - 3, pregnancyPeriod);
        }
    }
    //Action Event Subscriber
    public void LocalRegionManager(GameObject tigerOrigin, GameObject deerOrigin, GameObject _waterSourceDeer, GameObject _waterSourceTiger)
    {
        if (tigerOrigin != null)
        {
            origin = tigerOrigin.transform;
        }
        if (_waterSourceTiger != null)
        {
            // waterSource = _waterSourceTiger.transform;
        }
    }

    //Action Event Subscriber
    public void CalenderSystem()
    {
        age++;
        timeSinceLastDrink++;
        timeSinceLastMeal++;
        HungerThirst();
        if (gender == "female") timeSinceLastMate++;
    }
    void VisionCheck()
    {
        chaseDistance = visionRadius - 2;
        visionSphere.radius = visionRadius;
        visionSphere.isTrigger = true;
    }
    void Update()
    {

        IsStuck();
        VisionCheck();
        Engine();
        StateCheck();
        SwitchState(currentState);
        SwitchAnimation(animationState);
        // WaterConsumption();
    }

    void Engine()
    {
        if (gender == "female" && timeSinceLastMate > pregnancyPeriod)
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
        else if (isBreedNow)
        {
            isBreedNow = false;
            Breed();
        }
        else if (isThirsty)
        {
            Chase(waterSourcePosition());
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
    }

    float timer = 0.5f;
    public float targetReachedDistance = 1f;
    void Idle()
    {

        if (timer > 0)
        {
            animationState = AnimationState.Walking;
            timer -= Time.deltaTime;
        }
        else
        {
            if (agent?.remainingDistance <= agent?.stoppingDistance + targetReachedDistance || isStuck)
            {
                timer = UnityEngine.Random.Range(1f, 3f);
                animationState = AnimationState.Idle;
                float range = UnityEngine.Random.Range(roamDistance * 2, roamDistance * 5);
                agent?.SetDestination(RandomSearch(transform.position, range));
            }
            else
            {
                animationState = AnimationState.Walking;
                agent?.SetDestination(agent.destination);
            }

        }

    }
    float stuckTime = 10f;
    void IsStuck()
    {
        if (agent != null)
        {
            if (stuckTime > 0)
            {
                if ((agent.pathPending && agent.remainingDistance > agent.stoppingDistance + targetReachedDistance) || agent.isStopped)
                {
                    stuckTime -= Time.deltaTime;
                }
                else
                {
                    isStuck = false;
                }

            }
            else
            {
                isStuck = true;
                stuckTime = 10;
            }
        }


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
        if (agent != null)
        {
            if (!(agent?.remainingDistance <= agent?.stoppingDistance + targetReachedDistance)) agent?.SetDestination(agent.destination);
            else
            {
                agent.speed = walkingSpeed * speedFactor;
                animationState = AnimationState.Walking;
                agent?.SetDestination(RandomSearch(transform.position, roamDistance));
            }
        }

    }
    void Chase(Vector3 targetPos)
    {
        agent.speed = runningSpeed * speedFactor;
        animationState = AnimationState.Running;
        agent?.SetDestination(targetPos);
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
        //agent.speed = 0;
        isPregnant = false;
        isBreedNow = false;
        timeSinceLastMate = 0;
        OnSpawn(this.gameObject);
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
        NavMesh.SamplePosition(randomDirection, out navHit, radius, 1);
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

    public float waterConsumptionDistance = 5f;
    /* void WaterConsumption()
     {
         waterConsumptionDistance = waterSourceReachDistance;
         if (Vector3.Distance(transform.position, waterSource.position) <= waterConsumptionDistance)
         {
             isThirsty = false;
             isWaterFound = false;
             thirst = 0;
             timeSinceLastDrink = 0;
             isChase = false;
         }

     }
     /*
     NavMeshPath path;
     path = target; 
     if (path.status == NavMeshPathStatus.PathInvalid || path.status == NavMeshPathStatus.PathPartial) {
    // Target is unreachable
 }
     */
    Vector3 waterSourcePosition()
    {
        GameObject[] sources;
        sources = GameObject.FindGameObjectsWithTag("Water");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject source in sources)
        {
            Vector3 diff = source.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = source;
                distance = curDistance;
            }
        }
        waterSource = closest.transform;
        return closest.transform.position;
    }

    ///Triggers and Collisions //Lets "false" every bool after the triggered Method() is called
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Water" && isThirsty)
        {
            target = other.gameObject.transform.position;
            isWaterFound = true;
        }
        else if (other.gameObject.tag == "Prey" && (UnityEngine.Random.Range(1f, 101f) > 99 || isHungry))
        {
            target = other.gameObject.transform.position;
            isChase = true;

        }
    }
    public float waterSourceReachDistance = 5f; //Checks if the predator is close enough to the water source
    float chaseDistance;
    public float killDistance = 3f;
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            if (Vector3.Distance(transform.position, other.gameObject.transform.position) < waterSourceReachDistance)
            {
                isThirsty = false;
                isWaterFound = false;
                thirst = 0;
                timeSinceLastDrink = 0;
                isChase = false;
            }

        }
        if (other.gameObject.tag == "Prey")
        {

            if (Vector3.Distance(transform.position, other.gameObject.transform.position) <= killDistance)
            {
                OnKill?.Invoke(other.gameObject);
                isFoodFound = false;
                hunger = 0;
                timeSinceLastMeal = 0;
                isHungry = false;
                isChase = false;
            }
            if (Vector3.Distance(transform.position, other.gameObject.transform.position) < chaseDistance && (UnityEngine.Random.Range(1f, 101f) > 6 || isHungry))
            {
                target = other.gameObject.transform.position;
                isChase = true;
            }

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
            thirst = 0;
            timeSinceLastDrink = 0;
            isChase = false;
        }
        if (collision.gameObject.tag == "Prey")
        {

            OnKill?.Invoke(collision.gameObject);
            isFoodFound = false;
            hunger = 1;
            timeSinceLastMeal = 1;
            isHungry = false;
            isChase = false;

        }

    }
}


