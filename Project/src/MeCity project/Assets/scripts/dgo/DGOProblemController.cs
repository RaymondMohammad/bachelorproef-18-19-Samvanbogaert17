﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;

public class DGOProblemController : MonoBehaviour
{
    public Text availableWorkersCountxt;
    public Text unsolvedProblemsCountTxt;
    public Text solvedProblemsCountTxt;
    public Text moneyTxt;
    public Text happinessTxt;

    private int availableWorkers;
    private int unsolvedProblems;
    private int solvedProblems;
    private int money;
    private Slider hapSlider;
    [HideInInspector] public static float satisfaction;

    [HideInInspector] public List<Problem> problemList = new List<Problem>();
    [HideInInspector] public Dictionary<int, Problem> ongoingProblemList = new Dictionary<int, Problem>();

    //Lists of GameObjects needed for the ProblemCanvas
    private Dictionary<int, GameObject> problemPrefab = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> durationPrefab = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> deployedWorkerPrefab = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> cancelPrefab = new Dictionary<int, GameObject>();

    private int frameCounter;
    // Use this for initialization
    void Start()
    {
        hapSlider = GameObject.Find("HapSlider").GetComponent<Slider>();
        satisfaction = hapSlider.maxValue;
        hapSlider.value = satisfaction;
        GetProblemPrefabs();
    }

    // Update is called once per frame
    void Update()
    {
        happinessTxt.text = Math.Round((hapSlider.value / hapSlider.maxValue * 100)).ToString() + '%';

        if (!CameraControl.paused && !CameraControl.inQuiz)
        {
            if (ongoingProblemList.Count > 0)
            {
                for (int i = 0; i <= FindObjectOfType<DGOEventSystem>().index + 1; i++)
                {
                    if (ongoingProblemList.ContainsKey(i))
                    {
                        if (ongoingProblemList[i].durationInSeconds <= 0)
                        {
                            money = int.Parse(moneyTxt.text);
                            money += 1000 * ongoingProblemList[i].severity;
                            moneyTxt.text = money.ToString();

                            satisfaction += 1500 * ongoingProblemList[i].severity;
                            hapSlider.value = satisfaction;

                            RemoveProblem(i);
                            solvedProblems++;

                            solvedProblemsCountTxt.text = solvedProblems.ToString();
                            unsolvedProblemsCountTxt.text = unsolvedProblems.ToString();
                        }
                        else if (ongoingProblemList[i].deployedWorkers > 0)
                        {
                            if (Time.frameCount % (60 / ongoingProblemList[i].deployedWorkers) == 0)
                            {
                                ongoingProblemList[i].durationInSeconds--;
                                var timeSpan = TimeSpan.FromSeconds(ongoingProblemList[i].durationInSeconds);
                                //Leading zeroes
                                if (timeSpan.Seconds < 10)
                                {
                                    durationPrefab[i].GetComponent<Text>().text = timeSpan.Minutes + ":" + "0" + timeSpan.Seconds;
                                }
                                else
                                {
                                    durationPrefab[i].GetComponent<Text>().text = timeSpan.Minutes + ":" + timeSpan.Seconds;
                                }
                                //
                            }
                        }
                        else if (ongoingProblemList[i].deployedWorkers == 0)
                        {
                            satisfaction -= (float)ongoingProblemList[i].happinessDecrease;
                            hapSlider.value = satisfaction;
                        }
                    }
                }
            }
        }
    }

    public void AddProblem(int rndindex, int index)
    {
        ongoingProblemList.Add(index, new Problem(problemList[rndindex].id, problemList[rndindex].title, problemList[rndindex].severity, problemList[rndindex].durationInSeconds));
        FindObjectOfType<DGOProblemGridFiller>().AddProblem(index);
        unsolvedProblems++;
        unsolvedProblemsCountTxt.text = unsolvedProblems.ToString();
    }

    public void RemoveProblem(int index)
    {
        FindObjectOfType<DGOProblemGridFiller>().RemoveProblem(index);

        availableWorkers = int.Parse(availableWorkersCountxt.text);
        availableWorkers += ongoingProblemList[index].deployedWorkers;
        availableWorkersCountxt.text = availableWorkers.ToString();

        ongoingProblemList.Remove(index);

        unsolvedProblems--;
        unsolvedProblemsCountTxt.text = unsolvedProblems.ToString();
    }

    public void RemoveHappiness(int index)
    {
        satisfaction -= ongoingProblemList[index].severity * 5000;
        hapSlider.value = satisfaction;
    }

    public void AddWorker(int index)
    {
        availableWorkers = int.Parse(availableWorkersCountxt.text);
        if (availableWorkers > 0)
        {
            ongoingProblemList[index].deployedWorkers++;
            deployedWorkerPrefab[index].GetComponentInChildren<Text>().text = ongoingProblemList[index].deployedWorkers.ToString();

            availableWorkers--;
            availableWorkersCountxt.text = availableWorkers.ToString();
        }
    }
    public void RemoveWorker(int index)
    {
        availableWorkers = int.Parse(availableWorkersCountxt.text);
        if (ongoingProblemList[index].deployedWorkers > 0)
        {
            ongoingProblemList[index].deployedWorkers--;
            deployedWorkerPrefab[index].GetComponentInChildren<Text>().text = ongoingProblemList[index].deployedWorkers.ToString();

            availableWorkers++;
            availableWorkersCountxt.text = availableWorkers.ToString();
        }
    }

    public void FillProblemList()
    {
        problemList.Add(new Problem(0, "A customer needs his meterreadings taken.", 1));
        problemList.Add(new Problem(1, "A customer needs his meter replaced", 1));
        problemList.Add(new Problem(2, "A customer needs his meter removed", 1));
        problemList.Add(new Problem(3, "A customer needs his new meter installed", 1));
        problemList.Add(new Problem(4, "The grid connections needs maintaining", 2));
        problemList.Add(new Problem(5, "A high voltage transformer broke down", 3));
        problemList.Add(new Problem(6, "A customer reported a misreading and needs his readings retaken", 1));
        problemList.Add(new Problem(7, "Your employees are on strike! extra workers need to be deployed!", 2));
        problemList.Add(new Problem(8, "The GPRS-network is disrupted! online meterreadings are not possible!", 2));
        problemList.Add(new Problem(9, "A customer has a malfunctioning electricity connection", 1));
        problemList.Add(new Problem(10, "A neighbourhood substation needs maintaining", 2));
        problemList.Add(new Problem(11, "A neighbourhood substation has broken down and needs fixing", 2));
        problemList.Add(new Problem(12, "A city-wide blackout has occured!", 4));
    }

    private void GetProblemPrefabs()
    {
        problemPrefab = FindObjectOfType<DGOProblemGridFiller>().problemPrefab;
        durationPrefab = FindObjectOfType<DGOProblemGridFiller>().durationPrefab;
        deployedWorkerPrefab = FindObjectOfType<DGOProblemGridFiller>().deployedWorkerPrefab;
        cancelPrefab = FindObjectOfType<DGOProblemGridFiller>().cancelPrefab;
    }

    private void SetProblemPrefabs()
    {
        FindObjectOfType<DGOProblemGridFiller>().problemPrefab = problemPrefab;
        FindObjectOfType<DGOProblemGridFiller>().durationPrefab = durationPrefab;
        FindObjectOfType<DGOProblemGridFiller>().deployedWorkerPrefab = deployedWorkerPrefab;
        FindObjectOfType<DGOProblemGridFiller>().cancelPrefab = cancelPrefab;
    }

    public class Problem
    {
        public int id { get; set; }
        public int severity { get; set; }
        public double happinessDecrease;
        public int durationInSeconds { get; set; }
        public int deployedWorkers { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
        public Problem(int _id, string _title, int _severity, int _durationInSeconds = 60, int _deployedWorkers = 0, string _desc = "")
        {
            System.Random rnd = new System.Random();

            id = _id;
            severity = _severity;
            happinessDecrease = _severity * 10;
            if(_durationInSeconds == 60)
            {
                durationInSeconds = rnd.Next(10, _durationInSeconds * _severity);
            }
            else
            {
                durationInSeconds = _durationInSeconds;
            }
            deployedWorkers = _deployedWorkers;
            title = _title;
            desc = _desc;
        }
    }
}
