using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Linq; 

public class HangmanManager : MonoBehaviour
{
    // UI Elements
    public TMP_Text[] letterSlots; // Array to hold TMP objects for each letter
    public int maxAttempts = 6; // Max incorrect guesses

    public string SceneName;

    public AudioSource milkFillAudio; 

    public VideoPlayer videoPlayerEN; 
    public GameObject videoScreenEN;
    public int hiddenLetterCount = 7; // Adjustable from Unity Inspector



    // Words to guess
    private string fullSentence = "milkeverydayisthesmartway"; // Combined sentence (no spaces)
    private char[] displayedWord;
    private int incorrectAttempts = 0;

    public GameObject GameOverPanel, WinPanel; 

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

    public GameObject StartingScreen, HangmanEN, HangmanAR; 

    // Keyboard Input
    public GameObject[] keyboardButtons; // Array of keyboard buttons (A-Z)

    void Start()
    {
        AssignLetterValues();
        InitializeGame();
        AdjustMilkMeterCount(); // Ensure milk meters match hidden letters
        StartingScreen.SetActive(true);
        HangmanEN.SetActive(false);
    }


    Dictionary<string, Button> keyboardMap = new Dictionary<string, Button>();

    void AssignLetterValues()
    {
        foreach (GameObject buttonObject in keyboardButtons)
        {
            Button btn = buttonObject.GetComponent<Button>();
            string letter = buttonObject.name.ToLower(); // Ensure letter matches keyboard input

            if (!keyboardMap.ContainsKey(letter))
            {
                keyboardMap.Add(letter, btn);
                btn.onClick.AddListener(() => OnLetterPressed(letter));
            }
        }

        Debug.Log("Keyboard setup complete with Dictionary mapping.");
    }




    void InitializeGame()
    {
        displayedWord = fullSentence.ToCharArray();
        HideRandomLetters(displayedWord, hiddenLetterCount); // Use adjustable value
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
        string lowercaseLetter = letter.ToLower();
        Debug.Log($"Pressed letter: {lowercaseLetter}");

        char guess = lowercaseLetter[0];
        bool correctGuess = false;

        Button pressedButton = keyboardMap.ContainsKey(lowercaseLetter) ? keyboardMap[lowercaseLetter] : null;

        if (pressedButton == null)
        {
            Debug.LogError($"ERROR: No button found for '{lowercaseLetter}'!");
            return;
        }

        for (int i = 0; i < fullSentence.Length; i++)
        {
            if (fullSentence[i] == guess && displayedWord[i] == ' ')
            {
                displayedWord[i] = guess;
                correctGuess = true;
                Debug.Log($"? Correct guess! '{lowercaseLetter}' revealed at index {i}.");
            }
        }

        UpdateWordDisplay();

        if (!correctGuess)
        {
            incorrectAttempts++;
            Debug.Log($"? Incorrect guess: '{lowercaseLetter}' is not in the sentence.");

            cowAngry.SetActive(true);
            sadKids.SetActive(true);
            cowHappy.SetActive(false);
            happyKids.SetActive(false);
            idleKids.SetActive(false);

            pressedButton.GetComponent<Image>().color = Color.red; // ?? Button turns red
            incorrectAudio.PlayOneShot(incorrectAudio.clip); // ? Allows overlapping sounds

            StartCoroutine(ResetReaction());

            if (incorrectAttempts >= maxAttempts)
            {
                EndGame(false);
            }
        }
        else
        {
            cowHappy.SetActive(true);
            happyKids.SetActive(true);
            cowAngry.SetActive(false);
            sadKids.SetActive(false);
            idleKids.SetActive(false);

            pressedButton.GetComponent<Image>().color = Color.green; // ?? Button turns green
            correctAudio.PlayOneShot(correctAudio.clip); // ? Allows overlapping sounds

            StartCoroutine(ResetReaction());
            OnCorrectLetterGuessed();
        }

        CheckWinCondition();
    }



    IEnumerator ResetReaction()
    {
        yield return new WaitForSeconds(1);

        cowAngry.SetActive(false);
        cowHappy.SetActive(false);
        cowNormal.SetActive(true);

        sadKids.SetActive(false);
        happyKids.SetActive(false);
        idleKids.SetActive(true);

        Debug.Log("?? Reaction reset: Back to idle state.");
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
        int remainingHiddenLetters = new string(displayedWord).Count(c => c == ' ');

        Debug.Log($"Remaining hidden letters: {remainingHiddenLetters}");

        // Only trigger win condition when ALL hidden letters are revealed
        if (remainingHiddenLetters == 0)
        {
            Debug.Log("All hidden letters revealed! Player wins.");
            HandleWin();
        }
    }



    public void OnCorrectLetterGuessed()
    {
        correctGuesses++;

        Debug.Log($"Correct letter guessed! Current: {correctGuesses} / Total Hidden: {hiddenLetterCount}");

        // Ensure pouring animation starts using isPouring
        if (pourAnimator != null)
        {
            Debug.Log("Setting isPouring = true");
            pourAnimator.SetBool("isPouring", true);
            Invoke("StopPouring", 1.5f); // Stop animation after 1.5 seconds
        }
        else
        {
            Debug.LogError("ERROR: pourAnimator is NULL!");
        }

        int progressionStep = Mathf.Max(1, hiddenLetterCount / milkMeters.Length);

        if (correctGuesses % progressionStep == 0 && currentMilkState < milkMeters.Length)
        {
            StartCoroutine(HandleMilkMeterProgression());
        }

        if (!new string(displayedWord).Contains(" "))
        {
            Debug.Log("All hidden letters revealed! Player wins.");
            HandleWin();
        }
    }

    // Stop pouring after animation duration
    void StopPouring()
    {
        if (pourAnimator != null)
        {
            Debug.Log("Setting isPouring = false");
            pourAnimator.SetBool("isPouring", false);
        }
    }



    IEnumerator HandleMilkMeterProgression()
    {
        if (milkFillAudio != null)
        {
            milkFillAudio.Play();
            Debug.Log("Milk pouring sound played!");
        }

        yield return new WaitForSeconds(1.5f);

        if (currentMilkState < milkMeters.Length - 1)
        {
            Debug.Log($"Milk state progressing: {currentMilkState} ? {currentMilkState + 1}");

            foreach (GameObject milkMeter in milkMeters)
            {
                milkMeter.SetActive(false);
            }

            currentMilkState++; // Increment milk state
            milkMeters[currentMilkState].SetActive(true);
        }
    }




    void EndGame(bool won)
    {
        if (won)
        {
            Debug.Log("You Win! Milk meter is full!");
            StartCoroutine(WinReactionSequence()); // Show reaction first
            
        }
        else
        {
            Debug.Log("Game Over! Retry?");
            GameOverPanel.SetActive(true);
        }
    }



    IEnumerator WinReactionSequence()
    {
        correctAudio.Play();

        cowHappy.SetActive(true);
        cowNormal.SetActive(false);

        happyKids.SetActive(true);
        idleKids.SetActive(false);

    

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

    public void Retry()
    {
        SceneManager.LoadSceneAsync(SceneName);
        Debug.Log("loading Scene " + SceneName);
        StartingScreen.SetActive(true);
        HangmanEN.SetActive(false);

    }


    public void HandleWin()
    {
        Debug.Log("Player won! Activating video...");

        videoScreenEN.SetActive(true); // Show video screen
        videoPlayerEN.gameObject.SetActive(true); // Enable VideoPlayer
        videoPlayerEN.Play(); // Start playback

        StartCoroutine(ActivateWinPanelAfterDelay());
    }

    IEnumerator ActivateWinPanelAfterDelay()
    {
        yield return new WaitForSeconds(18f); // Adjust this delay as needed

        Debug.Log("Activating win panel...");
        WinPanel.SetActive(true); // Show win panel

        // Hide video elements after win panel appears
        videoPlayerEN.gameObject.SetActive(false);
        videoScreenEN.SetActive(false);
    }


    void AdjustMilkMeterCount()
    {
        int currentCount = milkMeters.Length;

        if (currentCount < hiddenLetterCount)
        {
            Debug.Log($"Milk meter count ({currentCount}) is lower than hidden letters ({hiddenLetterCount}). Adjusting...");

            List<GameObject> adjustedMilkMeters = new List<GameObject>(milkMeters);

            GameObject lastMilkMeter = milkMeters[currentCount - 1]; // Last assigned milk object

            while (adjustedMilkMeters.Count < hiddenLetterCount)
            {
                GameObject duplicate = Instantiate(lastMilkMeter, lastMilkMeter.transform.parent);
                adjustedMilkMeters.Add(duplicate);
            }

            milkMeters = adjustedMilkMeters.ToArray(); // Update array with new elements
            Debug.Log($"Milk meters successfully adjusted to {milkMeters.Length}.");
        }
    }

}
