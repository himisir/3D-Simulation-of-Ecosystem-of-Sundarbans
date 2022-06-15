using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class Prey : MonoBehaviour
{
    public static event Action OnSpawn;
    public static event Action<GameObject> OnDead;

    NavMeshAgent agent;
    Transform origin, waterSource, enemy;
    Animator animator;
    SphereCollider visionSphere;
    public enum State
    {
        Idle,
        Walking,
        Eating,
        Running,
        Dying

    }
    State state = State.Idle;
    [Header("Properties")]
    [SerializeField]
    float newBornScale = .13f;
    public float age;
    public string gender; //male/female
    public float hunger;
    public float thirst;
    public float breedCount = 0; // Number of time breed
    public float walkingSpeed = 1f;
    public float summerSpeed = 1.5f;
    public float rainSpeed = .5f;
    public float runningSpeed = 6f;
    public float visionRadius = 10f;

    public float maxTravelLimit = 20;
    public float roamDistance = 10;
    public float eatDistance = 1;
    public float runDistance = 20;
    public float breedFactor = 1;
    public float speedFactor = 1;
    float actionTimer;
    float timeStuck = 0;
    float multiplier = 1;
    float distance;
    float distanceToEdge;
    Vector3 closestEdge;
    public float range = 20;


    [Header("Booleans")]
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
    bool isBreedNow;
    [SerializeField]
    bool hasBreed;
    [SerializeField]
    bool isAdult;
    [SerializeField]
    bool unfortunateDeath;
    [SerializeField]
    bool isAvailable;
    [SerializeField]
    bool isChase;
    [SerializeField]
    bool isRoam;
    bool switchAction;
    bool reverseFlee;



    [Header("Date from sheet")]

    [SerializeField]
    float lifeTime = 1500;
    [SerializeField]
    float maxDaysWithoutFood = 20;
    [SerializeField]
    float maxDaysWithoutDrink = 2;
    [SerializeField]
    float breedCoolDownPeriod = 150;
    [SerializeField]
    float pregnancyPeriod = 200;
    [SerializeField]
    float femaleMaturity = 250;
    [SerializeField]
    float maleMaturity = 300;



    [Header("Consumption and Reproduction")]
    [SerializeField]
    float timeSinceLastDrink = 1;
    [SerializeField]
    float timeSinceLastBreed = 1;
    [SerializeField]
    float timeSinceLastMate = 1;

    [Header("Memory Properties")]
    int memorySize = 5;
    List<Vector3> lastSafeZoneFoundPositions = new List<Vector3>();// Start is called before the first frame update
    void Start()
    {
        SimulationManager.Initialize += Initialization;
        SimulationManager.AgeCounter += CalenderSystem;
        SimulationManager.Origin += LocalRegionManager;

        visionSphere = GetComponent<SphereCollider>();
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;
        agent.stoppingDistance = 0f;
        animator = GetComponent<Animator>();

        int random = UnityEngine.Random.Range(0, 2);
        gender = (random == 1) ? "male" : "female";
        age = 1;
        state = State.Idle;
        SwitchAnimation(state);
        actionTimer = UnityEngine.Random.Range(0.2f, 2f);
    }
    void OnDestroy()
    {
        SimulationManager.Initialize -= Initialization;
        SimulationManager.AgeCounter -= CalenderSystem;
        SimulationManager.Origin -= LocalRegionManager;
    }

    void Update()
    {
        Logic();
        VisionCheck();
        Engine();

    }
    void Logic()
    {
        if (state == State.Idle)
        {
            if (actionTimer > 0)
            {
                actionTimer -= Time.deltaTime;
            }
            else
            {
                Debug.Log("Action is called");
                if (ReachedDestination())
                {
                    Roam();
                }
                actionTimer = UnityEngine.Random.Range(4f, 10f);
            }
        }



        if (isBreedNow)
        {
            Breed();
            isBreedNow = false;
        }
        if (isThirsty)
        {
            FindWater();
        }


    }
    void VisionCheck()
    {
        visionSphere.radius = visionRadius;
        visionSphere.isTrigger = true;
    }

    void Initialization()
    {
        age = 300;
        newBornScale = .13f;
        transform.localScale = new Vector3(newBornScale, newBornScale, newBornScale);
        isThirsty = false;
        timeSinceLastBreed = age;
        if (gender == "female")
        {
            isPregnant = true;
        }
        isAdult = true;
    }
    void SwitchAnimation(State state)
    {
        animator.SetBool("isRunning", state == State.Running);
        animator.SetBool("isWalking", state == State.Walking);
        animator.SetBool("isDead", state == State.Eating);

    }

    void CalenderSystem()
    {
        age++;
        timeSinceLastDrink++;
        if (isPregnant)
        {
            timeSinceLastMate++;
        }
        if (hasBreed)
        {
            timeSinceLastBreed++;
        }
    }


    void LocalRegionManager(GameObject tigerOrigin, GameObject _origin, GameObject _waterSource)
    {
        origin = _origin.transform;
        waterSource = _waterSource.transform;
    }

    public void Death()
    {
        OnDead?.Invoke(this.gameObject);
        Destroy(this.gameObject);
    }

    /// <param name="other">The other Collider involved in this collision.</param>

    private void OnTriggerEnter(Collider other)
    {


        if (other.gameObject.tag == "Predator")
        {
            Flee(other.gameObject.transform);
        }
        else if (other.gameObject.tag == "Water")
        {
            if (isThirsty)
            {
                agent.SetDestination(other.gameObject.transform.position);
            }

        }
    }
    void OnTriggerStay(Collider other)
    {

        if (other.gameObject.tag == "Predator")
        {
            Flee(other.gameObject.transform);
        }
        else if (other.gameObject.tag == "Water")
        {
            if (isThirsty)
            {
                agent.SetDestination(other.gameObject.transform.position);
            }
        }
    }



    void Flee(Transform enemy)
    {
        agent.speed = runningSpeed;
        Vector3 runTo = (transform.position - enemy.position).normalized * runDistance;
        agent.SetDestination(runTo);
        state = State.Running;
        SwitchAnimation(state);
    }



    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Water")
        {
            isThirsty = false;
            timeSinceLastDrink = 0;
            agent.ResetPath();
            //GoHome();
        }
        if (other.gameObject.tag == "Predator")
        {
            Death();
        }
    }

    public void Breed()
    {
        isPregnant = false;
        hasBreed = true;
        timeSinceLastBreed = 1;
        breedFactor = 1.3f;
        isBreedNow = false;
        OnSpawn();
    }

    void GoHome()
    {
        agent.speed = walkingSpeed * speedFactor;
        Vector3 randomPos = RandomSearch(origin.position, roamDistance);
        agent.SetDestination(origin.position + randomPos);
        state = State.Walking;
        SwitchAnimation(state);
    }
    void FindWater()
    {
        agent.speed = walkingSpeed * speedFactor;
        agent.SetDestination(waterSource.position);
        state = State.Walking;
        SwitchAnimation(state);
    }
    void Engine()
    {
        //Check if Adult
        if (gender == "male" && age > maleMaturity || gender == "female" && age > femaleMaturity)
        {
            isAdult = true;
        }
        if (timeSinceLastBreed >= breedCoolDownPeriod && !isPregnant && isAdult && !isThirsty)
        {
            isAvailable = true;
        }
        if (isPregnant && timeSinceLastMate > pregnancyPeriod)
        {
            Breed();
        }

        if (timeSinceLastDrink > 1 || thirst >= 60)
        {
            FindWater();

        }
    }

    void Roam()
    {
        Vector3 targetPos = RandomSearch(transform.position, eatDistance);
        agent.speed = walkingSpeed * speedFactor;
        agent.SetDestination(targetPos);
        state = State.Walking;
        SwitchAnimation(state);
    }
    void Eating()
    {
        agent.speed = 0;
        state = State.Idle;
        SwitchAnimation(state);
    }

    void Chase(Vector3 targetPos)
    {
        agent.speed = runningSpeed * speedFactor;
        agent.SetDestination(targetPos);
        state = State.Running;
        SwitchAnimation(state);
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

    bool ReachedDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    state = State.Idle;
                    return true;
                }
            }
        }
        return false;
    }
}
