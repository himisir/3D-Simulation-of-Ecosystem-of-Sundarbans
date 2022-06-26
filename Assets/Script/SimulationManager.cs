
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class SimulationManager : MonoBehaviour
{
    public GameObject predator;
    public GameObject prey;

    public GameObject tigerOrigin;
    public GameObject deerOrigin;
    public GameObject waterSourceTiger;
    public GameObject waterSourceDeer;

    public static event Action AgeCounter;
    public static event Action Initialize;
    public static event Action NewBornStat;
    public static event Action<GameObject, GameObject, GameObject, GameObject> Origin;
    public static event Action<int, int, int> SimulationInfo;

    public List<GameObject> waterSources = new List<GameObject>();
    public List<GameObject> preyList = new List<GameObject>();
    public List<GameObject> predatorList = new List<GameObject>();

    public bool isSimulationOn = false;
    public float dayDuration = 5;
    public float minOffset = 1;
    public float maxOffset = 5;
    public float initialDeerPopulation = 20;
    public float initialTigerPopulation = 20;
    public int days;
    public int tigerPopulation;
    public int deerPopulation;
    bool initialized;

    [Range(1f, 100f)]
    public float timeScale;
    // Start is called before the first frame update
    void Start()
    {
        timeScale = 1;
        StartCoroutine(Calender());
        //Origin?.Invoke(tigerOrigin, deerOrigin, waterSourceTiger, waterSourceDeer);
        Predator.OnSpawn += BreedTiger;
        Prey.OnSpawn += BreedDeer;
        Prey.OnDeath += DeleteDeer;
        Predator.OnKill += DeleteDeer;
        Predator.OnDeath += DeleteTiger;

    }
    void OnDestroy()
    {
        Predator.OnSpawn -= BreedTiger;
        Prey.OnSpawn -= BreedDeer;
        Prey.OnDeath -= DeleteDeer;
        Predator.OnDeath -= DeleteTiger;
        Predator.OnKill -= DeleteDeer;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;

        tigerPopulation = predatorList.Count;
        deerPopulation = preyList.Count;
        if (tigerPopulation > 0 || deerPopulation > 0)
        {
            SimulationInfo?.Invoke(days, tigerPopulation, deerPopulation);
        }
    }

    IEnumerator Calender()
    {
        while (true)
        {
            if (!initialized)
            {
                initialized = true;
                InitializeTiger();
                InitializeDeer();
            }
            AgeCounter?.Invoke();

            yield return new WaitForSeconds(dayDuration);
            days++;

        }
    }

    Vector3 RandomSearch(Vector3 origin, float distance)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
        randomDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas);
        return navHit.position;
    }

    void InitializeTiger()
    {
        for (int i = 0; i < initialTigerPopulation; i++)
        {
            Debug.Log("Tiger Spawned" + i);
            float range = UnityEngine.Random.Range(minOffset, maxOffset);
            Vector3 position = RandomSearch(tigerOrigin.transform.position, range);
            var tiger = Instantiate(predator, position, Quaternion.identity);
            tiger.GetComponent<Predator>().Initialization();
            predatorList.Add(tiger);

        }
    }
    void InitializeDeer()
    {
        for (int i = 0; i < initialDeerPopulation; i++)
        {
            Debug.Log("Deer Spawned" + i);
            float range = UnityEngine.Random.Range(-minOffset, maxOffset);
            Vector3 position = RandomSearch(deerOrigin.transform.position, range);
            var deer = Instantiate(prey, position, Quaternion.identity);
            deer.GetComponent<Prey>().Initialization();
            preyList.Add(deer);
        }
    }
    public void BreedTiger()
    {

        int literSize = UnityEngine.Random.Range(1, 2);
        for (int i = 0; i < literSize; i++)
        {
            float range = UnityEngine.Random.Range(-minOffset, maxOffset);
            Vector3 position = RandomSearch(tigerOrigin.transform.position, range);
            var tiger = Instantiate(predator, position, Quaternion.identity);
            tiger.GetComponent<Predator>().ParameterInitializeForNewBreed();
            predatorList.Add(tiger);
        }

    }
    public void BreedDeer()
    {
        int literSize = UnityEngine.Random.Range(1, 2);
        for (int i = 0; i < literSize; i++)
        {
            float range = UnityEngine.Random.Range(-minOffset, maxOffset);
            Vector3 position = RandomSearch(deerOrigin.transform.position, range);
            var deer = Instantiate(prey, position, Quaternion.identity);
            deer.GetComponent<Prey>().ParameterInitializeForNewBreed();
            preyList.Add(deer);
        }
    }

    public void DeleteTiger(GameObject tiger)
    {
        predatorList.Remove(tiger);
        Destroy(tiger);
    }
    public void DeleteDeer(GameObject deer)
    {
        preyList.Remove(deer);
        Destroy(deer);
    }
}
