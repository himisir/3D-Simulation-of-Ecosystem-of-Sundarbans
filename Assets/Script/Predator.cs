using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Predator : MonoBehaviour
{
    public enum State
    {
        Idle,
        Walking,
        Running,
        Attacking,
        Dead
    }
    State state = State.Idle;


    [Header("Components and References")]
    Animator animator;
    NavMeshAgent agent;
    SphereCollider visionSphere;


    [Header("Predator Properties")]
    [SerializeField]
    float newBornScale;
    public float age;
    public string gender; //male/female
    public float hunger;
    public float thirst;
    public bool acceptMate; //if accepting mating offer
    public float walkingSpeed = 1f;
    public float runningSpeed = 2f;
    public float visionRadius = 10f;
    public float attackRadius = 1f;
    float scale; //
    float breedFactor; //


    [Header("Consumption and Reproduction")]

    [SerializeField]
    bool isPregnant;
    [SerializeField]
    bool isHungry;
    [SerializeField]
    bool isThirsty;
    [SerializeField]
    bool isMate;
    [SerializeField]
    bool isBreed;
    [SerializeField]
    bool isAvailable; //Available for mating or breeding
    [SerializeField]
    float timeSinceLastMeal;
    float timeSinceLastDrink;
    float timeSinceLastMate;
    float timeSinceLastBreed;
    bool findMate;
    bool isAdult;
    bool unfortunateDeath; //% will decrease over time since its birth



    ////////////////////////////////

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
    float mateMaturity = 1700;
    [SerializeField]


    ////////////////////////////////

    [Header("Memory Properties")]
    int memorySize = 5;
    List<Vector3> lastMateFoundPositions = new List<Vector3>();
    List<Vector3> lastFoodFoundPositions = new List<Vector3>();
    List<Vector3> lastWaterFoundPositions = new List<Vector3>();




    [Header("Predator Events")]
    float numberOfEvents;
    public event Action OnDead;

    void Start()
    {
        //Subscribe two events
        //AgeCount()
        //if it is first generation or not void Initialization()
        agent = new NavMeshAgent();
        animator = GetComponent<Animator>();
        Initialization();

    }

    public void Initialization()
    {
        age = 0;
        scale = newBornScale;
        gender = UnityEngine.Random.Range(0, 1) == 0 ? "male" : "female";
        hunger = 0;
        thirst = 0;
        acceptMate = false;
        isAvailable = false;
        isAdult = false;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    ////////////////////////////////
    ///<summary>
    //Predator Logic
    ///</summary>
    void Engine()
    {
        //prioritize thirst over hunger
        //When hunting, if water nearby, drink

        ///<summary>
        /// Check if hungry(after 7 days usually)
        ///</summary>
        if (timeSinceLastMeal > maxDaysWithoutFood / 2)
        {
            isHungry = true;
        }
        ///<summary>
        /// Check if thirsty(Drink every)
        ///</summary>
        if (timeSinceLastDrink > 1)
        {
            isThirsty = true;
        }
        ///<summary>
        /// Check if available for breeding
        ///</summary>
        if (timeSinceLastBreed >= breedCoolDownPeriod)
        {
            isAvailable = true;
        }
        ///<summary>
        /// Check if looking to breed
        ///if female and not hungry and not thirty and not pregnant and mature and is available then go find a partner to mate
        ///</summary>
        if (gender == "female" && !isHungry && !isThirsty && age >= femaleMaturity && !isPregnant && isAvailable)
        {
            findMate = true;
            FindMate();
        }

        ///<summary>
        ///Check if the predator is dead
        ///</summary>
        if (age >= lifeTime || timeSinceLastMeal >= maxDaysWithoutFood || timeSinceLastDrink >= maxDaysWithoutDrink || unfortunateDeath)
        {
            Dead();
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    ///<summary>
    /// Methods to check if the predator is hungry, thirsty, pregnant, or dead
    ///</summary>
    void HungerThirst()
    {
        hunger += (timeSinceLastMeal / (maxDaysWithoutFood * breedFactor)) * 100;
        thirst += (timeSinceLastDrink / (maxDaysWithoutDrink * breedFactor)) * 100;
    }

    void Walk()
    {
        agent.speed = walkingSpeed;
        agent.destination = transform.position + transform.forward * 2;
        SwitchAnimation(State.Walking);
    }
    void Chase()
    {
        agent.speed = runningSpeed;
        agent.destination = transform.position + transform.forward * 2;
        SwitchAnimation(State.Running);
    }
    void Attack()
    {
        SwitchAnimation(State.Attacking);
    }
    void Dead()
    {
        agent.speed = 0;
        SwitchAnimation(State.Dead);
    }
    void FindMate()
    {
        agent.speed = walkingSpeed;
        SwitchAnimation(State.Walking);
    }
    ////////////////////////////////////////////////////////////////
    ///<summary>
    /// Switch the animation to the given state
    ///</summary>
    void SwitchAnimation(State currentState)
    {
        animator.SetBool("Walking", currentState == State.Walking);
        animator.SetBool("Running", currentState == State.Running);
        animator.SetBool("Attacking", currentState == State.Attacking);
        animator.SetBool("Dead", currentState == State.Dead);
    }

    ///Roaming, hunting with NavMeshAgent and vision sphere



}
