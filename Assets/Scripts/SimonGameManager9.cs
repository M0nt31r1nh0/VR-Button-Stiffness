using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimonGameManager9 : MonoBehaviour
{
    public enum HardnessLevel { Soft, Normal, Hard }

    [Header("Buttons (exactly 9)")]
    [Tooltip("Assign 9 SimonButton components. Index = buttonId (0..8).")]
    [SerializeField] private SimonButton[] buttons = new SimonButton[9];

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI trialLabel;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject nextButton;

    [Header("Round Rules")]
    [SerializeField] private int baseSequenceLength = 3;   
    [SerializeField] private bool uniqueWithinRound = true;

    [Header("Timing")]
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float gapDuration = 0.3f;
    [SerializeField] private float startDelay = 1.0f;
    [SerializeField] private float errorFlashDuration = 0.5f; // New setting for error flash

    [Header("Experiment Control")]
    [SerializeField] private int roundsPerGroup = 2;
    [SerializeField] private bool shuffleGroupOrder = false;

    private int currentRoundInGroup = 0;
    private int currentGroupIndex = 0;
    private List<HardnessLevel> groupSequence;

    private enum GameMode { Idle, Listening, Playing }
    private GameMode gameMode = GameMode.Idle;

    private List<int> roundSequence = new List<int>();
    private int currentIndex = 0;
    private Coroutine roundRoutine;

    private void Start()
    {
        // Initialize buttons
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
            {
                Debug.LogError($"[SimonGameManager9] Missing button at index {i}");
                continue;
            }
            buttons[i].Init(this, i);
        }

        gameMode = GameMode.Idle;
        playButton.SetActive(true);
        trialLabel.gameObject.SetActive(false);
        nextButton.SetActive(false);
    }

    public void Play()
    {
        playButton.SetActive(false);
        trialLabel.gameObject.SetActive(true);
        nextButton.SetActive(false);

        SetupGroups();

        StartGroup();
    }

    public void StartGroup()
    {
        nextButton.SetActive(false);
        currentRoundInGroup = 0;
        HardnessLevel currentHardness = groupSequence[currentGroupIndex];
        
        Debug.Log($"[Simon] Starting Group {currentGroupIndex + 1}: {currentHardness}");

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetHardness(currentHardness);
            buttons[i].TurnOff();
        }

        StartNewRound();
    }

    private void SetupGroups()
    {
        groupSequence = new List<HardnessLevel> { HardnessLevel.Soft, HardnessLevel.Normal, HardnessLevel.Hard };

        if (shuffleGroupOrder)
        {
            for (int i = 0; i < groupSequence.Count; i++)
            {
                HardnessLevel temp = groupSequence[i];
                int randomIndex = Random.Range(i, groupSequence.Count);
                groupSequence[i] = groupSequence[randomIndex];
                groupSequence[randomIndex] = temp;
            }
        }

        currentGroupIndex = 0;
    }

    private void StartNewRound()
    {
        if (roundRoutine != null)
            StopCoroutine(roundRoutine);

        int targetLength = baseSequenceLength + currentRoundInGroup;
        
        roundSequence = GenerateRoundSequence(
            targetLength,
            buttons.Length,
            uniqueWithinRound
        );

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].TurnOff();
        }

        HardnessLevel currentHardness = groupSequence[currentGroupIndex];
        string buttonLetter = ((char)('A' + currentGroupIndex)).ToString();
        trialLabel.text = $"Group: {buttonLetter}\nTrial {currentRoundInGroup + 1} / {roundsPerGroup}";

        roundRoutine = StartCoroutine(PlayRoundSequence());
    }

    private void EndSimonGame()
    {
        Debug.Log("[Simon] Finished all rounds for this button.");

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].TurnOff();
        }

        gameMode = GameMode.Idle;
        nextButton.SetActive(true);
    }

    private void ConcludeGame()
    {
        Debug.Log("[Simon] Experiment concluded.");

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].TurnOff();
        }

        gameMode = GameMode.Idle;
        trialLabel.gameObject.SetActive(true);
        trialLabel.text = "Experiment Completed!";
        nextButton.SetActive(false);
    }

    private IEnumerator PlayRoundSequence()
    {
        gameMode = GameMode.Listening;

        yield return new WaitForSeconds(startDelay);

        foreach (int id in roundSequence)
        {
            yield return Flash(id);
            yield return new WaitForSeconds(gapDuration);
        }

        currentIndex = 0;
        gameMode = GameMode.Playing;
    }

    private IEnumerator Flash(int id)
    {
        if (id < 0 || id >= buttons.Length) yield break;

        buttons[id].TurnOn();
        yield return new WaitForSeconds(flashDuration);
        buttons[id].TurnOff();
    }

    private IEnumerator FlashErrorAndRetry()
    {
        gameMode = GameMode.Listening;
        
        Debug.Log("[Simon] Wrong button. Flashing all and replaying sequence.");

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].TurnOn();
        }

        yield return new WaitForSeconds(errorFlashDuration);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].TurnOff();
        }

        yield return new WaitForSeconds(gapDuration);

        roundRoutine = StartCoroutine(PlayRoundSequence());
    }

    public void OnPhysicalButtonPressed(int buttonId)
    {
        if (gameMode != GameMode.Playing) return;
        if (buttonId == roundSequence[currentIndex])
        {
            buttons[buttonId].TurnOn();
            currentIndex++;

            if (currentIndex >= roundSequence.Count)
            {
                currentRoundInGroup++;

                if (currentRoundInGroup >= roundsPerGroup)
                {
                    currentGroupIndex++;

                    if (currentGroupIndex >= groupSequence.Count)
                    {
                        // EndSimonGame();
                    }
                    else
                    {
                        EndSimonGame();
                    }
                }
                else
                {
                    StartNewRound();
                }
            }
        }
        else
        {
            // --- MODIFIED: Error Handling ---
            if (roundRoutine != null) StopCoroutine(roundRoutine);
            
            // Instead of resetting to Idle, we trigger the error flash and retry
            roundRoutine = StartCoroutine(FlashErrorAndRetry());
        }
    }

    private static List<int> GenerateRoundSequence(int length, int numButtons, bool unique)
    {
        length = Mathf.Clamp(length, 1, numButtons);
        var result = new List<int>(length);

        if (!unique)
        {
            for (int i = 0; i < length; i++)
                result.Add(Random.Range(0, numButtons));
            return result;
        }

        var pool = new List<int>();
        for (int i = 0; i < numButtons; i++) pool.Add(i);

        for (int i = 0; i < length; i++)
        {
            int pick = Random.Range(0, pool.Count);
            result.Add(pool[pick]);
            pool.RemoveAt(pick);
        }

        return result;
    }
}