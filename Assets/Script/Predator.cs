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
    public float maxTravelLimit = 20;
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
    bool isAcceptMate; //if accepting mating offer
    [SerializeField]
    bool isBreed;
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
    float timeSinceLastMeal;
    [SerializeField]
    float timeSinceLastDrink;
    [SerializeField]
    float timeSinceLastMate;
    [SerializeField]
    float timeSinceLastBreed; //% will decrease over time since its birth

    [Header("Date from sheet")]

    [SerializeField]
    float lifeTime = 3000;
    [SerializeField]
    float maxDaysWithoutFood = 14;
    [SerializeField]
    float maxDaysWithoutDrink = 3;
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
        visionSphere = GetComponent<SphereCollider>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        int random = UnityEngine.Random.Range(0, 2);
        gender = (random == 1) ? "male" : "female";

    }
    private void OnDestroy()
    {
        SimulationManager.AgeCounter -= CalenderSystem;
        SimulationManager.Initialize -= Initialization;
        SimulationManager.TigerOrigin -= LocalRegionManager;
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
        else if (isBreed)
        {
            timeSinceLastBreed++;
        }
    }
    void Update()
    {
        VisionCheck();
        Engine();
        HungerThirst();
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

        //When hunting, if water nearby, drink
        /// Check if hungry(after 7 days usually)
        if (timeSinceLastMeal > maxDaysWithoutFood / 2)
        {
            isHungry = true; //Trigger FindFood()
        }
        /// Check if thirsty(Drink every)
        if (timeSinceLastDrink > 1)
        {
            isThirsty = true; //Trigger FindWater()
        }
        ////////////////////////////////

        ///Check if the predator is dead
        if (age >= lifeTime || timeSinceLastMeal >= maxDaysWithoutFood || timeSinceLastDrink >= maxDaysWithoutDrink || unfortunateDeath)
        {
            isDead = true; //Trigger Dead()
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
            Dead();
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
        agent.speed = walkingSpeed * speedFactor;
        SwitchAnimation(State.Walking);
        Vector3 targetPos = RandomSearch(transform.position, roamDistance);
        if (Vector3.Distance(origin.position, targetPos) > maxTravelLimit)
            return;
        agent.SetDestination(targetPos);
    }
    void Chase(Vector3 targetPos)
    {
        if (Vector3.Distance(origin.position, targetPos) > maxTravelLimit)
            return;
        agent.speed = runningSpeed * speedFactor;
        agent.SetDestination(targetPos);

        SwitchAnimation(State.Running);
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
        SwitchAnimation(State.Attacking);
    }
    void Dead()
    {
        OnDead(this.gameObject);
        agent.speed = 0;
        SwitchAnimation(State.Dead);
    }

    void GoHome()
    {
        agent.speed = walkingSpeed * speedFactor;
        Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-roamDistance, roamDistance), 0, UnityEngine.Random.Range(-roamDistance, roamDistance));
        agent.SetDestination(origin.position + randomPos);
        animator.SetBool("isWalking", true);
    }

    public void Breed()
    {
        GoHome();
        isBreed = true;
        timeSinceLastBreed = 0;
        SwitchAnimation(State.Idle);
        OnSpawn();

    }
    ////////////////////////////////////////////////////////////////
    /// Switch the animation to the given state
    void SwitchAnimation(State currentState)
    {
        animator.SetBool("Walking", currentState == State.Walking);
        animator.SetBool("Running", currentState == State.Running);
        animator.SetBool("Attacking", currentState == State.Attacking);
        animator.SetBool("Dead", currentState == State.Dead);
    }

    ////////////////////////////////////////////////////////////////
    ///Roaming, hunting with NavMeshAgent and vision sphere

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

        //
        if (switchAction)
        {

        }

    }

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
            Attack();
            timeSinceLastMeal = 0;
            isHungry = false;
        }
        if (other.gameObject.tag == "Predator")
        {
            if (other.gameObject.GetComponent<Predator>().isAcceptMate)
            {
                timeSinceLastMate = 0;
                isPregnant = true;
                isAvailable = false;
            }
        }

        if (other.gameObject.tag == "Water")
        {
            timeSinceLastDrink = 0;
            isThirsty = false;
        }


    }

    Vector3 RandomSearch(Vector3 currentPos, float radius)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += currentPos;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, radius, NavMesh.AllAreas);
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


