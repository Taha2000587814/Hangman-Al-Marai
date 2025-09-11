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



    // ─── New Fields at Top ───────────────────────────────────────────────────────
    [Header("Audio Settings")]
    public AudioSource themeAudio;    // Assign your background/theme music here
    public AudioSource winAudio;      // Assign a one-shot win jingle here
    public AudioSource SongAudio;

    // Letters to hide (fixed, non-duplicate)
    private static readonly HashSet<char> hideLettersEN =
        new HashSet<char> { 'l', 'k', 't', 'h', 'm', 'v', 'r', 'y', 'w', 's' };
    private static readonly HashSet<char> hideLettersAR =
        new HashSet<char> { 'ا', 'ش', 'س', 'ل', 'ح', 'ي', 'أ' };


    public List<int> fixedHiddenIndices = new List<int> { 1, 3, 5, 7, 9, 11, 13 }; // Example pattern


    private Vector3 originalPourENPosition;
    private Vector3 originalPourARPosition;

    public Vector3 offScreenPosition = new Vector3(9999, 9999, 0);

   // public Image winFadeOverlay;           // Assign a full-screen UI Image (black or white)
   // public float fadeDuration = 2f;        // Control speed of fade-in

    private bool isFadingIn = false;
    private float fadeTimer = 0f;

    public AudioSource milkFillAudio;
    public GameObject WinPanelAR;

    public VideoPlayer videoPlayerEN;
    public GameObject videoScreenEN;
    public GameObject videoScreenAR;
    public int hiddenLetterCount = 7; // Adjustable from Unity Inspector

    [Header("Visual Settings")]
    public Color unhiddenKeyColor = Color.gray;

    public bool isArabicMode = false, isEnglishMode = false;

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

    private bool isWinVideoPrepared = false;


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
        originalPourENPosition = pourObjectEN.transform.position;
        originalPourARPosition = pourObjectAR.transform.position;

    }

    // ─── Updated English Initialization ────────────────────────────────────────
    public void InitializeGameVisual()
    {
        StartingScreen.SetActive(false);
        HangmanEN.SetActive(true);
        HangmanAR.SetActive(false);
        isEnglishMode = true;
        if (themeAudio != null) themeAudio.UnPause();
        if (SongAudio != null) SongAudio.Stop();
        // Reset pour visuals
        pourObjectEN.SetActive(false);
        pourObjectEN.transform.position = originalPourENPosition;

        AssignLetterValues();
        ValidateKeyboardInteraction();

        remainingTiles = 0;
        HashSet<char> visibleChars = new HashSet<char>();

        for (int i = 0; i < englishLetterTiles.Length; i++)
        {
            char letter = char.ToLower(englishLetterValues[i]);
            bool shouldHide = hideLettersEN.Contains(letter);

            // Show/hide tile
            englishLetterTiles[i].SetActive(!shouldHide);

            if (shouldHide)
            {
                remainingTiles++;
            }
            else
            {
                visibleChars.Add(letter);
            }
        }

        // Grey out & disable keys for visible letters
        foreach (var kv in keyboardMap)
        {
            char keyChar = kv.Key[0];
            if (visibleChars.Contains(keyChar))
            {
                var btn = kv.Value;
                btn.GetComponent<Image>().color = unhiddenKeyColor;
                btn.interactable = false;
            }
        }

        Debug.Log($"🔠 English hidden tiles: {remainingTiles} / {englishLetterTiles.Length}");
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
        AudioListener.volume = 1f;
        AssignLetterValues();
        StartingScreen.SetActive(false);
        HangmanAR.SetActive(false);
        HangmanEN.SetActive(true);
        ValidateKeyboardInteraction();
        isEnglishMode = true;
        InitializeGameVisual();

        if (themeAudio != null) themeAudio.UnPause();
        if (SongAudio != null) SongAudio.Stop();


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

        // 🔒 Disable interaction immediately
        pressedButton.interactable = false;

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

            //englishCowHappy.SetActive(true);
            //englishHappyKids.SetActive(true);
            //englishCowAngry.SetActive(false);
            //englishSadKids.SetActive(false);
            //englishIdleKids.SetActive(false);

            englishPouringAnimation.SetBool("isPouring", true);
            englishPouringAnimation.Play("pour");
            Invoke("StopPouring", 1.5f);

            // ✅ Win check now centralized
            OnCorrectLetterGuessed();
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

    private bool hasStartedWinSequence = false;

    void CheckWinCondition()
    {
        if (hasStartedWinSequence) return; // 🛑 Prevent duplicate win trigger

        if (HangmanEN.activeSelf)
        {
            if (remainingTiles <= 0)
            {
                Debug.Log("✅ All tiles revealed! Player wins.");
                hasStartedWinSequence = true;
                HandleWin();
                pourObjectEN.SetActive(false);
            }
        }
        else if (HangmanAR.activeSelf)
        {
            Debug.Log($"🧐 Remaining Arabic hidden tiles: {remainingArabicTiles}");

            if (remainingArabicTiles <= 0)
            {
                Debug.Log("🏆 All Arabic tiles revealed! Player wins.");
                hasStartedWinSequence = true;
                HandleWin();
                pourObjectAR.SetActive(false);
            }
        }
    }









    public void OnCorrectLetterGuessed()
    {
        correctGuesses++;
        Debug.Log($"Correct letter guessed! Current: {correctGuesses} / Total Hidden: {hiddenLetterCount}");

        bool isArabic = HangmanAR.activeSelf;

        PlayCorrectReaction(isArabic); // 🎬 Unified pour + milk logic

        // ✅ Both modes now check win centrally
        if (HangmanEN.activeSelf && remainingTiles == 0)
            CheckWinCondition();
        else if (HangmanAR.activeSelf)
            CheckWinCondition();
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





    private IEnumerator HandleMilkMeterProgression(float delay)
    {
        yield return new WaitForSeconds(delay);

        bool isArabic = HangmanAR.activeSelf;
        GameObject[] activeMeter = isArabic ? arabicMilkMeters : englishMilkMeters;

        if (activeMeter == null || activeMeter.Length == 0)
        {
            Debug.LogWarning("⚠ Milk meter array is empty!");
            yield break;
        }

    // 🧹 Hide splash and pour visuals
    (isArabic ? splashObjectAR : splashObjectEN)?.SetActive(false);
        (isArabic ? pourObjectAR : pourObjectEN)?.SetActive(false);

        int totalHiddenLetters = isArabic ? hideLettersAR.Count : hideLettersEN.Count;
        float fillRatio = Mathf.Clamp01((float)correctGuesses / Mathf.Max(1, totalHiddenLetters));
        int targetIndex = Mathf.Clamp(Mathf.RoundToInt(fillRatio * (activeMeter.Length - 1)), 0, activeMeter.Length - 1);

        for (int i = 0; i < activeMeter.Length; i++)
            activeMeter[i].SetActive(i == targetIndex);

        currentMilkState = targetIndex;

        Debug.Log($"🥛 Milk meter updated → State {currentMilkState + 1}/{activeMeter.Length} (Correct: {correctGuesses} / Total Hidden: {totalHiddenLetters})");

        if (currentMilkState == activeMeter.Length - 1)
        {
            Debug.Log("✅ Final milk state reached — triggering pour animation…");
            StartCoroutine(TriggerMilkPourAndWin());
        }
    }





    private IEnumerator TriggerMilkPourAndWin()
    {
        float pourDuration = HangmanAR.activeSelf ? milkPourDurationAR : milkPourDurationEN;

        // Optional: Activate pour/splash visuals or play an animation
        GameObject pourObject = HangmanAR.activeSelf ? pourObjectAR : pourObjectEN;
        if (pourObject != null)
            pourObject.SetActive(true);

        // Optional: Trigger animator here
        Animator pourAnimator = pourObject?.GetComponent<Animator>();
        pourAnimator?.SetTrigger("Pour");

        Debug.Log($"⏳ Waiting {pourDuration}s for milk pour animation…");
        yield return new WaitForSeconds(pourDuration);

        Debug.Log("🥛 Pour complete — launching win flow.");
        HandleWin();
    }


    [Header("Win Video Timing")]
    public float winVideoDurationAR = 4f;
    public float winVideoDurationEN = 4.5f;







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
        StartCoroutine(WaitForMilkPourThenStartWinSequence());
    }

    private IEnumerator WaitForMilkPourThenStartWinSequence()
    {
        Debug.Log("⏳ Waiting for milk pour animation before win sequence…");

        float pourDuration = HangmanAR.activeSelf ? milkPourDurationAR : milkPourDurationEN;

        // 🥛 Trigger pour animation
        Animator pourAnimator = HangmanAR.activeSelf ? pourObjectAR.GetComponent<Animator>() : pourObjectEN.GetComponent<Animator>();
        pourAnimator?.SetTrigger("Pour");

        yield return new WaitForSeconds(pourDuration);

        Debug.Log("🥛 Pour complete — launching win sequence");

        StartCoroutine(PlayTransitionOnceThenHide());
        StartCoroutine(HandleWinSequence());
    }



    private IEnumerator HandleWinSequence()
    {
        Debug.Log("🏆 Player won! Starting win sequence…");

        // 🎵 Pause theme music
        if (themeAudio != null && themeAudio.isPlaying)
            themeAudio.Pause();

        // 🔇 Mute all game sounds
        AudioListener.volume = 0f;

        // 🧹 Move pour visuals off-screen
        if (HangmanAR.activeSelf)
            pourObjectAR.transform.position = offScreenPosition;
        else
            pourObjectEN.transform.position = offScreenPosition;

        // 🌍 Select appropriate video and screen
        VideoPlayer player = HangmanAR.activeSelf ? videoPlayerAR : videoPlayerEN;
        GameObject screen = HangmanAR.activeSelf ? videoScreenAR : videoScreenEN;

        // ✅ Activate both before playback
        player.gameObject.SetActive(true);
        screen.SetActive(true);

        // 🖼 Ensure RawImage is assigned correctly
        RawImage rawImage = screen.GetComponentInChildren<RawImage>();
        if (rawImage != null && player.targetTexture != null)
            rawImage.texture = player.targetTexture;

        if (player.targetTexture != null)
            player.targetTexture.Release();

        Debug.Log($"🎥 Playing {(HangmanAR.activeSelf ? "Arabic" : "English")} win video directly");

        // ⏱ Wait for custom duration before win panel
        float delay = HangmanAR.activeSelf ? winVideoDurationAR : winVideoDurationEN;
        yield return new WaitForSeconds(delay);

        player.Pause(); // optional — prevents accidental loop

        // 🏆 Activate correct win panel
        if (HangmanAR.activeSelf)
        {
            WinPanelAR.SetActive(true);
            Debug.Log("✅ Arabic win panel activated!");
        }
        else
        {
            WinPanel.SetActive(true);
            Debug.Log("✅ English win panel activated!");
        }

        // 🧹 Hide video objects
        videoPlayerAR.gameObject.SetActive(false);
        videoScreenAR.SetActive(false);
        videoPlayerEN.gameObject.SetActive(false);
        videoScreenEN.SetActive(false);

        // 🔊 Restore audio
        AudioListener.volume = 1f;

        // 🔔 Play end win audio
        if (endWinAudio != null && endWinAudio.clip != null)
        {
            // Avoid replaying too quickly in overlapping coroutines
            float clipLength = endWinAudio.clip.length;

            if (!endWinAudio.isPlaying)
            {
                endWinAudio.volume = 1f;
                endWinAudio.loop = false;
                endWinAudio.playOnAwake = false;
                endWinAudio.spatialBlend = 0f;
                endWinAudio.outputAudioMixerGroup = null;

                if (!endWinAudio.gameObject.activeSelf)
                    endWinAudio.gameObject.SetActive(true);
                if (!endWinAudio.enabled)
                    endWinAudio.enabled = true;

                endWinAudio.Stop();
               endWinAudio.Play();

                Debug.Log($"🔊 Win sound triggered → Clip: {endWinAudio.clip.name}");

                yield return new WaitForSeconds(clipLength);
            }
            else
            {
                Debug.Log($"⏳ Skipped duplicate playback — already playing: {endWinAudio.clip.name}");
                yield return new WaitForSeconds(clipLength);
            }
        }
        else
        {
            Debug.LogWarning("❌ End win audio missing or clip not assigned!");
        }


        // 🎶 Resume theme music
        SongAudio.Play(); 
       // if (themeAudio != null) themeAudio.UnPause();
        Debug.Log("🎶 Theme music resumed — win sequence complete");
    }




    // ─── No changes below this line; helpers still work as before ──────────────
    /*  private void StopAllAudio()
      {
          if (correctAudio?.isPlaying == true) correctAudio.Stop();
          if (incorrectAudio?.isPlaying == true) incorrectAudio.Stop();
          if (milkFillAudio?.isPlaying == true) milkFillAudio.Stop();
          //if (winAudio?.isPlaying == true) winAudio.Stop(); 
      } */


    [Header("Milk Pour Timing")]
    public float milkPourDurationAR = 2.5f;
    public float milkPourDurationEN = 2.5f;




    // ─── Updated Win Panel Coroutine ───────────────────────────────────────────
    IEnumerator ActivateWinPanelAfterDelay()
    {
        // 🎥 Choose video player based on mode
        VideoPlayer player = HangmanAR.activeSelf ? videoPlayerAR : videoPlayerEN;

        // ⏱ Use manually defined duration
        float duration = HangmanAR.activeSelf ? winVideoDurationAR : winVideoDurationEN;
        Debug.Log($"⏳ Custom win video display time: {duration} seconds");

        // 🔧 Prepare video
        player.Prepare();
        while (!player.isPrepared) yield return null;

        // ▶️ Play video
        player.Play();
        Debug.Log("🎬 Win video started");

        // 🎶 Mute global audio
        AudioListener.volume = 0f;

        // 🕒 Wait for custom duration instead of full video length
        yield return new WaitForSeconds(duration);

        // 🛑 Pause video (optional, prevents auto-loop or trailing frames)
        player.Pause();

        // 🏆 Show Win Panel
        if (HangmanAR.activeSelf)
        {
            WinPanelAR.SetActive(true);
            Debug.Log("✅ Arabic win panel activated!");
        }
        else
        {
            WinPanel.SetActive(true);
            Debug.Log("✅ English win panel activated!");
        }

        // 🧹 Hide video elements
        videoPlayerEN.gameObject.SetActive(false);
        videoScreenEN.SetActive(false);
        videoPlayerAR.gameObject.SetActive(false);
        videoScreenAR.SetActive(false);

        // 🔊 Restore global audio
        AudioListener.volume = 1f;

        // 🔔 Play endWinAudio if available
       

        // 🔁 Resume theme music
        if (themeAudio != null) themeAudio.UnPause();
        Debug.Log("🎶 Theme music resumed");
    }







    public AudioSource endWinAudio;




    void AdjustMilkMeterCount()
    {
        if (HangmanAR.activeSelf)
        {
            hiddenLetterCount = hideLettersAR.Count;

            if (arabicMilkMeters == null || arabicMilkMeters.Length != 7)
            {
                Debug.LogWarning($"⚠ Arabic milk meter setup should have 7 states (currently: {arabicMilkMeters?.Length ?? 0})");
            }
            else
            {
                Debug.Log("✅ Arabic milk meters linked from Inspector — using preassigned states");
            }
        }
        else if (HangmanEN.activeSelf)
        {
            hiddenLetterCount = hideLettersEN.Count;

            if (englishMilkMeters == null || englishMilkMeters.Length != 11)
            {
                Debug.LogWarning($"⚠ English milk meter setup should have 11 states (currently: {englishMilkMeters?.Length ?? 0})");
            }
            else
            {
                Debug.Log("✅ English milk meters linked from Inspector — using preassigned states");
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

    [System.Serializable]
    public class ArabicTileData
    {
        public GameObject tileObject;
        public List<char> tileLetters; // Assignable directly in Inspector
    }

    public List<ArabicTileData> arabicTiles = new List<ArabicTileData>();
    private int remainingArabicTiles = 0;


    // ─── Updated Arabic Initialization ──────────────────────────────────────────
    public void InitializeArabicGame()
    {
        remainingArabicTiles = 0;

        // Reset pour visuals
        pourObjectAR.SetActive(false);
        pourObjectAR.transform.position = originalPourARPosition;

        StartingScreen.SetActive(false);
        HangmanAR.SetActive(true);
        HangmanEN.SetActive(false);
        isArabicMode = true;
        AudioListener.volume = 1f;

        AssignArabicLetterValues();
        ValidateKeyboardInteraction();

        if (themeAudio != null) themeAudio.UnPause();
        if (SongAudio != null) SongAudio.Stop();

        HashSet<char> visibleChars = new HashSet<char>();

        for (int i = 0; i < arabicTiles.Count; i++)
        {
            // hide if any of this tile’s letters are in our fixed hide set
            bool shouldHide = arabicTiles[i]
                .tileLetters
                .Any(c => hideLettersAR.Contains(c));

            arabicTiles[i].tileObject.SetActive(!shouldHide);

            if (shouldHide)
            {
                remainingArabicTiles++;
            }
            else
            {
                // collect letters to grey out on keyboard
                foreach (char c in arabicTiles[i].tileLetters)
                    visibleChars.Add(c);
            }
        }

        // Grey out & disable arabic keys for visible letters
        foreach (var kv in arabicKeyboardMap)
        {
            char keyChar = kv.Key[0];
            if (visibleChars.Contains(keyChar))
            {
                var btn = kv.Value;
                btn.GetComponent<Image>().color = unhiddenKeyColor;
                btn.interactable = false;
            }
        }

        Debug.Log($"🕌 Arabic hidden tiles: {remainingArabicTiles} / {arabicTiles.Count}");
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

        if (!arabicKeyboardMap.TryGetValue(letter, out Button pressedButton) || pressedButton == null)
        {
            Debug.LogError($"ERROR: Arabic letter '{letter}' not found!");
            return;
        }

        // 🔒 Disable interaction immediately
        pressedButton.interactable = false;

        char guess = letter[0];
        bool correctGuess = false;

        foreach (var tileData in arabicTiles)
        {
            if (!tileData.tileObject.activeSelf && tileData.tileLetters.Contains(guess))
            {
                tileData.tileObject.SetActive(true);
                remainingArabicTiles--;
                correctGuess = true;
                Debug.Log($"✅ Revealed tile with Arabic letter '{guess}'");
            }
        }

        if (correctGuess)
        {
            pressedButton.GetComponent<Image>().color = Color.green;
            correctAudio?.PlayOneShot(correctAudio.clip);

            arabicCowHappy.SetActive(true);
            arabicHappyKids.SetActive(true);
            arabicCowAngry.SetActive(false);
            arabicSadKids.SetActive(false);
            arabicIdleKids.SetActive(false);

            OnCorrectLetterGuessed();

            if (remainingArabicTiles <= 0)
            {
                Debug.Log("🏆 All Arabic tiles revealed! Player wins.");
                HandleWin();
            }
        }
        else
        {
            pressedButton.GetComponent<Image>().color = Color.red;
            incorrectAudio?.PlayOneShot(incorrectAudio.clip);
            incorrectAttempts++;
            StartCoroutine(IncorrectReactionArabic());

            if (incorrectAttempts >= maxAttempts)
            {
                EndGame(false);
            }
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

    [Header("Reaction Timing")]
    public float kidsResetDelay = 0.75f;
    public float cowAndPourDelay = 0.1f;

    public void PlayCorrectReaction(bool isArabic)
    {
        StartCoroutine(PlayReactionSequence(isArabic));
    }

    private IEnumerator PlayReactionSequence(bool isArabic)
    {
        // 🎭 Kids setup
        GameObject kidsIdle = isArabic ? arabicIdleKids : englishIdleKids;
        GameObject kidsHappy = isArabic ? arabicHappyKids : englishHappyKids;

        // 🐄 Cow setup
        GameObject cowNormal = isArabic ? arabicCowNormal : englishCowNormal;
        GameObject cowHappy = isArabic ? arabicCowHappy : englishCowHappy;

        // 🥛 Pouring setup
        GameObject pourObj = isArabic ? pourObjectAR : pourObjectEN;
        Animator pourAnim = isArabic ? arabicPouringAnimation : englishPouringAnimation;

        // 👦 STEP 1: Show happy kids
        kidsIdle.SetActive(false);
        kidsHappy.SetActive(true);

        yield return new WaitForSeconds(kidsResetDelay); // e.g., 0.75f

        kidsHappy.SetActive(false);
        kidsIdle.SetActive(true);

        yield return new WaitForSeconds(cowAndPourDelay); // e.g., 0.1f

        // 🐄 STEP 2: Cow reacts
        cowNormal.SetActive(false);
        cowHappy.SetActive(true);

        // 🥛 STEP 3: Start pour + play milk audio
        if (pourObj && pourAnim)
        {
            pourObj.SetActive(true);
            pourAnim.SetBool("isPouring", true);
            pourAnim.Play("pour", 0, 0f);

            // 🟢 Start milk audio at pour start
            if (milkFillAudio)
            {
                if (milkFillAudio.isPlaying) milkFillAudio.Stop();
                milkFillAudio.Play();
                Debug.Log("🔊 Milk fill audio started with pour animation.");
            }
            else
            {
                Debug.LogWarning("⚠️ milkFillAudio is not assigned!");
            }

            float clipLength = pourAnim.runtimeAnimatorController.animationClips
                .FirstOrDefault(c => c.name == "pour")?.length ?? 1.5f;

            yield return new WaitForSeconds(clipLength);

            pourAnim.SetBool("isPouring", false);
            pourObj.SetActive(false);

            // Reset cow to neutral
            cowHappy.SetActive(false);
            cowNormal.SetActive(true);

            // 🧪 Update milk meter
            StartCoroutine(HandleMilkMeterProgression(0f));
        }
    }



    //Transtion 

   
    void Update()
    {
        
    }

    public VideoPlayer transitionVideoPlayer;
    public GameObject transitionScreen;
    public float transitionClipDuration = 1f;

    private IEnumerator PlayTransitionOnceThenHide()
    {
        if (transitionVideoPlayer == null || transitionScreen == null)
        {
            Debug.LogWarning("⚠ Transition video or screen is not assigned!");
            yield break;
        }

        transitionVideoPlayer.Stop(); // 🔁 Reset previous playback
        transitionVideoPlayer.Prepare(); // ⏳ Let it fully load

        while (!transitionVideoPlayer.isPrepared)
        {
            yield return null; // Wait until prepared
        }

        transitionScreen.SetActive(true);
        transitionVideoPlayer.gameObject.SetActive(true);

        // 🖼 Ensure texture binding
        RawImage rawImage = transitionScreen.GetComponentInChildren<RawImage>();
        if (rawImage != null && transitionVideoPlayer.targetTexture != null)
        {
            rawImage.texture = transitionVideoPlayer.targetTexture;
        }

        transitionVideoPlayer.Play();
        Debug.Log($"🎥 Transition video prepared & started → Duration: {transitionClipDuration}");

        float adjustedDuration = transitionClipDuration / Mathf.Max(0.01f, transitionVideoPlayer.playbackSpeed);
        yield return new WaitForSeconds(adjustedDuration);

        transitionVideoPlayer.Stop();
        transitionScreen.SetActive(false);
        transitionVideoPlayer.gameObject.SetActive(false);

        Debug.Log("🧹 Transition video cleaned up.");
    }

    private bool hasPlayedEndWinAudio = false; // Add at top

    void PlayEndWinAudioOnce()
    {
        if (hasPlayedEndWinAudio || endWinAudio == null || endWinAudio.clip == null)
            return;

        hasPlayedEndWinAudio = true;
        endWinAudio.volume = 1f;
        endWinAudio.loop = false;
        endWinAudio.playOnAwake = false;
        endWinAudio.spatialBlend = 0f;
        endWinAudio.outputAudioMixerGroup = null;

        if (!endWinAudio.gameObject.activeSelf) endWinAudio.gameObject.SetActive(true);
        if (!endWinAudio.enabled) endWinAudio.enabled = true;

        endWinAudio.Stop();
        endWinAudio.Play();

        Debug.Log($"🔊 Played End Win Audio → {endWinAudio.clip.name}");
    }


}
