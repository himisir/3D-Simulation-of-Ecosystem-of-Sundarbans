using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI : MonoBehaviour
{

    public TextMeshProUGUI days, tigerPopulation, deerPopulation;
    public TextMeshProUGUI tigerSpeedText, tigerVisionRadiusText, tigerLifeSpanText, tigerPregnancyPeriodText, tigerStarvationPeriodText, tigerDehydrationPeriodText;
    public TextMeshProUGUI deerSpeedText, deerVisionRadiusText, deerLifeSpanText, deerPregnancyPeriodText, deerDehydrationPeriodText, deerStarvationPeriodText;



    [HideInInspector]
    public float tigerSpeed, tigerVisionRadius, tigerLifeSpan, tigerPregnancyPeriod, tigerDaysWithoutFood, tigerDaysWithoutWater;

    [HideInInspector]
    public float deerSpeed, deerVisionRadius, deerLifeSpan, deerPregnancyPeriod, deerDaysWithoutWater, deerDaysWithoutFood;
    [HideInInspector]
    public float tigerInitialPopulation, deerInitialPopulation, timeScale, dayDuration;

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






    public void TigerSpeed(float _value)
    {
        tigerSpeed = _value;
        tigerSpeedText.text = _value.ToString();

    }
    public void TigerVisionRadius(float _value)
    {
        tigerVisionRadius = _value;
        tigerVisionRadiusText.text = _value.ToString();


    }
    public void TigerLifeSpan(float _value)
    {
        tigerLifeSpan = _value;
        tigerLifeSpanText.text = _value.ToString();


    }
    public void TigerPregnancyPeriod(float _value)
    {
        tigerPregnancyPeriod = _value;
        tigerPregnancyPeriodText.text = _value.ToString();

    }
    public void TigerDaysWithoutWater(float _value)
    {
        tigerDaysWithoutWater = _value;
        tigerDehydrationPeriodText.text = _value.ToString();

    }
    public void TigerDaysWithoutFood(float _value)
    {
        tigerDaysWithoutFood = _value;
        tigerStarvationPeriodText.text = _value.ToString();

    }

    ////////////////////////////
    ///////////////////////////

    public void DeerSpeed(float _value)
    {
        deerSpeed = _value;
        deerSpeedText.text = _value.ToString();
    }
    public void DeerVisionRadius(float _value)
    {
        deerVisionRadius = _value;
        deerVisionRadiusText.text = _value.ToString();
    }
    public void DeerLifeSpan(float _value)
    {
        deerLifeSpan = _value;
        deerLifeSpanText.text = _value.ToString();
    }
    public void DeerPregnancyPeriod(float _value)
    {
        deerPregnancyPeriod = _value;
        deerPregnancyPeriodText.text = _value.ToString();
    }
    public void DeerDaysWithoutWater(float _value)
    {
        deerDaysWithoutWater = _value;
        deerDehydrationPeriodText.text = _value.ToString();
    }
    public void DeerDaysWithoutFood(float _value)
    {
        deerDaysWithoutFood = _value;
        deerStarvationPeriodText.text = _value.ToString();
    }

    //////////////////////////////
    //////////////////////////////

    public void InitialPopulationTiger(float _value)
    {
        tigerInitialPopulation = _value;
    }
    public void InitialPopulationDeer(float _value)
    {
        deerInitialPopulation = _value;
    }
    public void TimeScale(float _value)
    {
        timeScale = _value;
    }
    public void DayDuration(float _value)
    {
        dayDuration = _value;
    }

    ///////////////////////////////
    ///////////////////////////////

    public void TigerInitials(float _tigerVisionRadius, float _tigerSpeed, float _tigerLifeSpan, float _tigerPregnancyPeriod, int _tigerDaysWithoutFood, int _tigerDaysWithoutWater)
    {
        tigerVisionRadius = _tigerVisionRadius;
        tigerSpeed = _tigerSpeed;
        tigerLifeSpan = _tigerLifeSpan;
        tigerPregnancyPeriod = _tigerPregnancyPeriod;
        tigerDaysWithoutFood = _tigerDaysWithoutFood;
        tigerDaysWithoutWater = _tigerDaysWithoutWater;
    }
    public void DeerInitials(float _deerVisionRadius, float _deerSpeed, float _deerLifeSpan, float _deerPregnancyPeriod, int _deerDaysWithoutWater)
    {
        deerVisionRadius = _deerVisionRadius;
        deerSpeed = _deerSpeed;
        deerLifeSpan = _deerLifeSpan;
        deerPregnancyPeriod = _deerPregnancyPeriod;
        deerDaysWithoutWater = _deerDaysWithoutWater;
    }
    public void SimulationInitials(float _tigerPopulationInit, float _deerPopulationInit, float _timeScale, float _dayDuration)
    {
        tigerInitialPopulation = _tigerPopulationInit;
        deerInitialPopulation = _deerPopulationInit;
        timeScale = _timeScale;
        dayDuration = _dayDuration;
    }



}
