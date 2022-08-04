
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.AI;

public class SimulationManager : MonoBehaviour
{
    public GameObject predator;
    public GameObject prey;
    public GameObject tigerOrigin;
    public GameObject deerOrigin;
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
    string[] stats; // 0: day, 1: tiger population, 2: deer population, 3: water sources
    string filePath, fileName;
    [Range(1f, 100f)]
    public float timeScale;
    // Start is called before the first frame update
    void Start()
    {
        initialDeerPopulation = 20;
        initialTigerPopulation = 10;
        dayDuration = 24;
        timeScale = 1;
        isSimulationOn = false;
        StartCoroutine(Calender());

        fileName = "SimulationStats.csv";
        if (!File.Exists(fileName))
        {
            File.Create(fileName);
        }
        filePath = Application.dataPath + "/" + fileName;
        stats = new string[300];
        //Origin?.Invoke(tigerOrigin, deerOrigin, waterSourceTiger, waterSourceDeer);
        Predator.OnSpawn += BreedTiger;
        Prey.OnSpawn += BreedDeer;
        Prey.OnDeath += DeleteDeer;
        Predator.OnKill += DeleteDeer;
        Predator.OnDeath += DeleteTiger;
        UI.Booleans += SetBooleans;
        UI.Configuration += SetConfiguration;
        UI.TimeScaleCaster += SetTimeScale;

    }
    void OnDestroy()
    {
        Predator.OnSpawn -= BreedTiger;
        Prey.OnSpawn -= BreedDeer;
        Prey.OnDeath -= DeleteDeer;
        Predator.OnDeath -= DeleteTiger;
        Predator.OnKill -= DeleteDeer;
        UI.Booleans -= SetBooleans;
        UI.Configuration -= SetConfiguration;
        UI.TimeScaleCaster -= SetTimeScale;
    }
    void SetBooleans(bool _isSimulationOn)
    {
        isSimulationOn = _isSimulationOn;
    }
    void SetConfiguration(float _tigerInitialPopulation, float _deerInitialPopulation, float _dayDuration)
    {
        initialTigerPopulation = _tigerInitialPopulation;
        initialDeerPopulation = _deerInitialPopulation;
        dayDuration = _dayDuration;
    }
    void SetTimeScale(float _timeScale)
    {
        timeScale = _timeScale;
    }
    // Update is called once per frame
    void Update()
    {
        if (isSimulationOn)
        {
            Time.timeScale = timeScale;
            tigerPopulation = predatorList.Count;
            deerPopulation = preyList.Count;
            if (tigerPopulation > 0 || deerPopulation > 0)
            {
                SimulationInfo?.Invoke(days, tigerPopulation, deerPopulation);
            }
        }

    }

    IEnumerator Calender()
    {

        while (true)
        {
            if (isSimulationOn)
            {
                if (!initialized)
                {
                    initialized = true;
                    InitializeTiger();
                    InitializeDeer();
                }
                AgeCounter?.Invoke();
                yield return new WaitForSeconds(dayDuration);
                GenerateStats(days);
                days++;
            }
            else yield return new WaitForSeconds(.1f);

        }

    }

    Vector3 RandomSearch(Vector3 origin, float distance)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
        randomDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, distance, 1);
        return navHit.position;
    }

    void InitializeTiger()
    {
        for (int i = 0; i < initialTigerPopulation; i++)
        {
            Debug.Log("Tiger Spawned" + i);
            float range = UnityEngine.Random.Range(minOffset, maxOffset);
            Vector3 position = RandomSearch(tigerOrigin.transform.position, range);
            var tiger = Instantiate(predator, position, predator.transform.rotation);
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
            var deer = Instantiate(prey, position, prey.transform.rotation);
            deer.GetComponent<Prey>().Initialization();
            preyList.Add(deer);
        }
    }
    public void BreedTiger(GameObject animal)
    {

        int literSize = UnityEngine.Random.Range(1, 3);
        for (int i = 0; i < literSize; i++)
        {
            // float range = UnityEngine.Random.Range(-minOffset, maxOffset);
            Vector3 position = animal.transform.position;
            //Vector3 position = RandomSearch(animal.transform.position, range);
            var tiger = Instantiate(predator, position, animal.transform.rotation);
            tiger.GetComponent<Predator>().ParameterInitializeForNewBreed();
            predatorList.Add(tiger);
        }

    }
    public void GenerateStats(int i)
    {
        stats[i] = i + "," + tigerPopulation + "," + deerPopulation;
        File.WriteAllLines(filePath, stats);
    }


    public void BreedDeer(GameObject animal)
    {
        int literSize = UnityEngine.Random.Range(1, 4);
        for (int i = 0; i < literSize; i++)
        {
            //  float range = UnityEngine.Random.Range(-minOffset, maxOffset);
            Vector3 position = animal.transform.position;
            // Vector3 position = RandomSearch(animal.transform.position, range);
            var deer = Instantiate(prey, position, animal.transform.rotation);
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
