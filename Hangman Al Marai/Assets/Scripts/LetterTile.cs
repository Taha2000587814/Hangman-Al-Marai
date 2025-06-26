using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterTile : MonoBehaviour
{
    public char letterValue; // Assign in Inspector
    public bool IsRevealed => gameObject.activeSelf;

    public void HideTile()
    {
        gameObject.SetActive(false);
    }

    public void RevealTile()
    {
        gameObject.SetActive(true);
    }
}

    

