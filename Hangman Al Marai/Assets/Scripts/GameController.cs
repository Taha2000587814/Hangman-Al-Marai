using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Linq;
using ArabicSupport;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System;
using Unity.VisualScripting;
public class GameController : MonoBehaviour
{
    public AudioSource VideoAudioEN; 
    string sentence;
    string[] words;
    [SerializeField] HorizontalLayoutGroup wordSlots;
    [SerializeField] GameObject wordPrefab;
    [SerializeField] GameObject letterPrefab;
    [SerializeField] float spaceBetweenWords;
    int healthPoints = 8;
    int score = 0;
    [SerializeField] Animator kidsAnimator;
    [SerializeField] Animator glassAnimator;
    public string language = "ar";
    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject loseScreen;
    bool gameEnded = false;
    [SerializeField] AudioClip[] clips;
    AudioSource audioSource;
    [SerializeField] Animator milkAnimator;
    [SerializeField] Animator cowAnimator;
    int winScore;
    [SerializeField] Sprite englishWin;
    [SerializeField] Sprite arabicWin; 
    [SerializeField] SpriteRenderer tryAgainScreen;
    [SerializeField] Sprite arabicTryAgain;
    [SerializeField] Sprite englishTryAgain;
    [SerializeField] GameObject arabicSentence;
    [SerializeField] GameObject logo;
[SerializeField] Sprite englishLogo;
[SerializeField] GameObject videoScreen;
VideoPlayer videoPlayer;
    public GameObject WinIMG;
   // public GameObject VPOBj; 

    //New 
    public GameObject KidsIdle;
    public GameObject KidsHappy;
    public GameObject KidsSad;
    public VideoPlayer videoPlayerEN;
    public VideoPlayer videoPlayerAR;
    public RenderTexture renderTexture;

    bool playVideo = false;
    // Start is called before the first frame update
    void Start()
    {videoPlayer = GetComponent<VideoPlayer>();
        arabicSentence.SetActive(false);
            audioSource = GetComponent<AudioSource>();
        KidsIdle.SetActive(true);
        KidsHappy.SetActive(false);
        KidsSad.SetActive(false);
        WinIMG.SetActive(false);
        // VPOBj.SetActive(false); 

        // Ensure both players are stopped initially
        videoPlayerEN.Stop();
        videoPlayerAR.Stop();
    }

    public void GenerateSlots(bool arabic){



        if (arabic){
            language = "ar";
            winScore = 6;
            sentence = ReverseString("شرب الحليب سر اللبيب");
            
            wordSlots.gameObject.SetActive(false);
            arabicSentence.SetActive(true);
            wordSlots = arabicSentence.GetComponent<HorizontalLayoutGroup>();
            words = sentence.Split(" ");
            TryLetter("ب");
        TryLetter("ر");
return;
        } else {
            logo.GetComponent<Image>().sprite = englishLogo;
            language = "en";
            winScore = 10;
            sentence = "milk everyday is the smart way";
            wordSlots.transform.localScale = new Vector3(.62f,.9f,.9f);
            words = sentence.Split(" ");
        }
        
        
        int lettersBeforeThisWord = 0;
        foreach (string _word in words)
        {

            Transform newWord = Instantiate(wordPrefab,wordSlots.transform).transform;
            newWord.GetComponent<HorizontalLayoutGroup>().padding.left = (int)(letterPrefab.GetComponent<RectTransform>().rect.width*spaceBetweenWords * lettersBeforeThisWord);
            foreach (char c in _word)
            {
                LetterScript letter = Instantiate(letterPrefab,newWord).GetComponent<LetterScript>();
                letter.trueLetter = ArabicFixer.Fix(c.ToString());
            }
            lettersBeforeThisWord += _word.Length;
        }

        TryLetter("i");
        TryLetter("s");
        TryLetter("r");
        TryLetter("y");

    }

    public bool TryLetter(string letter){
        // string letter = debugText.text;
        bool correct = false;
        int i = 0;
        foreach (string word in words)
        {   

            string _word = word; 
            int charsRemoved = 0;
            while(_word.IndexOf(letter) != -1){            
                
            int charIndex = _word.IndexOf(letter) + charsRemoved;
            wordSlots.transform.GetChild(i).GetChild(charIndex).GetComponent<LetterScript>().ShowLetter();
            correct = true;
            _word = _word.Remove(_word.IndexOf(letter),1);
            charsRemoved++;
            }
            i++;
        }
        if(correct){
            List<string> BegLetters = new() { "y", "r", "s", "i", "ر", "ب" };
            bool bl = !BegLetters.Contains(letter);
            if (bl)
            {  
                RightAnswer();
            }
        }
        else{
            WrongAnswer();
        }
        return correct;
    }

    void RightAnswer(){
        kidsAnimator.Play("Celebrate");
        cowAnimator.Play("happyCow");
        milkAnimator.Play("pour");
        glassAnimator.SetTrigger("wrong");
        ActivateKidsHappy();
         score++;

        audioSource.clip = clips[1];
        audioSource.Play();
        audioSource.clip = clips[2];
        audioSource.Play();
        if (score == winScore){
            videoScreen.SetActive(true);
            videoPlayer.Play();
            VideoAudioEN.Play(); 
                  }
    }
    void WrongAnswer(){

        cowAnimator.Play("cow");
        kidsAnimator.Play("Sad");
        ActivateKidsSad();


        healthPoints--;
        audioSource.clip = clips[4];
        audioSource.Play();
        
        if(healthPoints == 0){
            Lose();
        }
    }
    
    void Win(){
       // VPOBj.SetActive(true); 
        videoScreen.SetActive(false);
        PlayVideoBasedOnLanguage(); // Play the correct video

        if (language == "ar")   winScreen.GetComponentInChildren<SpriteRenderer>().sprite = arabicWin;
    if (language == "en")   winScreen.GetComponentInChildren<SpriteRenderer>().sprite = englishWin;

    gameEnded = true;
    kidsAnimator.Play("Win");
        ActivateKidsHappy();
        winScreen.SetActive(true);
        WinIMG.SetActive(true); 
    
        audioSource.clip = clips[2];
        audioSource.Play();
    }
    void Lose(){
        if (language == "ar")   tryAgainScreen.GetComponentInChildren<SpriteRenderer>().sprite = arabicTryAgain;
        if (language == "en")   tryAgainScreen.GetComponentInChildren<SpriteRenderer>().sprite = englishTryAgain;

        loseScreen.SetActive(true);
            gameEnded = true;
            audioSource.clip = clips[3];
        audioSource.Play();
        kidsAnimator.Play("Lose");
        ActivateKidsSad();



    }

    public void ReloadScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public string ReverseString(string stringToReverse)
    {
        string reversedString = "";

        for(int i = stringToReverse.Length - 1; i >= 0; i--)
        {
            reversedString += stringToReverse[i];
        }

        return reversedString;
    }


    //New Functions 
    void ActivateKidsSad()
    {
        StartCoroutine(SwitchState(KidsSad));
    }

    void ActivateKidsHappy()
    {
        StartCoroutine(SwitchState(KidsHappy));
    }

    IEnumerator SwitchState(GameObject obj)
    {
        KidsIdle.SetActive(false); // Disable KidsIdle
        obj.SetActive(true); // Activate selected object
        yield return new WaitForSeconds(1f);
        obj.SetActive(false); // Disable selected object
        KidsIdle.SetActive(true); // Re-enable KidsIdle
    }


    void PlayVideoBasedOnLanguage()
    {
        if (language == "ar")
        {
            videoPlayerAR.targetTexture = renderTexture;
           // videoPlayerAR.Stop(); // Stop the video first
            videoPlayerAR.time = 0; // Reset video to the beginning
            videoPlayerAR.Play();
        }
        else if (language == "en")
        {
            videoPlayerEN.targetTexture = renderTexture;
          //  videoPlayerEN.Stop();
            videoPlayerEN.time = 0; // Reset video to the beginning
            videoPlayerEN.Play();
        }
    }


}
