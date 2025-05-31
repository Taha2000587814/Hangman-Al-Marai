using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video; 

public class HangmanManager : MonoBehaviour
{
    // UI Elements
    public TMP_Text[] letterSlots; // Array to hold TMP objects for each letter
    public int maxAttempts = 6; // Max incorrect guesses

    public string SceneName;

    public AudioSource milkFillAudio; 

    public VideoPlayer videoPlayerEN; 
    public GameObject videoScreenEN;



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
        StartingScreen.SetActive(true);
        HangmanEN.SetActive(false); 

        if (videoPlayerEN != null)
        {
            videoPlayerEN.loopPointReached += OnVideoEnd; // Subscribe to event when video ends
        }
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
        pourAnimator.SetTrigger("pour"); // Start milk pouring animation

        if (milkFillAudio != null) // Check if audio source is assigned
        {
            milkFillAudio.Play(); // Play pouring sound
            Debug.Log("Milk pouring sound played!");
        }

        yield return new WaitForSeconds(1.5f); // Wait for animation to complete

        if (currentMilkState < milkMeters.Length)
        {
            foreach (GameObject milkMeter in milkMeters)
            {
                milkMeter.SetActive(false);
            }

            milkMeters[currentMilkState].SetActive(true); // Update milk level
            currentMilkState++;
        }
    }


    void EndGame(bool won)
    {
        if (won)
        {
            Debug.Log("You Win! Milk meter is full!");
            StartCoroutine(WinReactionSequence()); // Show reaction first
            StartCoroutine(EnableWinPanelAfterDelay()); // Delay WinPanel activation
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

        pourAnimator.SetTrigger("WinAnimation");

        yield return new WaitForSeconds(1.5f);

        cowHappy.SetActive(false);
        cowNormal.SetActive(true);
        happyKids.SetActive(false);
        idleKids.SetActive(true);
        WinPanel.SetActive(true);
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



    public void PlayVideo()
    {
        if (videoPlayerEN != null && videoScreenEN != null)
        {
            videoScreenEN.SetActive(true); // Enable video screen
            WinPanel.SetActive(false);//hde winner panel
            videoPlayerEN.Play(); // Start playing video
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video finished, enabling WinPanel...");
        videoScreenEN.SetActive(false);
        WinPanel.SetActive(true);
    }

    IEnumerator EnableWinPanelAfterDelay()
    {
        yield return new WaitForSeconds(2);
        WinPanel.SetActive(true);
    }

}
