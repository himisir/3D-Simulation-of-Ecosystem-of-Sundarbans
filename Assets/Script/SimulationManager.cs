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

    public static event Action AgeCounter;
    public static event Action Initialize;
    public static event Action NewBornStat;
    public static event Action<GameObject> TigerOrigin;
    public static event Action<GameObject> DeerOrigin;

    public List<GameObject> deerList = new List<GameObject>();
    public List<GameObject> predatorList = new List<GameObject>();


    public float dayDuration = 5;
    public float minOffset = 1;
    public float maxOffset = 5;
    bool initialized;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Calender());
        Predator.OnSpawn += BreedTiger;
        Predator.OnDead += DeleteTiger;

        Prey.OnSpawn += BreedDeer;
        Prey.OnDead += DeleteDeer;

    }
    void OnDestroy()
    {
        Predator.OnSpawn -= BreedTiger;
        Predator.OnDead -= DeleteTiger;
        Prey.OnSpawn -= BreedDeer;
        Prey.OnDead -= DeleteDeer;
    }

    // Update is called once per frame
    void Update()
    {
        TigerOrigin?.Invoke(tigerOrigin);
        DeerOrigin?.Invoke(deerOrigin);
    }

    IEnumerator Calender()
    {
        while (true)
        {
            if (!initialized)
            {
                initialized = true;
                Initialize?.Invoke();
            }
            AgeCounter?.Invoke();
            yield return new WaitForSeconds(dayDuration);

        }
    }


    Vector3 RandomPosition(Vector3 origin, float radius)
    {
        Vector3 newPosition = new Vector3(UnityEngine.Random.Range(-radius, radius), origin.y, UnityEngine.Random.Range(-radius, radius));

        if (!Physics.Raycast(newPosition, Vector3.down, radius)) newPosition = RandomPosition(origin, radius);

        return newPosition;
    }


    Vector3 RandomSearch(Vector3 origin, float distance)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;

        randomDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas);

        return navHit.position;
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
            deerList.Add(deer);
        }
    }

    public void DeleteDeer(GameObject deer)
    {
        deerList.Remove(deer);
        Destroy(deer);
    }

    public void DeleteTiger(GameObject tiger)
    {
        predatorList.Remove(tiger);
        Destroy(tiger);
    }
}
