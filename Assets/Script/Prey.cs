
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class Prey : MonoBehaviour
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
    Vector3 target, fleeFrom;

    // [Header("Predator Properties")]
    public float age;
    public string gender; //male/female
    public float hunger;
    public float thirst;
    public float walkingSpeed = 10f;
    float summerSpeedFactor = 1.5f;
    float rainSpeedFactor = 0.5f;
    public float runningSpeed = 20f;
    public float fleeSpeed = 25f;
    public float visionRadius = 10f;
    public float roamDistance = 15;
    public float fleeDistance = 20;
    float breedFactor = 1;
    float speedFactor = 1;

    // [Header("Predator Booleans")]
    [SerializeField]
    bool isPregnant;
    [SerializeField]
    bool isFlee;
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
    public static event Action<GameObject> OnSpawn;

    void Start()
    {
        isSimulationOn = false;

        visionSphere = GetComponent<SphereCollider>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentState = State.Idle;
        waterSource = GameObject.FindGameObjectWithTag("Water Source Deer").transform;
        origin = GameObject.FindGameObjectWithTag("Deer Origin").transform;
        agent.speed = walkingSpeed;

        // SimulationManager.Origin += LocalRegionManager;
        SimulationManager.Initialize += Initialization;
        SimulationManager.AgeCounter += CalenderSystem;
        SimulationManager.NewBornStat += ParameterInitializeForNewBreed;
        UI.Booleans += SetBooleans;

    }
    private void OnDestroy()
    {
        SimulationManager.AgeCounter -= CalenderSystem;
        SimulationManager.Initialize -= Initialization;
        //SimulationManager.Origin -= LocalRegionManager;
        SimulationManager.NewBornStat -= ParameterInitializeForNewBreed;
        UI.Booleans -= SetBooleans;
    }

    void SetProperties(float _deerSpeed, float _deerVisionRadius, float _deerLifeSpan, float _deerPregnancyPeriod, float _deerDaysWithoutWater)
    {
        walkingSpeed = _deerSpeed;
        fleeSpeed = _deerSpeed * 2.1f;
        runningSpeed = _deerSpeed * 2;
        visionRadius = _deerVisionRadius;
        lifeTime = _deerLifeSpan;
        pregnancyPeriod = _deerPregnancyPeriod;
        maxDaysWithoutDrink = _deerDaysWithoutWater;

    }

    public bool isSimulationOn;
    void SetBooleans(bool _isSimulationOn)
    {
        isSimulationOn = _isSimulationOn;
    }
    public void ParameterInitializeForNewBreed()
    {
        int random = UnityEngine.Random.Range(0, 3);
        gender = (random == 1) ? "male" : "female";
        age = 0;
        thirst = 1;
        isPregnant = false;
        isDead = false;
        isBreedNow = false;
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
        timeSinceLastDrink = UnityEngine.Random.Range(0, maxDaysWithoutDrink);
        maxDaysWithoutDrink = UnityEngine.Random.Range(maxDaysWithoutDrink, maxDaysWithoutDrink + 3);
        if (gender == "female")
        {
            isPregnant = true;
            timeSinceLastMate = UnityEngine.Random.Range(pregnancyPeriod - 1, pregnancyPeriod);
        }
    }
    //Action Event Subscriber
    public void LocalRegionManager(GameObject tigerOrigin, GameObject deerOrigin, GameObject _waterSourceDeer, GameObject _waterSourceTiger)
    {
        if (deerOrigin != null)
        {
            origin = deerOrigin.transform;
        }
        if (_waterSourceTiger != null)
        {
            waterSource = _waterSourceDeer.transform;
        }
    }
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
        return closest.transform.position;
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
        dangerDistance = visionRadius - 2;
        visionSphere.radius = visionRadius;
        visionSphere.isTrigger = true;
    }


    void Update()
    {
        if (isSimulationOn)
        { //Debug.Log(currentState);
            VisionCheck();
            Engine();
            StateCheck();
            // SwitchState(currentState);
            SwitchAnimation(animationState);
            WaterConsumption();
        }


    }



    void Engine()
    {
        if (gender == "female" && timeSinceLastMate > pregnancyPeriod)
        {
            isBreedNow = true;
        }
        if (timeSinceLastDrink > maxDaysWithoutDrink / 2)
        {
            isThirsty = true;
        }
        if (age >= lifeTime)
        {
            Debug.Log("Died at age: " + age);
            isDead = true;
        }
        else if (timeSinceLastMeal >= maxDaysWithoutFood * 2)
        {
            Debug.Log("Died of Hunger: " + timeSinceLastMeal);
            isDead = true;
        }
        else if (timeSinceLastDrink >= maxDaysWithoutDrink * 2)
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
            case State.Flee:
                Flee(fleeFrom);
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
            //currentState = State.Dead;
        }
        else if (isFlee)
        {
            Flee(fleeFrom);
            //currentState = State.Flee;
        }
        else if (isBreedNow)
        {
            isBreedNow = false;
            Breed();

            //currentState = State.Breed;
        }
        else if (isThirsty)
        {
            Chase(waterSourcePosition());
            // currentState = State.Thirsty;
        }

        else if (isChase)
        {
            Chase(target);
            // currentState = State.Chase;
        }
        else
        {
            Idle();
            //currentState = State.Idle;
        }
    }

    float timer = 3;
    public float targetReachedDistance = 3;
    void Idle()
    {

        if (timer > 0)
        {
            animationState = AnimationState.Walking;
            timer -= Time.deltaTime;
        }
        else
        {
            if ((agent != null) && agent.remainingDistance <= agent.stoppingDistance + targetReachedDistance || isStuck)
            {
                timer = UnityEngine.Random.Range(1f, 6f);
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

        if (stuckTime > 0)
        {
            if ((agent != null) && (agent.pathPending && agent.remainingDistance > agent.stoppingDistance + targetReachedDistance) || agent.isStopped)
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

    public bool pathOutdated;
    void Roam()
    {
        if ((agent != null) && !(agent.remainingDistance <= agent.stoppingDistance + targetReachedDistance)) agent.SetDestination(agent.destination);
        else
        {
            agent.speed = walkingSpeed * speedFactor;
            animationState = AnimationState.Walking;
            agent?.SetDestination(RandomSearch(transform.position, roamDistance));
        }
    }

    void Chase(Vector3 targetPos)
    {
        agent.speed = runningSpeed * speedFactor;
        animationState = AnimationState.Running;
        agent?.SetDestination(targetPos);
    }
    void Flee(Vector3 _fleeFrom)
    {
        agent.speed = fleeSpeed * speedFactor;
        //float range = UnityEngine.Random.Range(0.5f, 1f);
        Vector3 runTo = ((transform.position - _fleeFrom)).normalized * fleeDistance;
        runTo += transform.position;
        agent?.SetDestination(runTo);
    }
    void FindWater()
    {
        target = waterSource.position;
        isChase = true;
        Debug.Log("FindWater");
    }
    public void Death()
    {
        Debug.Log("Prey is dead");
        OnDeath(this.gameObject);
    }
    public void Breed()
    {

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
    public float waterConsumptionDistance = 5f;
    void WaterConsumption()
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

    ///Triggers and Collisions //Lets "false" every bool after the triggered Method() is called
    public float dangerDistance;
    public float waterSourceReachDistance = 5f;
    public float killDistance = 1f;
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Predator")
        {
            if (Vector3.Distance(transform.position, other.transform.position) < dangerDistance)
            {
                isFlee = true;
                fleeFrom = other.transform.position;
            }

            else isFlee = false;
        }

        else if (other.gameObject.tag == "Water")
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

    }
    //Might face issues with the conditions in the if statements
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Predator")
        {
            Debug.Log("Prey is dead from collision");
            isDead = true;
        }
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
}
