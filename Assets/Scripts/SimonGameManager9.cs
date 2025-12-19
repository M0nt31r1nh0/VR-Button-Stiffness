using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonGameManager9 : MonoBehaviour
{
    [Header("Buttons (exactly 9)")]
    [Tooltip("Assign 9 SimonButton components. Index = buttonId (0..8).")]
    [SerializeField] private SimonButton[] buttons = new SimonButton[9];

    [Header("UI")]
    [SerializeField] private GameObject playButton;

    [Header("Round Rules")]
    [SerializeField] private int roundLength = 3;   // exactly 3 flashes per round
    [SerializeField] private bool uniqueWithinRound = true;

    [Header("Timing")]
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float gapDuration = 0.3f;
    [SerializeField] private float startDelay = 1.0f;

    [Header("Experiment Control")]
    [SerializeField] private int maxRounds = 2;

    private int currentRound = 0;

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
        if (playButton != null) playButton.SetActive(true);
    }

    public void Play()
    {
        if (playButton != null) playButton.SetActive(false);
        currentRound = 0;
        StartNewRound();
    }

    private void StartNewRound()
    {
        if (roundRoutine != null)
            StopCoroutine(roundRoutine);

        roundSequence = GenerateRoundSequence(
            roundLength,
            buttons.Length,
            uniqueWithinRound
        );

        roundRoutine = StartCoroutine(PlayRoundSequence());
    }

    private void EndSimonGame()
    {
        Debug.Log("[Simon] Finished all rounds for this button.");

        gameMode = GameMode.Idle;
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

    // Called by SimonButton when a physical press succeeds
    public void OnPhysicalButtonPressed(int buttonId)
    {
        if (gameMode != GameMode.Playing) return;

        // Visual feedback on press
        StartCoroutine(Flash(buttonId));

        if (buttonId == roundSequence[currentIndex])
        {
            currentIndex++;

            // Completed 3 presses → start a new round
            if (currentIndex >= roundSequence.Count)
            {
                currentRound++;

                if (currentRound >= maxRounds)
                {
                    EndSimonGame();
                }
                else
                {
                    StartNewRound();
                }
            }
        }
        else
        {
            Debug.Log("[Simon] Wrong button. Resetting.");

            gameMode = GameMode.Idle;
            if (playButton != null) playButton.SetActive(true);
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
