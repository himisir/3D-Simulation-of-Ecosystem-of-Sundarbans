using System.IO;
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
    bool isAvailable; //Available for mating or breeding

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
        actionTimer = UnityEngine.Random.Range(0.1f, 2f);
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
        timeSinceLastBreed = 1;
        timeSinceLastMate = 1;
        timeSinceLastDrink = 1;
        timeSinceLastMeal = 1;

    }


    void SimulationLoop()
    {
        if (actionTimer > 0)
        {
            actionTimer -= Time.deltaTime;
        }
        else
        {
            switchAction = true;
        }
        if (switchAction)
        {
            switchAction = false;
            EventTrigger();
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



    void LocalRegionManager(GameObject tigerOrigin)
    {
        origin = tigerOrigin.transform;
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

    public void CalenderSystem()
    {
        age++;
        timeSinceLastDrink++;
        timeSinceLastMeal++;
        if (isPregnant)
        {
            timeSinceLastMate++;
        }
        else if (hasBreed)
        {
            timeSinceLastBreed++;
        }
    }
    void Update()
    {
        VisionCheck();
        Engine();
        HungerThirst();
        SimulationLoop();
    }
    void VisionCheck()
    {
        visionSphere.radius = visionRadius;
        visionSphere.isTrigger = true;
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
            isFindMate = true; //Trigger FindMate()
        }
        if (isPregnant && timeSinceLastMate > pregnancyPeriod)
        {
            isBreedNow = true;
        }

        //When hunting, if water nearby, drink
        /// Check if hungry(after 7 days usually)
        if (timeSinceLastMeal > maxDaysWithoutFood / 2 || hunger >= 60)
        {
            isHungry = true; //Trigger FindFood()
        }
        /// Check if thirsty(Drink every)
        if (timeSinceLastDrink > 1 || thirst >= 60)
        {
            isThirsty = true; //Trigger FindWater()
        }
        ////////////////////////////////

        ///Check if the predator is dead
        if (age >= lifeTime || timeSinceLastMeal >= maxDaysWithoutFood || timeSinceLastDrink >= maxDaysWithoutDrink || unfortunateDeath)
        {
            isDead = true; //Trigger Dead()
        }
        if (isGoHome && DestinationReached())
        {
            isGoHome = false;
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
            Breed();
        }
        if (isGoHome)
        {
            GoHome();
        }
    }
    ///////////////////////////////////////////////////////////////////////////////////////
    /// Methods to check if the predator is hungry, thirsty, pregnant, or dead
    void HungerThirst()
    {
        hunger += (timeSinceLastMeal / (maxDaysWithoutFood * breedFactor)) * 100;
        thirst += (timeSinceLastDrink / (maxDaysWithoutDrink * breedFactor)) * 100;
    }

    void Roam()
    {
        if (!DestinationReached()) return;
        agent.speed = walkingSpeed * speedFactor;
        state = State.Walking;
        SwitchAnimation(state);
        Vector3 targetPos = RandomSearch(transform.position, roamDistance);
        if (Vector3.Distance(origin.position, targetPos) > maxTravelLimit)
            return;
        agent.SetDestination(targetPos);
    }
    void Chase(Vector3 targetPos)
    {
        //if (!DestinationReached()) return;
        if (Vector3.Distance(origin.position, targetPos) > maxTravelLimit)
            return;
        agent.speed = runningSpeed * speedFactor;
        agent.SetDestination(targetPos);
        state = State.Running;
        SwitchAnimation(state);
    }
    void FindFood()
    {
        if (lastFoodFoundPositions.Count > 0) //&& DestinationReached()
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
        State current = state;
        state = State.Attacking;
        SwitchAnimation(state);
        state = current;
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
        breedFactor = .5f;
        OnSpawn();
        isBreedNow = false;

    }
    ////////////////////////////////////////////////////////////////
    /// Switch the animation to the given state
    void SwitchAnimation(State currentState)
    {
        animator.SetBool("isWalking", currentState == State.Walking);
        animator.SetBool("isRunning", currentState == State.Running);
        animator.SetBool("isAttacking", currentState == State.Attacking);
        //animator.SetBool("Dead", currentState == State.Dead);
    }

    ////////////////////////////////////////////////////////////////
    ///Roaming, hunting with NavMeshAgent and vision sphere


    ////////////////////////////////////////////////////////////////

    void OnTriggerStay(Collider other)

    {
        if (other.gameObject.tag == "Prey")
        {
            lastFoodFoundPositions.Add(other.transform.position);
            Chase(other.transform.position);
        }
        if (other.gameObject.tag == "Water")
        {
            lastWaterFoundPositions.Add(other.transform.position);
            if (isThirsty) Chase(other.transform.position);
        }
        if (other.gameObject.tag == "Predator")
        {
            if (other.gameObject.GetComponent<Predator>().isAcceptMate)
            {
                lastMateFoundPositions.Add(other.transform.position);
                Chase(other.gameObject.transform.position);
            }

        }

    }

    ////////////////////////////////
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Prey")
        {

            Destroy(other.gameObject);

            timeSinceLastMeal = 1;
            isHungry = false;
            if (!isThirsty)
            {
                isGoHome = true;
            }
            state = State.Attacking;
            SwitchAnimation(state);

        }
        if (other.gameObject.tag == "Predator")
        {
            if (other.gameObject.GetComponent<Predator>().isAcceptMate)
            {
                timeSinceLastMate = 1;
                isPregnant = true;
                isAvailable = false;
                isBreedNow = true;
                hasBreed = true;
                if (!isHungry && !isThirsty)
                {
                    isGoHome = true;
                }

            }
        }

        if (other.gameObject.tag == "Water")
        {
            timeSinceLastDrink = 1;
            isThirsty = false;
            if (!isHungry) isGoHome = true;
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


