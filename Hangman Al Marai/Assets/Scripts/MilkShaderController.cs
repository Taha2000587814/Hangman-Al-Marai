using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilkShaderController : MonoBehaviour
{
    [Header("Fill Settings")]
    [Tooltip("Material with milk shader applied")]
    public Material milkMaterial;

    [Tooltip("Total hidden letters (non-duplicate)")]
    public int totalHiddenLetters = 5;

    [Tooltip("Current number of correctly guessed letters")]
    public int correctGuesses = 0;

    private static readonly string progressProperty = "_FillProgress";

    void Start()
    {
        if (milkMaterial == null)
            Debug.LogWarning("⚠ Milk material not assigned!");
        UpdateMilkLevel();
    }

    /// <summary>
    /// Call this after each correct letter is revealed.
    /// </summary>
    public void RegisterCorrectGuess()
    {
        correctGuesses = Mathf.Clamp(correctGuesses + 1, 0, totalHiddenLetters);
        UpdateMilkLevel();
    }

    void UpdateMilkLevel()
    {
        if (milkMaterial == null) return;

        float fillRatio = (float)correctGuesses / Mathf.Max(1, totalHiddenLetters);
        milkMaterial.SetFloat(progressProperty, fillRatio);

        Debug.Log($"🥛 Milk shader updated — {correctGuesses}/{totalHiddenLetters} → {Mathf.RoundToInt(fillRatio * 100f)}%");
    }
}
