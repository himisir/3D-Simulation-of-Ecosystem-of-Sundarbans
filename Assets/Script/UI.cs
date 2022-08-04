
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button simulate, configure, maniMenu, done, exit;
    [Header("UI Holder")]
    [SerializeField] private GameObject mainMenuHolder, configurationHolder, statHolder;

    [Header("Stats")]
    public TextMeshProUGUI days, tigerPopulation, deerPopulation, timeScaleStatText;
    [Header("Tiger GUI")]
    public TextMeshProUGUI tigerSpeedText, tigerVisionRadiusText, tigerLifeSpanText, tigerPregnancyPeriodText, tigerStarvationPeriodText, tigerDehydrationPeriodText;
    [Header("Deer GUI")]
    public TextMeshProUGUI deerSpeedText, deerVisionRadiusText, deerLifeSpanText, deerPregnancyPeriodText, deerDehydrationPeriodText, deerStarvationPeriodText;
    [Header("Configuration GUI")]
    public TextMeshProUGUI tigerPopulationInitText, deerPopulationInitText, dayDurationText, timeScaleText;

    [HideInInspector]
    public float tigerSpeed, tigerVisionRadius, tigerLifeSpan, tigerPregnancyPeriod, tigerDaysWithoutFood, tigerDaysWithoutWater;
    [HideInInspector]
    public float deerSpeed, deerVisionRadius, deerLifeSpan, deerPregnancyPeriod, deerDaysWithoutWater, deerDaysWithoutFood;
    [HideInInspector]
    public float tigerInitialPopulation, deerInitialPopulation, timeScale, dayDuration;

    [Header("Booleans")]
    public bool isSimulationOn, isConfigurationDone, isClickedMainMenuButton, isClickedStartButton, isClickedConfigurationButton, isClickedExitButton;
    [HideInInspector] private int day, tiger, deer;

    public static event Action<float, float, float, float, float, float> TigerProperties;
    public static event Action<float, float, float, float, float> DeerProperties;
    public static event Action<float, float, float> Configuration;
    public static event Action<float> TimeScaleCaster;

    //isSimulationOn, isConfigurationDone, isClickedMainMenuButton, isClickedStartButton, isClickedConfigurationButton isClickedExitButton;
    public static event Action<bool> Booleans;



    // Start is called before the first frame update
    void Start()
    {
        //days = GetComponent<TextMeshProUGUI>();
        //deerPopulation = GetComponent<TextMeshProUGUI>();
        //tigerPopulation = GetComponent<TextMeshProUGUI>();

        simulate.onClick.AddListener(() => Simulate());
        configure.onClick.AddListener(() => Configure());
        maniMenu.onClick.AddListener(() => MainMenu());
        done.onClick.AddListener(() => Done());
        exit.onClick.AddListener(() => Exit());

        //Panel initialization
        mainMenuHolder.SetActive(true);
        configurationHolder.SetActive(false);
        statHolder.SetActive(false);
        SimulationManager.SimulationInfo += ShowSimulationInfo;
        //Boolean initialization
        isSimulationOn = false;
        isConfigurationDone = false;
        timeScale = 1;



    }
    void OnDestroy()
    {
        SimulationManager.SimulationInfo -= ShowSimulationInfo;
    }

    public void ShowSimulationInfo(int _days, int _tigerPopulation, int _deerPopulation)
    {
        day = _days;
        tiger = _tigerPopulation;
        deer = _deerPopulation;
        this.days.text = "Age\n" + _days.ToString();
        this.tigerPopulation.text = "Tiger\n" + _tigerPopulation.ToString();
        this.deerPopulation.text = "Deer\n" + _deerPopulation.ToString();
    }

    // mainMenuHolder, configurationHolder, statHolder;

    public void Simulate()
    {
        isSimulationOn = true;
        statHolder.SetActive(true);

        mainMenuHolder.SetActive(false);
        configurationHolder.SetActive(false);

    }
    public void Configure()
    {
        isSimulationOn = false;
        configurationHolder.SetActive(true);

        mainMenuHolder.SetActive(false);
        statHolder.SetActive(false);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        isConfigurationDone = false;
        isSimulationOn = false;

        mainMenuHolder.SetActive(true);

        configurationHolder.SetActive(false);
        statHolder.SetActive(false);
    }
    public void Done()
    {
        isConfigurationDone = true;
        isSimulationOn = false;

        mainMenuHolder.SetActive(true);

        configurationHolder.SetActive(false);
        statHolder.SetActive(false);

    }
    public void Exit()
    {
        Application.Quit();
    }

    void Update()
    {
        Bridge();
    }
    /// <summary>
    /// Exchanging data in between scripts
    /// </summary>
    void Bridge()
    {
        if (isConfigurationDone) //Send it to simulation manager once data insertion is done. 
        {
            Configuration?.Invoke(tigerInitialPopulation, deerInitialPopulation, dayDuration);
            TigerProperties?.Invoke(tigerSpeed, tigerVisionRadius, tigerLifeSpan, tigerPregnancyPeriod, tigerDaysWithoutFood, tigerDaysWithoutWater);
            DeerProperties?.Invoke(deerSpeed, deerVisionRadius, deerLifeSpan, deerPregnancyPeriod, deerDaysWithoutWater);
        }
        if (isSimulationOn)
        {
            TimeScaleCaster?.Invoke(timeScale);
        }
        Booleans?.Invoke(isSimulationOn);
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
        tigerPopulationInitText.text = _value.ToString();
    }
    public void InitialPopulationDeer(float _value)
    {
        deerInitialPopulation = _value;
        deerPopulationInitText.text = _value.ToString();
    }
    public void TimeScale(float _value)
    {
        timeScale = _value;
        timeScaleText.text = _value.ToString();
    }
    public void DayDuration(float _value)
    {
        dayDuration = _value;
        dayDurationText.text = _value.ToString();
    }
    public void TimeScaleStat(float _value) //For to show the time scale in the stat menu
    {
        timeScale = _value;
        timeScaleStatText.text = _value.ToString(); //Need it to update based on the value

    }
    ///////////////////////////////
    ///////////////////////////////
}
