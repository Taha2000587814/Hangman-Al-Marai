using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HangmanManager : MonoBehaviour
{
    // UI Elements
    public TMP_Text[] letterSlots; // Array to hold TMP objects for each letter
    public int maxAttempts = 6; // Max incorrect guesses

    // Words to guess
    private string fullSentence = "milkeverydayisthesmartway"; // Combined sentence (no spaces)
    private char[] displayedWord;
    private int incorrectAttempts = 0;

    // Milk Meter Progression
    public Animator pourAnimator; // Milk pouring animation
    public GameObject[] milkMeters; // Array of milk meter objects (low to high)
    private int currentMilkState = 0;
    private int correctGuesses = 0;
    private int totalMissingLetters = 7; // Number of required guesses

    // Correct Guess Reactions
    public GameObject cowHappy;
    public GameObject cowNormal;
    public GameObject happyKids;
    public GameObject idleKids;
    public AudioSource correctAudio;

    // Incorrect Guess Reactions
    public GameObject sadKids;
    public GameObject cowAngry;
    public AudioSource incorrectAudio;

    // Keyboard Input
    public GameObject[] keyboardButtons; // Array of keyboard buttons (A-Z)

    void Start()
    {
        AssignLetterValues();
        InitializeGame();
    }

    void AssignLetterValues()
    {
        if (keyboardButtons == null || keyboardButtons.Length < 26) // Ensure 26 buttons exist
        {
            Debug.LogError("ERROR: Keyboard buttons array is either null or missing elements! Assign all buttons in Unity.");
            return;
        }

        char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        for (int i = 0; i < keyboardButtons.Length; i++)
        {
            if (i >= letters.Length) // Prevent index out of range
            {
                Debug.LogError($"ERROR: Button index {i} is out of range for letters array.");
                break; // Stop loop to avoid crashes
            }

            if (keyboardButtons[i] == null)
            {
                Debug.LogError($"ERROR: Missing button at index {i}! Assign it in Inspector.");
                continue;
            }

            keyboardButtons[i].GetComponent<Button>().onClick.AddListener(() => OnLetterPressed(letters[i].ToString()));

            Debug.Log($"Assigned button {i} to letter: {letters[i]}");
        }
    }


    void InitializeGame()
    {
        displayedWord = fullSentence.ToCharArray();
        HideRandomLetters(displayedWord, 7); // Hide 7 letters in the sentence
        UpdateWordDisplay();
    }

    void HideRandomLetters(char[] displayedWord, int numToHide)
    {
        System.Random random = new System.Random();
        HashSet<int> hiddenIndexes = new HashSet<int>();

        while (hiddenIndexes.Count < numToHide)
        {
            int index = random.Next(displayedWord.Length);
            if (!hiddenIndexes.Contains(index))
            {
                displayedWord[index] = ' '; // Hide this letter
                hiddenIndexes.Add(index);
            }
        }
    }

    public void OnLetterPressed(string letter)
    {
        Debug.Log("Pressed letter: " + letter); // Debugging key press detection

        char guess = letter[0];
        bool correctGuess = false;

        for (int i = 0; i < displayedWord.Length; i++)
        {
            if (fullSentence[i] == guess && displayedWord[i] == ' ')
            {
                displayedWord[i] = guess; // Reveal hidden letter
                correctGuess = true;
            }
        }

        if (!correctGuess)
        {
            incorrectAttempts++;
            StartCoroutine(IncorrectReaction());

            if (incorrectAttempts >= maxAttempts)
            {
                EndGame(false);
                return;
            }
        }
        else
        {
            OnCorrectLetterGuessed();
        }

        UpdateWordDisplay();
        CheckWinCondition();
    }

    void UpdateWordDisplay()
    {
        for (int i = 0; i < letterSlots.Length; i++)
        {
            letterSlots[i].text = displayedWord[i].ToString(); // Update individual TMP elements
        }

        Debug.Log("Updated Word Display: " + new string(displayedWord));
    }

    void CheckWinCondition()
    {
        if (new string(displayedWord) == fullSentence)
        {
            EndGame(true);
        }
    }

    public void OnCorrectLetterGuessed()
    {
        correctGuesses++;

        if (correctGuesses <= totalMissingLetters)
        {
            StartCoroutine(HandleMilkMeterProgression());
        }

        if (correctGuesses == totalMissingLetters)
        {
            EndGame(true);
        }
    }

    IEnumerator HandleMilkMeterProgression()
    {
        pourAnimator.SetTrigger("PourMilk");

        yield return new WaitForSeconds(1.5f);

        if (currentMilkState < milkMeters.Length)
        {
            foreach (GameObject milkMeter in milkMeters)
            {
                milkMeter.SetActive(false);
            }

            milkMeters[currentMilkState].SetActive(true);
            currentMilkState++;
        }
    }

    void EndGame(bool won)
    {
        if (won)
        {
            Debug.Log("You Win! Milk meter is full!");
            StartCoroutine(WinReactionSequence());
        }
        else
        {
            Debug.Log("Game Over! Retry?");
        }
    }

    IEnumerator WinReactionSequence()
    {
        correctAudio.Play();

        cowHappy.SetActive(true);
        cowNormal.SetActive(false);

        happyKids.SetActive(true);
        idleKids.SetActive(false);

        pourAnimator.SetTrigger("WinAnimation");

        yield return new WaitForSeconds(1.5f);

        cowHappy.SetActive(false);
        cowNormal.SetActive(true);
        happyKids.SetActive(false);
        idleKids.SetActive(true);
    }

    IEnumerator IncorrectReaction()
    {
        incorrectAudio.Play();

        sadKids.SetActive(true);
        cowAngry.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        sadKids.SetActive(false);
        cowAngry.SetActive(false);
    }
}
