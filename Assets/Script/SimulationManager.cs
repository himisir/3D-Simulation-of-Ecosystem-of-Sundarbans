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
    public GameObject waterSource;

    public static event Action AgeCounter;
    public static event Action Initialize;
    public static event Action NewBornStat;
    public static event Action<GameObject, GameObject, GameObject> Origin;
    public static event Action<int, int, int> SimulationInfo;

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
        Predator.OnSpawn += BreedTiger;
        Predator.OnDeath += DeleteTiger;
        Prey.OnSpawn += BreedDeer;
        Prey.OnDeath += DeleteDeer;

    }
    void OnDestroy()
    {
        Predator.OnSpawn -= BreedTiger;
        Predator.OnDeath -= DeleteTiger;
        Prey.OnSpawn -= BreedDeer;
        Prey.OnDeath -= DeleteDeer;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
        Origin?.Invoke(tigerOrigin, deerOrigin, waterSource);
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
            days++;
            yield return new WaitForSeconds(dayDuration);

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
            float range = UnityEngine.Random.Range(minOffset * 2, maxOffset * 3);
            Vector3 position = RandomSearch(tigerOrigin.transform.position, range);
            var tiger = Instantiate(predator, position, Quaternion.identity);
            predatorList.Add(tiger);
            Initialize?.Invoke();
        }
    }
    void InitializeDeer()
    {
        for (int i = 0; i < initialDeerPopulation; i++)
        {
            float range = UnityEngine.Random.Range(minOffset * 2, maxOffset * 3);
            Vector3 position = RandomSearch(deerOrigin.transform.position, range);
            var deer = Instantiate(prey, position, Quaternion.identity);
            Initialize?.Invoke();
            preyList.Add(deer);
        }
    }

    public void BreedTiger()
    {

        int literSize = UnityEngine.Random.Range(1, 2);
        for (int i = 0; i < literSize; i++)
        {
            float range = UnityEngine.Random.Range(minOffset, maxOffset);
            Vector3 position = RandomSearch(tigerOrigin.transform.position, range);
            var tiger = Instantiate(predator, position, Quaternion.identity);
            NewBornStat?.Invoke();
            predatorList.Add(tiger);
        }

    }
    public void BreedDeer()
    {
        int literSize = UnityEngine.Random.Range(2, 4);
        for (int i = 0; i < literSize; i++)
        {
            float range = UnityEngine.Random.Range(minOffset, maxOffset);
            Vector3 position = RandomSearch(deerOrigin.transform.position, range);
            var deer = Instantiate(prey, position, Quaternion.identity);
            NewBornStat?.Invoke();
            preyList.Add(deer);
        }
    }

    public void DeleteDeer(GameObject deer)
    {
        Debug.Log("Deer is dead");
        preyList.Remove(deer);
        Destroy(deer);
    }

    public void DeleteTiger(GameObject tiger)
    {
        predatorList.Remove(tiger);
        Destroy(tiger);
    }
}
