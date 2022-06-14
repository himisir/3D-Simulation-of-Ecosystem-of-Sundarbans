using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Predator : MonoBehaviour
{
    [Header("Predator Events")]
    float numberOfEvents;
    public static event Action<GameObject> OnDead;
    public static event Action OnSpawn;

    public enum State
    {
        Idle,
        Walking,
        Running,
        Attacking,
        Dead
    };
    State state = State.Idle;
    [Header("Simulation Properties")]
    float actionTimer = 0;
    bool switchAction;

    [Header("Components and References")]
    Animator animator;
    NavMeshAgent agent;
    SphereCollider visionSphere;
    Transform origin;

    [Header("Predator Properties")]
    [SerializeField]
    float newBornScale = 1;
    public float age;
    public string gender; //male/female
    public float hunger;
    public float thirst;
    public float breedCount = 0; // Number of time breed
    public float walkingSpeed = 1f;
    public float pregnantSpeed = .7f;
    public float summerSpeed = 1.5f;
    public float rainSpeed = .5f;
    public float runningSpeed = 2f;
    public float visionRadius = 10f;
    public float attackRadius = 1f;
    public float maxTravelLimit = 50;
    public float roamDistance = 15;
    public float breedFactor = 1;

    float scale; //
    float speedFactor = 1;

    [Header("Predator Booleans")]
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
    bool isGoHome;
    [SerializeField]
    bool isAcceptMate; //if accepting mating offer
    [SerializeField]
    bool isBreedNow;
    [SerializeField]
    bool hasBreed;
    [SerializeField]
    bool isAdult;
    [SerializeField]
    bool unfortunateDeath;
    [SerializeField]
    bool isFindMate;
    [SerializeField]
    bool isAvailable;
    [SerializeField]
    bool isChase;
    [SerializeField]
    bool isRoam;
    [SerializeField]
    bool chaseIndicator;

    [Header("Consumption and Reproduction")]
    [SerializeField]
    float timeSinceLastMeal = 1;
    [SerializeField]
    float timeSinceLastDrink = 1;
    [SerializeField]
    float timeSinceLastMate = 1;
    [SerializeField]
    float timeSinceLastBreed = 1; //% will decrease over time since its birth

    [Header("Date from sheet")]

    [SerializeField]
    float lifeTime = 3000;
    [SerializeField]
    float maxDaysWithoutFood = 14;
    [SerializeField]
    float maxDaysWithoutDrink = 4;
    [SerializeField]
    float breedCoolDownPeriod = 2500;
    [SerializeField]
    float pregnancyPeriod = 110;
    [SerializeField]
    float femaleMaturity = 1095;
    [SerializeField]
    float maleMaturity = 1700;
    [SerializeField]

    ////////////////////////////////

    [Header("Memory Properties")]
    int memorySize = 5;
    List<Vector3> lastMateFoundPositions = new List<Vector3>();
    List<Vector3> lastFoodFoundPositions = new List<Vector3>();
    List<Vector3> lastWaterFoundPositions = new List<Vector3>();



    void Start()
    {
        SimulationManager.Initialize += Initialization;
        SimulationManager.AgeCounter += CalenderSystem;
        SimulationManager.TigerOrigin += LocalRegionManager;
        SimulationManager.NewBornStat += ParameterInitializeForNewBreed;


        visionSphere = GetComponent<SphereCollider>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        int random = UnityEngine.Random.Range(0, 2);
        gender = (random == 1) ? "male" : "female";
        state = State.Idle;
        actionTimer = UnityEngine.Random.Range(0.5f, 2f);
        SwitchAnimation(state);

    }
    private void OnDestroy()
    {
        SimulationManager.AgeCounter -= CalenderSystem;
        SimulationManager.Initialize -= Initialization;
        SimulationManager.TigerOrigin -= LocalRegionManager;
        SimulationManager.NewBornStat -= ParameterInitializeForNewBreed;
    }
    public void ParameterInitializeForNewBreed()
    {

        age = 0;
        actionTimer = 0;
        switchAction = false;
        newBornScale = 0.1f;
        hunger = 0;
        thirst = 0;
        breedCount = 0;
        breedFactor = 0;
        walkingSpeed = 1f;
        pregnantSpeed = .7f;
        summerSpeed = 1.5f;
        rainSpeed = .5f;
        runningSpeed = 2f;
        visionRadius = 10f;
        attackRadius = 1f;
        maxTravelLimit = 20;
        roamDistance = 15;
        breedFactor = 1;
        speedFactor = 1;
        isPregnant = false;
        isHungry = false;
        isDead = false;
        isMate = false;
        isGoHome = false;
        isAcceptMate = false;
        isBreedNow = false;
        hasBreed = false;
        isAdult = false;
        unfortunateDeath = false;
        isFindMate = false;
        isAvailable = false;
        isChase = false;
        isRoam = false;
        chaseIndicator = false;
        timeSinceLastBreed = 1;
        timeSinceLastMate = 1;
        timeSinceLastDrink = 1;
        timeSinceLastMeal = 1;

    }
    public void Initialization()
    {
        age = 1800;
        scale = 1;
        transform.localScale = new Vector3(scale, scale, scale);
        hunger = 0;
        thirst = 0;
        timeSinceLastBreed = age;
        if (gender == "male") isAcceptMate = true;
        else
        {
            isAvailable = true;
            isFindMate = true;
        }
        isAdult = true;
    }


    void SimulationLoop()
    {
        if (actionTimer > 0)
        {
            actionTimer -= Time.deltaTime;
        }
        else
        {
            if (switchAction)
            {
                switchAction = false;
                EventTrigger();
            }
        }


    }
    bool DestinationReached()
    {
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


    //Action Event Subscriber
    void LocalRegionManager(GameObject tigerOrigin)
    {
        origin = tigerOrigin.transform;
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
        else if (hasBreed)
        {
            timeSinceLastBreed++;
        }
    }
    void VisionCheck()
    {
        visionSphere.radius = visionRadius;
        visionSphere.isTrigger = true;
    }

    void Update()
    {
        VisionCheck();
        Engine();
        EventTrigger();
        //SimulationLoop();
    }


    ////////////////////////////////

    void Engine()
    {
        //Check if Adult
        if (gender == "male" && age > maleMaturity || gender == "female" && age > femaleMaturity)
        {
            isAdult = true;
        }
        //If male is ready to mate
        if (gender == "male" && age >= maleMaturity) isAcceptMate = true;
        //If female is ready to breed
        if (timeSinceLastBreed >= breedCoolDownPeriod && !isPregnant && isAdult && !isHungry && !isThirsty)
        {
            isAvailable = true;
        }
        //Find mate if all is clear
        if (gender == "female" && isAvailable)
        {
            FindMate();
            //isFindMate = true; //Trigger FindMate()
        }
        if (isPregnant && timeSinceLastMate > pregnancyPeriod)
        {
            //isBreedNow = true;
            Breed();
        }
        //When hunting, if water nearby, drink
        /// Check if hungry(after 7 days usually)
        if (timeSinceLastMeal > maxDaysWithoutFood / 2 || hunger >= 60)
        {
            //isHungry = true; //Trigger FindFood()
            FindFood();
        }
        /// Check if thirsty(Drink every)
        if (timeSinceLastDrink > 2 || thirst >= 60)
        {
            //isThirsty = true;  //Trigger FindWater()
            FindWater();

        }
        ////////////////////////////////
        ///Check if the predator is dead
        if (age >= lifeTime || timeSinceLastMeal >= maxDaysWithoutFood || timeSinceLastDrink >= maxDaysWithoutDrink || unfortunateDeath)
        {
            //Dead();
            // isDead = true; //Trigger Dead()
        }
        if (isGoHome && DestinationReached())
        {
            GoHome();
            //isGoHome = false;
        }
    }

    void EventTrigger()
    {
        if (isThirsty)
        {
            FindWater();
        }
        if (isHungry)
        {
            FindFood();
        }
        if (isFindMate)
        {
            FindMate();
        }
        if (isDead)
        {
            // Dead();
        }
        if (isBreedNow)
        {
            Debug.Log("Breed is triggered");
            Breed();
        }
        if (isGoHome)
        {
            GoHome();
        }
        if (isRoam || DestinationReached() || isStuck())
        {
            Roam();
        }
    }


    bool isStuck()
    {
        float stuckTime = 2f;
        while (stuckTime > 0)
        {
            if (!(agent.velocity.sqrMagnitude <= 0.1f))
            {
                return false;
            }
            stuckTime -= Time.deltaTime;
        }
        return true;
    }
    ///////////////////////////////////////////////////////////////////////////////////////
    /// Methods to check if the predator is hungry, thirsty, pregnant, or dead
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
        //agent.stoppingDistance = 0;
        if (!DestinationReached()) return;
        //if (!DestinationReached() || chaseIndicator) return;
        float timer = UnityEngine.Random.Range(.5f, .9f);
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            state = State.Idle;
            SwitchAnimation(state);
        }
        Vector3 targetPos = RandomSearch(transform.position, roamDistance);
        /*
        while ((Vector3.Distance(origin.position, targetPos) > maxTravelLimit))
        {
            targetPos = RandomSearch(transform.position, roamDistance);
        }
        */
        agent.speed = walkingSpeed * speedFactor;
        state = State.Walking;
        SwitchAnimation(state);
        agent.SetDestination(targetPos);



    }
    void Chase(Vector3 targetPos)
    {
        chaseIndicator = true;
        agent.speed = runningSpeed * speedFactor;
        agent.SetDestination(targetPos);
        state = State.Running;
        SwitchAnimation(state);
        /*
        if (Vector3.Distance(origin.position, targetPos) > maxTravelLimit)
        {
            agent.SetDestination(transform.position * (maxTravelLimit - Vector3.Distance(transform.position, origin.position)));
            //Return perhaps. Or just roam again
        }
        */
    }
    void FindFood()
    {
        if (lastFoodFoundPositions.Count > 0)
        {
            Vector3 closestFood = lastFoodFoundPositions[0];
            float closestDistance = Vector3.Distance(transform.position, closestFood);
            foreach (Vector3 food in lastFoodFoundPositions)
            {
                float distance = Vector3.Distance(transform.position, food);
                if (distance < closestDistance)
                {
                    closestFood = food;
                    closestDistance = distance;
                }
            }
            Chase(closestFood);
        }
        else
        {
            Roam();
        }
    }
    void FindWater()
    {
        if (lastWaterFoundPositions.Count > 0) //&& DestinationReached()
        {
            Vector3 closestWater = lastWaterFoundPositions[0];
            float closestDistance = Vector3.Distance(transform.position, closestWater);
            foreach (Vector3 water in lastWaterFoundPositions)
            {
                float distance = Vector3.Distance(transform.position, water);
                if (distance < closestDistance)
                {
                    closestWater = water;
                    closestDistance = distance;
                }
            }
            Chase(closestWater);
        }
        else
        {
            Roam();
        }
    }

    void FindMate()
    {
        if (lastMateFoundPositions.Count > 0)
        {
            Vector3 closestMate = lastMateFoundPositions[0];
            float closestDistance = Vector3.Distance(transform.position, closestMate);
            foreach (Vector3 mate in lastMateFoundPositions)
            {
                float distance = Vector3.Distance(transform.position, mate);
                if (distance < closestDistance)
                {
                    closestMate = mate;
                    closestDistance = distance;
                }
            }
            Chase(closestMate);
        }
        else
        {
            Roam();
        }

    }

    void Attack()
    {
        state = State.Attacking;
        SwitchAnimation(state);
    }
    void Dead()
    {
        OnDead(this.gameObject);
        agent.speed = 0;
    }

    void GoHome()
    {
        agent.speed = walkingSpeed * speedFactor;
        Vector3 randomPos = RandomSearch(origin.position, roamDistance);
        agent.SetDestination(origin.position + randomPos);
        state = State.Walking;
        SwitchAnimation(state);
    }

    public void Breed()
    {
        isPregnant = false;
        hasBreed = true;
        timeSinceLastBreed = 0;
        breedFactor = 1.5f;
        isBreedNow = false;
        OnSpawn();


    }
    ////////////////////////////////////////////////////////////////
    /// Switch the animation to the given state
    void SwitchAnimation(State currentState)
    {
        animator.SetBool("isWalking", currentState == State.Walking);
        animator.SetBool("isRunning", currentState == State.Running);
        animator.SetBool("isAttacking", currentState == State.Attacking);
    }

    ////////////////////////////////////////////////////////////////
    ///Roaming, hunting with NavMeshAgent and vision sphere
    void OnTriggerEnter(Collider other)

    {
        if (other.gameObject.tag == "Prey")
        {
            AddToList(lastFoodFoundPositions, other.transform.position);
            if (!isHungry)
            {
                Chase(other.transform.position);
            }

        }
        if (other.gameObject.tag == "Water")
        {
            AddToList(lastWaterFoundPositions, other.transform.position);
            if (isThirsty)
            {
                Chase(other.transform.position);
            }
        }
        if (other.gameObject.tag == "Predator")
        {
            if (other.gameObject.GetComponent<Predator>().isAcceptMate)
            {
                Chase(other.transform.position);
                AddToList(lastMateFoundPositions, other.transform.position);
            }

        }

    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Prey" || other.gameObject.tag == "Water" || other.gameObject.tag == "Predator" && other.gameObject.GetComponent<Predator>().isAcceptMate)
        {
            Chase(other.transform.position);
        }

    }
    void OnTriggerExit(Collider other)
    {
        agent.isStopped = true;
        agent.ResetPath();
        Roam();
        //if (DestinationReached()) Roam();
        //Should also send another signal that make it room until something else happens;
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
    void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision");
        if (other.gameObject.tag == "Prey")
        {
            Attack();
            Destroy(other.gameObject);
            timeSinceLastMeal = 1;
            isHungry = false;
            agent.isStopped = true;
            agent.ResetPath();
            GoHome();
        }
        if (other.gameObject.tag == "Water")
        {
            timeSinceLastDrink = 1;
            isThirsty = false;
            agent.isStopped = true;
            agent.ResetPath();
            GoHome();
        }

        if (other.gameObject.tag == "Predator" && other.gameObject.GetComponent<Predator>().isAcceptMate)
        {
            Breed();
            agent.isStopped = true;
            agent.ResetPath();
            GoHome();
            timeSinceLastMate = 1;
            isPregnant = true;
            isAvailable = false;
            hasBreed = true;
        }

    }

    Vector3 RandomSearch(Vector3 currentPos, float radius)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += currentPos;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, radius, NavMesh.AllAreas);
        if (!navHit.hit)
        {
            RandomSearch(currentPos, radius);
        }
        return navHit.position;
    }


    void PregnantStat()
    {
        speedFactor = pregnantSpeed; //Have to multiply it with speedFactor; 
    }
    void SummerStat()
    {
        speedFactor = pregnantSpeed;
    }
    void RainStat()
    {
        speedFactor = pregnantSpeed;
    }

    void CheckTask()
    {
        PregnantStat();
        SummerStat();
        RainStat();
    }

}


