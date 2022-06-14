using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Prey : MonoBehaviour
{
    public static event Action OnSpawn;
    public static event Action<GameObject> OnDead;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Breed()
    {
        OnSpawn?.Invoke();
    }
    public void Death()
    {
        OnDead?.Invoke(this.gameObject);
    }

    void OnCollisionEnter(Collision other)
    {
        //if (other.gameObject.tag == "Predator") Destroy(this.gameObject);
    }

}
