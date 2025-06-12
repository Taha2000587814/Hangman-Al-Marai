using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Linq;
using ArabicSupport;

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
        AdjustMilkMeterCount(); // Ensure milk meters match hidden letters
        StartingScreen.SetActive(true);
        HangmanEN.SetActive(false);
        HangmanAR.SetActive(false);
    }


    Dictionary<string, Button> keyboardMap = new Dictionary<string, Button>();

    void AssignLetterValues()
    {
        foreach (GameObject buttonObject in keyboardButtons)
        {
            Button btn = buttonObject.GetComponent<Button>();

            if (btn == null)
            {
                Debug.LogError($"Button missing on {buttonObject.name}");
                continue; // Skip this button
            }

            string letter = buttonObject.name.ToLower(); // Ensure name matches correctly

            if (!keyboardMap.ContainsKey(letter))
            {
                keyboardMap.Add(letter, btn);
                btn.onClick.RemoveAllListeners(); // Avoid duplicate listeners
                btn.onClick.AddListener(() => OnLetterPressed(letter));
            }
        }

        Debug.Log("Keyboard setup complete.");
    }





    public void InitializeGame()
    {
        displayedWord = fullSentence.ToCharArray();
        HideRandomLetters(displayedWord, hiddenLetterCount); // Use adjustable value
        UpdateWordDisplay();
        AssignLetterValues();
        StartingScreen.SetActive(false);
        HangmanAR.SetActive(false);
        HangmanEN.SetActive(true);
        ValidateKeyboardInteraction(); 
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




    // Arabic Section

   public string arabicSentence = "??? ?????? ?? ??????"; // Arabic sentence
    public bool isArabicMode = false; // Tracks the current language mode
    public GameObject[] arabicKeyboardButtons; // Arabic keyboard buttons array

    public TMP_Text[] arabicSentenceHolders; // Array for sentence segments
    public string[] arabicSentenceParts = { "???", "??????", "??", "??????" }; // Four sentence parts
    private char[][] displayedWords; // Multi-array for hiding letters


   
    public TMP_Text[] arabicLetterSlots; // Arabic text slots



    public void InitializeArabicGame()
    {
        displayedWords = new char[arabicSentenceParts.Length][];

        int lettersToHidePerPart = hiddenLetterCount / arabicSentenceParts.Length; // Ensure consistency

        for (int i = 0; i < arabicSentenceParts.Length; i++)
        {
            displayedWords[i] = arabicSentenceParts[i].ToCharArray();
            HideRandomLetters(displayedWords[i], lettersToHidePerPart); // Use the same logic as English mode
        }

        UpdateArabicDisplay();
        AssignArabicLetterValues();
        ValidateKeyboardInteraction(); // Ensure all Arabic keyboard buttons are interactable

        StartingScreen.SetActive(false);
        HangmanAR.SetActive(true);
        HangmanEN.SetActive(false);
    }



    void UpdateArabicDisplay()
    {
        for (int i = 0; i < arabicSentenceHolders.Length; i++)
        {
            if (i < displayedWords.Length)
            {
                arabicSentenceHolders[i].text = ArabicFixer.Fix(new string(displayedWords[i]), true, true); // Fix RTL & spacing
            }
        }

        Debug.Log($"? Arabic Sentence Display (RTL Fixed): {string.Join(" ", arabicSentenceParts)}");
    }


    Dictionary<string, Button> arabicKeyboardMap = new Dictionary<string, Button>();

    // Arabic Alphabet Mapping (Ensure Order)
    string[] arabicAlphabet = { "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?",
                            "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "??", "?", "?" };

    void AssignArabicLetterValues()
    {
        if (arabicKeyboardButtons.Length != arabicAlphabet.Length)
        {
            Debug.LogError("ERROR: Arabic keyboard buttons count doesn't match alphabet count!");
            return;
        }

        for (int i = 0; i < arabicKeyboardButtons.Length; i++)
        {
            Button btn = arabicKeyboardButtons[i].GetComponent<Button>();
            string letter = arabicAlphabet[i]; // Assign letters in correct order

            if (!arabicKeyboardMap.ContainsKey(letter))
            {
                arabicKeyboardMap.Add(letter, btn);
                btn.onClick.RemoveAllListeners(); // Avoid duplicate listeners
                btn.onClick.AddListener(() => OnArabicLetterPressed(letter));
            }
        }

        Debug.Log("? Arabic keyboard mapping complete with correct alphabetical order.");
    }



    public void OnArabicLetterPressed(string letter)
    {
        Debug.Log($"Pressed Arabic letter: {letter}");

        char guess = letter[0];
        bool correctGuess = false;

        Button pressedButton = arabicKeyboardMap.ContainsKey(letter) ? arabicKeyboardMap[letter] : null;

        if (pressedButton == null)
        {
            Debug.LogError($"ERROR: No button found for '{letter}'!");
            return;
        }

        for (int i = 0; i < arabicSentence.Length; i++)
        {
            if (arabicSentence[i] == guess && displayedWord[i] == ' ')
            {
                displayedWord[i] = guess;
                correctGuess = true;
                Debug.Log($"? Correct Arabic guess! '{letter}' revealed at index {i}.");
            }
        }

        UpdateArabicDisplay();

        if (!correctGuess)
        {
            incorrectAttempts++;
            Debug.Log($"? Incorrect Arabic guess: '{letter}' is not in the sentence.");

            // Match English incorrect effects
            pressedButton.GetComponent<Image>().color = Color.red;
            incorrectAudio.PlayOneShot(incorrectAudio.clip);

            cowAngry.SetActive(true);
            sadKids.SetActive(true);
            cowHappy.SetActive(false);
            happyKids.SetActive(false);
            idleKids.SetActive(false);

            StartCoroutine(ResetReaction());

            if (incorrectAttempts >= maxAttempts)
            {
                EndGame(false);
            }
        }
        else
        {
            // Match English correct effects
            pressedButton.GetComponent<Image>().color = Color.green;
            correctAudio.PlayOneShot(correctAudio.clip);

            cowHappy.SetActive(true);
            happyKids.SetActive(true);
            cowAngry.SetActive(false);
            sadKids.SetActive(false);
            idleKids.SetActive(false);

            StartCoroutine(ResetReaction());
            OnCorrectLetterGuessed();
            CheckWinCondition();
        }

        
    }



    //Testing 

    void ValidateKeyboardInteraction()
    {
        foreach (GameObject buttonObject in arabicKeyboardButtons)
        {
            Button btn = buttonObject.GetComponent<Button>();

            if (btn == null)
            {
                Debug.LogError($"ERROR: Button missing on {buttonObject.name}");
                continue;
            }

            if (!btn.interactable)
            {
                Debug.LogError($"WARNING: {buttonObject.name} is not interactable!");
                btn.interactable = true; // Force enable interaction
            }

            Image img = buttonObject.GetComponent<Image>();
            if (img != null && !img.raycastTarget)
            {
                Debug.LogError($"WARNING: {buttonObject.name} Raycast Target is disabled!");
                img.raycastTarget = true; // Force enable raycast
            }
        }
    }


}
