using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SimBattleManager : MonoBehaviour
{
    [Header("UI")] 
    [SerializeField]
    private TMP_Text matrixText;
    [SerializeField] 
    private TMP_Text actionText;
    [SerializeField] 
    private GameObject setupBtn;
    [SerializeField] 
    private Transform factionStatsPrefab;
    [SerializeField]
    private float factionStatsPrefabHeight;
    [SerializeField] 
    private Transform factionStatsContainer;
    [SerializeField] 
    private float spaceBetweenFactionStats;

    [Header("Simulation")] 
    [SerializeField] 
    [Tooltip("The amount of seconds to wait between each attack.")]
    private float attackInterval;

    private SimulationSetupManager setupManager;
    private int numberOfFactions;
    private List<int> warriorsPerFaction, originalWarriorsPerFaction, factions;
    private double[,] stochasticMatrix;
    private StreamWriter writer;
    private List<Transform> factionStats;

    private void Awake()
    {
        setupBtn.SetActive(false);
        setupManager = GameObject.Find("SetupManager").GetComponent<SimulationSetupManager>();
        writer = new StreamWriter("Assets/Results/output.txt", true);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupBattle();
        StartBattle();
    }
    
    private void SetupBattle()
    {
        factions = new List<int>();
        factionStats = new List<Transform>();
        numberOfFactions = setupManager.NumberOfFactions;
        warriorsPerFaction = new List<int>(setupManager.WarriorsPerFaction);
        originalWarriorsPerFaction = new List<int>(setupManager.WarriorsPerFaction);
        stochasticMatrix = setupManager.StochasticMatrix;
        
        for (int i = 0; i < numberOfFactions; i++)
        {
            var factionNumber = i + 1;
            factions.Add(factionNumber);
            CreateFactionStats(factionNumber);
        }
        
        writer.WriteLine("A new battle between " + numberOfFactions + " factions is about to start!\nThe initial stats are:\n");
        DrawMatrix();
        writer.WriteLine("");
        OutputWarriorsPerFaction();
        writer.WriteLine("====================================");
    }

    private void CreateFactionStats(int factionNumber)
    {
        Transform factionStatTransform = Instantiate(factionStatsPrefab, factionStatsContainer);
        factionStatTransform.Find("FactionFlag/FactionNumber").GetComponent<TextMeshProUGUI>().SetText(factionNumber.ToString());
        string warriorsNumber = warriorsPerFaction[factionNumber - 1].ToString() + "/" +
                                originalWarriorsPerFaction[factionNumber - 1].ToString();
        factionStatTransform.Find("WarriorsBar/WarriorsNumber").GetComponent<TextMeshProUGUI>().SetText(warriorsNumber);
        Color factionColor = Random.ColorHSV(0f, 1f, 0f, 1f, 1f, 1f);
        factionStatTransform.Find("FactionFlag").GetComponent<Image>().color = factionColor;
        factionStatTransform.Find("WarriorsBar/Bar").GetComponent<Image>().color = factionColor;
        factionStatTransform.Find("WarriorsBar/Bar").GetComponent<Image>().fillAmount = 1f;
        factionStatTransform.Find("WarriorsBar/EliminatedPanel").gameObject.SetActive(false);
        
        RectTransform factionStatRectTransform = factionStatTransform.GetComponent<RectTransform>();
        factionStatRectTransform.anchoredPosition = new Vector2(0, -factionStatsPrefabHeight * (factionNumber - 1) - spaceBetweenFactionStats );
        if (factionNumber - 1 != 0)
        {
            factionStatRectTransform.anchoredPosition -= new Vector2(0, spaceBetweenFactionStats * (factionNumber - 1));
        }
        factionStats.Add(factionStatTransform);
    }

    private void UpdateFactionStat(int index)
    {
        Transform factionStatTransform = factionStats[index];
        string warriorsNumber = warriorsPerFaction[index].ToString() + "/" +
                                originalWarriorsPerFaction[index].ToString();
        factionStatTransform.Find("WarriorsBar/WarriorsNumber").GetComponent<TextMeshProUGUI>().SetText(warriorsNumber);
        factionStatTransform.Find("WarriorsBar/Bar").GetComponent<Image>().fillAmount =
            (float)warriorsPerFaction[index] / (float)originalWarriorsPerFaction[index];

        if (warriorsPerFaction[index] == 0)
        {
            factionStatTransform.Find("WarriorsBar/EliminatedPanel").gameObject.SetActive(true);
            factionStats.Remove(factionStats[index]);
        }
    }

    private void OutputWarriorsPerFaction()
    {
        string output = null;
        writer.WriteLine("Number of warriors for each faction");
        for (int i = 0; i < numberOfFactions; i++)
        {
            if (warriorsPerFaction[i] > 0)
            {
                writer.WriteLine("Faction " + factions[i] + ": " + warriorsPerFaction[i]);
            }
            else
            {
                output = "\nFaction " + factions[i] + " is annihilated!\n";
            }
        }

        if (output != null)
        {
            writer.WriteLine(output);
            writer.WriteLine("====================================");
        }
    }

    private void DrawMatrix()
    {
        matrixText.text = "";
        for (int i = -1; i < numberOfFactions; i++)
        {
            string outputTxt = "";
            for (int j = -1; j < numberOfFactions; j++)
            {
                if (i == -1 && j == -1)
                {
                    matrixText.text += "\t";
                    outputTxt += "\t\t";
                }
                else if (i == -1)
                {
                    matrixText.text += "<b>" + factions[j] + "</b>\t";
                    outputTxt += factions[j] + "\t\t";
                }
                else if (j == -1)
                {
                    matrixText.text += "<b>" + factions[i] + "</b>\t";
                    outputTxt += factions[i] + "\t\t";
                }
                else
                {
                    matrixText.text += stochasticMatrix[i, j];
                    outputTxt += stochasticMatrix[i, j];
                }

                if (i != -1 && j != -1 && (i + 1 != numberOfFactions || j + 1 != numberOfFactions))
                {
                    if ((stochasticMatrix[i, j] * 10).ToString().Split(".").Length > 1)
                    {
                        outputTxt += "\t";
                    }
                    else
                    {
                        outputTxt += "\t\t";
                    }
                    matrixText.text += "\t";
                }
            }
            matrixText.text += "\n";
            writer.WriteLine(outputTxt);
        }
    }
    
    private IEnumerator FactionAttack()
    {
        yield return new WaitForSecondsRealtime(attackInterval);
        int attackingFaction = Random.Range(0, numberOfFactions);
        int defendingFaction = GetDefendingFaction(attackingFaction);
        actionText.text = "Faction " + factions[attackingFaction] + " attacked Faction " + factions[defendingFaction] + "!";
        writer.WriteLine("\n" + actionText.text);
        warriorsPerFaction[defendingFaction]--;
        UpdateFactionStat(defendingFaction);
        OutputWarriorsPerFaction();
        Debug.Log("Faction " + factions[defendingFaction] + " now has " + warriorsPerFaction[defendingFaction] + " warriors.");
        if (warriorsPerFaction[defendingFaction] == 0)
        {
            actionText.text = "Faction " + factions[defendingFaction] + " has been eliminated!";
            numberOfFactions--;
            //Win Condition
            if (numberOfFactions == 1)
            {
                actionText.text = "Faction " + factions[attackingFaction] + " is the winner!";
                writer.WriteLine(actionText.text);
                writer.WriteLine("====================================\n");
                writer.Close();
                setupBtn.SetActive(true);
                Destroy(setupManager.gameObject);
            }
            else
            {
                warriorsPerFaction.Remove(warriorsPerFaction[defendingFaction]);
                originalWarriorsPerFaction.Remove(originalWarriorsPerFaction[defendingFaction]);
                factions.Remove(factions[defendingFaction]);
                ReconfigureMatrix();
            }
        }
        else
        {
            StartCoroutine(nameof(FactionAttack));
        }
    }

    private int GetDefendingFaction(int attackingFaction)
    {
        int defendingFaction = attackingFaction;
        while (attackingFaction == defendingFaction)
        {
            double r = Random.Range(0f, 1f);
            double acumProb = 0f;
            double leftVal, rightVal;
            for (int i = 0; i < numberOfFactions; i++)
            {
                double attackProbability = stochasticMatrix[attackingFaction, i];
                if (i == 0)
                {
                    leftVal = 0f;
                    rightVal = attackProbability;
                }
                else
                {
                    leftVal = acumProb;
                    rightVal = acumProb + attackProbability;
                }
                
                if (leftVal < r && r <= rightVal)
                {
                    defendingFaction = i;
                    break;
                }

                acumProb += attackProbability;
            }
        }

        return defendingFaction;
    }

    private void ReconfigureMatrix()
    {
        writer.WriteLine("Reconfiguring stochastic matrix");
        stochasticMatrix = new double[numberOfFactions, numberOfFactions];
        for (int i = 0; i < numberOfFactions; i++)
        {
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
            }
        }
        DrawMatrix();
        StartCoroutine(nameof(FactionAttack));
    }
    
    private void StartBattle()
    {
        StartCoroutine(nameof(FactionAttack));
    }

    
}
