using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SimulationSetupManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_InputField factionsNumberInput;
    [SerializeField]
    private TMP_InputField warriorsNumberInput;
    [SerializeField]
    private TMP_InputField matrixInput;

    [Header("Setup Options")] 
    [SerializeField] private int MaxNumberOfFactions;
    [SerializeField] private int MaxWarriorsPerFaction;

    private int numberOfFactions;
    private List<int> warriorsPerFaction;
    private double[,] stochasticMatrix;
    
    public int NumberOfFactions => numberOfFactions;
    public List<int> WarriorsPerFaction => warriorsPerFaction;
    public double[,] StochasticMatrix => stochasticMatrix;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        warriorsPerFaction = new List<int>();
    }

    public void GenerateRandomConfiguration()
    {
        factionsNumberInput.text = "";
        warriorsNumberInput.text = "";
        matrixInput.text = "";
        
        numberOfFactions = Random.Range(2, MaxNumberOfFactions);
        factionsNumberInput.text = numberOfFactions.ToString();
        stochasticMatrix = new double[numberOfFactions,numberOfFactions];
        for (int i = 0; i < numberOfFactions; i++)
        {
            int numOfWarriors = Random.Range(1, MaxWarriorsPerFaction);
            warriorsPerFaction.Add(numOfWarriors);
            warriorsNumberInput.text += numOfWarriors;
            if (i + 1 < numberOfFactions)
            {
                warriorsNumberInput.text += ", ";
            }

            double probOfAttack = 0f;
            float acumProb = 0f;
            for (int j = 0; j < numberOfFactions; j++)
            {
                
                if (i == j)
                {
                    probOfAttack = 0f;
                }
                else if (j + 1 == numberOfFactions || (j + 1 == i && j + 2 > numberOfFactions - 1))
                {
                    probOfAttack = Math.Round(1f - acumProb, 2);
                }
                else
                {
                    probOfAttack = Math.Round(Random.Range(0f, 1f - acumProb), 2);
                }
                acumProb += (float) probOfAttack;

                stochasticMatrix[i, j] = probOfAttack;
                matrixInput.text += probOfAttack;
                if (i + 1 != numberOfFactions || j + 1 != numberOfFactions)
                {
                    matrixInput.text += ", ";
                }
            }

            matrixInput.text += "\n";
        }
    }

    public void SetupInitialConfiguration()
    {
        numberOfFactions = int.Parse(factionsNumberInput.text);
        warriorsPerFaction = warriorsNumberInput.text.Split(",").Select(i => int.Parse(i)).ToList();
        stochasticMatrix = new double[numberOfFactions,numberOfFactions];
        var matrixRows = matrixInput.text.Split("\n");
        for (int i = 0; i < numberOfFactions; i++)
        {
            var currentRowNumbers = matrixRows[i].Split(",");
            for (int j = 0; j < numberOfFactions; j++)
            {
                stochasticMatrix[i, j] = double.Parse(currentRowNumbers[j]);
            }
        }
    }
}
