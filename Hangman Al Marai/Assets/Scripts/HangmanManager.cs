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
    public GameObject WinPanelAR; 

    public VideoPlayer videoPlayerEN; 
    public GameObject videoScreenEN;
    public GameObject videoScreenAR; 
    public int hiddenLetterCount = 7; // Adjustable from Unity Inspector

    public bool isArabicMode = false , isEnglishMode = false; 

    // Words to guess
    private string fullSentence = "MilkEverydayIsTheSmartWay"; // Combined sentence (no spaces)
    private char[] displayedWord;
    private int incorrectAttempts = 0;

    public GameObject GameOverPanel, WinPanel; 

  
    private int currentMilkState = 0;
    private int correctGuesses = 0;
    private int totalMissingLetters = 7; // Number of required guesses

    // Arabic milk meter and pouring animation
    public GameObject[] arabicMilkMeters;  // Assign these in the Inspector
    public GameObject[] englishMilkMeters; // Assign these in the Inspector

    public Animator arabicPouringAnimation;

    // English milk meter and pouring animation
 
    public Animator englishPouringAnimation;


    public GameObject arabicCowHappy;
    public GameObject arabicCowAngry;
    public GameObject arabicCowNormal;
    public GameObject arabicHappyKids;
    public GameObject arabicSadKids;
    public GameObject arabicIdleKids;

    public GameObject englishCowHappy;
    public GameObject englishCowAngry;
    public GameObject englishCowNormal;
    public GameObject englishHappyKids;
    public GameObject englishSadKids;
    public GameObject englishIdleKids;


    public AudioSource correctAudio;
    public AudioSource incorrectAudio;

    public GameObject StartingScreen, HangmanEN, HangmanAR;
    public GameObject[] englishLetterTiles; // Visual GameObjects (assign in order)
    public char[] englishLetterValues;      // Match each tile's letter (assign in Inspector)

    private int remainingTiles;

    public GameObject pourObjectEN, pourObjectAR;

    public GameObject splashObjectEN, splashObjectAR;
    public Animator splashAnimatorEN, splashAnimatorAR;



    // Keyboard Input
    public GameObject[] keyboardButtons; // Array of keyboard buttons (A-Z)

    void Start()
    {
        AdjustMilkMeterCount(); // Ensure milk meters match hidden letters
        StartingScreen.SetActive(true);
        HangmanEN.SetActive(false);
        HangmanAR.SetActive(false);
    }

    public void InitializeGameVisual()
    {
        StartingScreen.SetActive(false);
        HangmanEN.SetActive(true);
        HangmanAR.SetActive(false);
        isEnglishMode = true;
        pourObjectEN.SetActive(false);
        AssignLetterValues();
        ValidateKeyboardInteraction();

        remainingTiles = 0;

        // Create shuffled index list
        List<int> indices = Enumerable.Range(0, englishLetterTiles.Length).ToList();
        System.Random rand = new System.Random();
        indices = indices.OrderBy(x => rand.Next()).ToList();

        for (int i = 0; i < englishLetterTiles.Length; i++)
        {
            bool shouldHide = i < hiddenLetterCount;

            if (shouldHide)
            {
                englishLetterTiles[indices[i]].SetActive(false);
                remainingTiles++;
            }
            else
            {
                englishLetterTiles[indices[i]].SetActive(true);
            }
        }

        Debug.Log($"🔠 Initialized {hiddenLetterCount} hidden tiles out of {englishLetterTiles.Length}.");
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
                btn.onClick.AddListener(() => OnEnglishLetterPressed(letter));
            }
        }

        Debug.Log("Keyboard setup complete.");
    }





    public void InitializeGame()
    {
      //  displayedWord = fullSentence.ToCharArray();
     //   HideRandomLetters(displayedWord, hiddenLetterCount); // Use adjustable value
     //   UpdateWordDisplay();
        AssignLetterValues();
        StartingScreen.SetActive(false);
        HangmanAR.SetActive(false);
        HangmanEN.SetActive(true);
        ValidateKeyboardInteraction();
        isEnglishMode = true;
        InitializeGameVisual(); 
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

    public void OnEnglishLetterPressed(string letter)
    {
        Debug.Log($"Pressed English letter: {letter}");

        if (!keyboardMap.TryGetValue(letter, out Button pressedButton) || pressedButton == null)
        {
            Debug.LogError($"❌ ERROR: Button for '{letter}' is missing or null!");
            return;
        }

        char guess = char.ToUpper(letter[0]);
        bool correctGuess = false;

        for (int i = 0; i < englishLetterValues.Length; i++)
        {
            if (!englishLetterTiles[i].activeSelf &&
                char.ToUpper(englishLetterValues[i]) == guess)
            {
                englishLetterTiles[i].SetActive(true);
                remainingTiles--;
                correctGuess = true;
                Debug.Log($"✅ Revealed letter '{guess}' at index {i}.");
            }
        }

        if (correctGuess)
        {
            pressedButton.GetComponent<Image>().color = Color.green;
            correctAudio.PlayOneShot(correctAudio.clip);

            StartCoroutine(WinReactionSequenceEnglish());

            englishCowHappy.SetActive(true);
            englishHappyKids.SetActive(true);
            englishCowAngry.SetActive(false);
            englishSadKids.SetActive(false);
            englishIdleKids.SetActive(false);

            englishPouringAnimation.SetBool("isPouring", true);
            englishPouringAnimation.Play("pour");
            Invoke("StopPouring", 1.5f);

            OnCorrectLetterGuessed();

            if (remainingTiles <= 0)
            {
                Debug.Log("🏆 All tiles revealed. Player wins!");
                HandleWin();
            }
        }
        else
        {
            pressedButton.GetComponent<Image>().color = Color.red;
            incorrectAudio.PlayOneShot(incorrectAudio.clip);
            incorrectAttempts++;

            StartCoroutine(IncorrectReactionEnglish());

            if (incorrectAttempts >= maxAttempts)
            {
                EndGame(false);
            }
        }
    }








    IEnumerator ResetArabicReaction()
    {
        yield return new WaitForSeconds(1.5f);

        arabicCowHappy.SetActive(false);
        arabicCowAngry.SetActive(false);
        arabicCowNormal.SetActive(true); // Ensure normal state resets

        arabicSadKids.SetActive(false);
        arabicHappyKids.SetActive(false);
        arabicIdleKids.SetActive(true); // Ensure idle kids activate

        Debug.Log("✅ Arabic reaction reset to idle.");
    }


    IEnumerator ResetEnglishReaction()
    {
        yield return new WaitForSeconds(1.5f);

        englishCowAngry.SetActive(false);
        englishCowHappy.SetActive(false);
        englishCowNormal.SetActive(true);

        englishSadKids.SetActive(false);
        englishHappyKids.SetActive(false);
        englishIdleKids.SetActive(true);

        Debug.Log("✅ English reaction reset to idle.");
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
        if (HangmanEN.activeSelf) // English mode with GameObject tiles
        {
            if (remainingTiles <= 0)
            {
                Debug.Log("✅ All tiles revealed! Player wins.");
                HandleWin();
            }
        }
        else if (HangmanAR.activeSelf) // Arabic mode
        {
            int remainingHiddenLetters = 0;

            foreach (char[] part in displayedWords)
            {
                if (part != null)
                {
                    remainingHiddenLetters += part.Count(c => c == ' ');
                }
            }

            Debug.Log($"🧐 Remaining Arabic hidden letters: {remainingHiddenLetters}");

            if (remainingHiddenLetters == 0)
            {
                Debug.Log("✅ Arabic mode: All letters revealed! Player wins.");
                HandleWin();
            }
        }
    }







    public void OnCorrectLetterGuessed()
    {
        correctGuesses++;
        Debug.Log($"Correct letter guessed! Current: {correctGuesses} / Total Hidden: {hiddenLetterCount}");

        bool isArabic = HangmanAR.activeSelf;

        // 🎬 Pouring setup
        GameObject pourObj = isArabic ? pourObjectAR : pourObjectEN;
        Animator pourAnim = isArabic ? arabicPouringAnimation : englishPouringAnimation;

        // 💦 Splash setup
        GameObject splashObj = isArabic ? splashObjectAR : splashObjectEN;
        Animator splashAnim = isArabic ? splashAnimatorAR : splashAnimatorEN;

        // ▶️ Activate pour
        if (pourObj != null && pourAnim != null)
        {
            pourObj.SetActive(true);
            pourAnim.SetBool("isPouring", true);
            pourAnim.Play("pour", 0, 0f);

            // Realtime adjusted duration
            AnimationClip pourClip = pourAnim.runtimeAnimatorController.animationClips
                .FirstOrDefault(clip => clip.name == "pour");

            float playbackSpeed = pourAnim.GetCurrentAnimatorStateInfo(0).speed;
            float clipLength = pourClip != null ? pourClip.length : 1.5f;

            float realDuration = clipLength / playbackSpeed;

            Debug.Log($"⏱ Pouring animation clip is '{clipLength}'s, speed = {playbackSpeed}, so total = {realDuration}s");

            StartCoroutine(HandleMilkMeterProgression(realDuration));
        }





        float animLength = pourAnim.GetCurrentAnimatorStateInfo(0).length;
        StartCoroutine(HandleMilkMeterProgression(animLength));

        // 🏆 Check win condition using updated visual logic
        if (HangmanEN.activeSelf)
        {
            if (remainingTiles == 0) CheckWinCondition();
        }
        else
        {
            CheckWinCondition(); // For Arabic sentence logic
        }
    }






    // Stop pouring after animation duration
    void StopPouring()
    { 
        if (englishPouringAnimation != null)
        {
            Debug.Log("Setting isPouring = false");
            englishPouringAnimation.SetBool("isPouring", false);
        }
    }

    void StopArabicPouring()
    {
        if (arabicPouringAnimation != null)
        {
            Debug.Log("🚫 Stopping Arabic pouring animation.");
            arabicPouringAnimation.SetBool("isPouring", false);
        }
    }





    IEnumerator HandleMilkMeterProgression(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Audio
        milkFillAudio?.Play();

        // Get proper milk meter array
        bool isArabic = HangmanAR.activeSelf;
        GameObject[] activeMeter = isArabic ? arabicMilkMeters : englishMilkMeters;

        // Disable pour & splash objects
        (isArabic ? pourObjectAR : pourObjectEN)?.SetActive(false);
        (isArabic ? splashObjectAR : splashObjectEN)?.SetActive(false);

        if (currentMilkState >= activeMeter.Length - 1)
        {
            Debug.Log("🚀 Milk meter is fully filled!");
            yield break;
        }

        for (int i = 0; i < activeMeter.Length; i++)
        {
            activeMeter[i].SetActive(i == currentMilkState + 1);
        }

        currentMilkState++;
        Debug.Log($"✅ Milk meter updated to level: {currentMilkState}");
    }








    void EndGame(bool won)
    {
        if (won)
        {
            Debug.Log("✅ You Win! Milk meter is full!");

            if (HangmanAR.activeSelf) // Arabic mode
            {
                StartCoroutine(WinReactionSequenceArabic()); // Arabic reactions
            }
            else // English mode
            {
                StartCoroutine(WinReactionSequenceEnglish()); // English reactions
            }
        }
        else
        {
            Debug.Log("❌ Game Over! Retry?");
            GameOverPanel.SetActive(true);
        }
    }




    IEnumerator WinReactionSequenceArabic()
    {
        correctAudio.Play();

        arabicCowHappy.SetActive(true);
        arabicCowNormal.SetActive(false);

        arabicHappyKids.SetActive(true);
        arabicIdleKids.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        arabicCowHappy.SetActive(false);
        arabicCowNormal.SetActive(true);
        arabicHappyKids.SetActive(false);
        arabicIdleKids.SetActive(true);

        Debug.Log("✅ Arabic win reaction sequence completed.");
    }

    IEnumerator WinReactionSequenceEnglish()
    {
        correctAudio.Play();

        englishCowHappy.SetActive(true);
        englishCowNormal.SetActive(false);

        englishHappyKids.SetActive(true);
        englishIdleKids.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        englishCowHappy.SetActive(false);
        englishCowNormal.SetActive(true);
        englishHappyKids.SetActive(false);
        englishIdleKids.SetActive(true);

        Debug.Log("✅ English win reaction sequence completed.");
    }

    IEnumerator IncorrectReactionArabic()
    {
        incorrectAudio.Play();

        // 🔹 Hide idle state immediately
        arabicIdleKids.SetActive(false);
        arabicCowNormal.SetActive(false);

        arabicSadKids.SetActive(true);
        arabicCowAngry.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        arabicSadKids.SetActive(false);
        arabicCowAngry.SetActive(false);

        arabicIdleKids.SetActive(true);
        arabicCowNormal.SetActive(true);

        // Optional: Don't immediately show idle—let next input control it
        Debug.Log("❌ Arabic incorrect reaction sequence completed.");
    }


    IEnumerator IncorrectReactionEnglish()
    {
        incorrectAudio.Play();

        // 🔹 Hide idle state immediately
        englishIdleKids.SetActive(false);
        englishCowNormal.SetActive(false);

        englishSadKids.SetActive(true);
        englishCowAngry.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        englishSadKids.SetActive(false);
        englishCowAngry.SetActive(false);
        englishIdleKids.SetActive(true);
        englishCowNormal.SetActive(true);

        // Optional: skip calling ResetEnglishReaction() here
        Debug.Log("❌ English incorrect reaction sequence completed.");
    }



    public void Retry()
    {
        SceneManager.LoadSceneAsync(SceneName);
        Debug.Log("loading Scene " + SceneName);
        StartingScreen.SetActive(true);
        HangmanEN.SetActive(false);

    }


    public VideoPlayer videoPlayerAR; 

    public void HandleWin()
    {
        Debug.Log("Player won! Activating video...");

        // 🔇 Stop any currently playing audio
        if (correctAudio.isPlaying) correctAudio.Stop();
        if (incorrectAudio.isPlaying) incorrectAudio.Stop();
        if (milkFillAudio != null && milkFillAudio.isPlaying) milkFillAudio.Stop();

        // 🔹 Activate appropriate video and screen based on mode
        if (HangmanAR.activeSelf) // Arabic mode
        {
            videoScreenAR.SetActive(true);
            videoPlayerAR.gameObject.SetActive(true);
            videoPlayerAR.Play();
            Debug.Log("🎥 Playing Arabic win video");
        }
        else // English mode
        {
            videoScreenEN.SetActive(true);
            videoPlayerEN.gameObject.SetActive(true);
            videoPlayerEN.Play();
            Debug.Log("🎥 Playing English win video");
        }

        StartCoroutine(ActivateWinPanelAfterDelay());
    }


    IEnumerator ActivateWinPanelAfterDelay()
    {
        yield return new WaitForSeconds(18f); // Adjust this delay as needed

        Debug.Log("🚀 Activating win panel...");

        if (HangmanAR.activeSelf) // Arabic mode
        {
            WinPanelAR.SetActive(true);
            Debug.Log("✅ Arabic win panel activated!");
        }
        else // English mode
        {
            WinPanel.SetActive(true);
            Debug.Log("✅ English win panel activated!");
        }

        // 🔹 Hide video elements after win panel appears (Handles both modes)
        videoPlayerEN.gameObject.SetActive(false);
        videoScreenEN.SetActive(false);
        videoPlayerAR.gameObject.SetActive(false);
        videoScreenAR.SetActive(false);
    }



    void AdjustMilkMeterCount()
    {
        // 🔹 Select the correct milk meter parent based on the active mode
        GameObject[] activeMilkMeters = HangmanAR.activeSelf ? arabicMilkMeters : englishMilkMeters;

        int currentCount = activeMilkMeters.Length;

        if (currentCount < hiddenLetterCount)
        {
            Debug.Log($"Milk meter count ({currentCount}) is lower than hidden letters ({hiddenLetterCount}). Adjusting...");

            List<GameObject> adjustedMilkMeters = new List<GameObject>(activeMilkMeters);

            GameObject lastMilkMeter = activeMilkMeters[currentCount - 1]; // Last assigned milk object

            while (adjustedMilkMeters.Count < hiddenLetterCount)
            {
                GameObject duplicate = Instantiate(lastMilkMeter, lastMilkMeter.transform.parent);
                adjustedMilkMeters.Add(duplicate);
            }

            // 🔹 Update the correct array based on the mode
            if (HangmanAR.activeSelf)
            {
                arabicMilkMeters = adjustedMilkMeters.ToArray();
                Debug.Log($"✅ Arabic milk meters successfully adjusted to {arabicMilkMeters.Length}.");
            }
            else
            {
                englishMilkMeters = adjustedMilkMeters.ToArray();
                Debug.Log($"✅ English milk meters successfully adjusted to {englishMilkMeters.Length}.");
            }
        }
    }





    // Arabic Section

    public string arabicSentence = "??? ?????? ?? ??????"; // Arabic sentence
   
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
        pourObjectAR.SetActive(false);
        StartingScreen.SetActive(false);
        HangmanAR.SetActive(true);
        HangmanEN.SetActive(false);
        isArabicMode = true; 
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

    // Arabic Alphabet Mapping (Ensure Correct Order)
    string[] arabicAlphabet = { "ا", "ب", "ت", "ث", "ج", "ح", "خ", "د", "ذ", "ر", "ز", "س", "ش", "ص", "ض",
                            "ط", "ظ", "ع", "غ", "ف", "ق", "ك", "ل", "م", "ن", "هـ", "و", "ي" };

    void AssignArabicLetterValues()
    {
        string[] arabicAlphabet = { "ا", "ب", "ت", "ث", "ج", "ح", "خ", "د", "ذ", "ر", "ز", "س", "ش", "ص", "ض",
                                "ط", "ظ", "ع", "غ", "ف", "ق", "ك", "ل", "م", "ن", "هـ", "و", "ي" };

        if (arabicKeyboardButtons.Length != arabicAlphabet.Length)
        {
            Debug.LogError("ERROR: Arabic keyboard buttons count doesn't match alphabet count!");
            return;
        }

        arabicKeyboardMap.Clear();

        for (int i = 0; i < arabicKeyboardButtons.Length; i++)
        {
            Button btn = arabicKeyboardButtons[i].GetComponent<Button>();
            string letter = arabicAlphabet[i];

            if (btn == null)
            {
                Debug.LogError($"ERROR: Arabic button '{letter}' is missing a Button component!");
                continue;
            }

            if (!arabicKeyboardMap.ContainsKey(letter))
            {
                arabicKeyboardMap.Add(letter, btn);
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnArabicLetterPressed(letter));

                Debug.Log($"✅ Arabic key mapped: {letter} → {btn.name}");
            }
        }

        Debug.Log("✅ Arabic keyboard mapping complete.");
    }




    public void OnArabicLetterPressed(string letter)
    {
        Debug.Log($"Pressed Arabic letter: {letter}");

        if (!arabicKeyboardMap.ContainsKey(letter))
        {
            Debug.LogError($"ERROR: Arabic letter '{letter}' not found in dictionary!");
            return;
        }

        Button pressedButton = arabicKeyboardMap[letter];

        if (pressedButton == null)
        {
            Debug.LogError($"ERROR: Arabic button '{letter}' is NULL!");
            return;
        }

        char guess = letter[0];
        bool correctGuess = false;
        int remainingHiddenLetters = 0;  // Track unrevealed letters

        for (int i = 0; i < arabicSentenceParts.Length; i++)
        {
            for (int j = 0; j < displayedWords[i].Length; j++)
            {
                if (displayedWords[i][j] == ' ' && arabicSentenceParts[i][j] == guess) // Correct replacement
                {
                    displayedWords[i][j] = guess;
                    correctGuess = true;
                    Debug.Log($"✅ Correct Arabic guess! '{letter}' revealed at [{i}, {j}].");
                }

                // Count hidden letters AFTER replacement
                if (displayedWords[i][j] == ' ')
                {
                    remainingHiddenLetters++;
                }
            }
        }

        UpdateArabicDisplay();

        if (correctGuess)
        {
            pressedButton.GetComponent<Image>().color = Color.green;
            correctAudio.PlayOneShot(correctAudio.clip);

            arabicCowHappy.SetActive(true);
            arabicHappyKids.SetActive(true);
            arabicCowAngry.SetActive(false);
            arabicSadKids.SetActive(false);
            arabicIdleKids.SetActive(false);

            arabicPouringAnimation.SetTrigger("pour");

            StartCoroutine(ResetArabicReaction());
            OnCorrectLetterGuessed();
        }
        else
        {
            incorrectAttempts++;
            Debug.Log($"❌ Incorrect Arabic guess: '{letter}' is not in the sentence.");
            pressedButton.GetComponent<Image>().color = Color.red;
            incorrectAudio.PlayOneShot(incorrectAudio.clip);

            StartCoroutine(IncorrectReactionArabic());

            if (incorrectAttempts >= maxAttempts)
            {
                EndGame(false);
            }
        }

        // 🔹 Fix: Ensure win condition ONLY happens when all hidden letters are revealed
        if (remainingHiddenLetters == 0)
        {
            Debug.Log("✅ Arabic mode: All hidden letters revealed! Player wins.");
            HandleWin();
        }
    }








    //Testing 

    void ValidateKeyboardInteraction()
    {
        foreach (GameObject buttonObject in keyboardButtons.Concat(arabicKeyboardButtons))
        {
            Button btn = buttonObject.GetComponent<Button>();

            if (btn == null)
            {
                Debug.LogError($"ERROR: {buttonObject.name} is missing a Button component!");
                continue;
            }

            if (!btn.interactable)
            {
                Debug.LogWarning($"⚠ {buttonObject.name} is not interactable! Fixing...");
                btn.interactable = true;
            }

            Image img = buttonObject.GetComponent<Image>();
            if (img != null && !img.raycastTarget)
            {
                Debug.LogWarning($"⚠ {buttonObject.name} Raycast Target is disabled! Fixing...");
                img.raycastTarget = true;
            }
        }

        Debug.Log("✅ All keyboard buttons are now interactable.");
    }


}
