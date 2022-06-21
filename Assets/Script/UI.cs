using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI days, tigerPopulation, deerPopulation;
    // Start is called before the first frame update
    void Start()
    {
        //days = GetComponent<TextMeshProUGUI>();
        //deerPopulation = GetComponent<TextMeshProUGUI>();
        //tigerPopulation = GetComponent<TextMeshProUGUI>();
        SimulationManager.SimulationInfo += ShowSimulationInfo;

    }
    void OnDestroy()
    {
        SimulationManager.SimulationInfo -= ShowSimulationInfo;
    }

    int day, tiger, deer;
    public void ShowSimulationInfo(int _days, int _tigerPopulation, int _deerPopulation)
    {
        day = _days;
        tiger = _tigerPopulation;
        deer = _deerPopulation;
        this.days.text = "Age\n" + _days.ToString();
        this.tigerPopulation.text = "Tiger\n" + _tigerPopulation.ToString();
        this.deerPopulation.text = "Deer\n" + _deerPopulation.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        // ShowSimulationInfo(day, deer, tiger);
    }

}
