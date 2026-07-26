using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the main gameplay scene to load when clicking Play.")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("UI Panels")]
    [SerializeField] private GameObject mainButtonsPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject prologuePanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("Prologue Typewriter Settings")]
    [SerializeField] private TMP_Text prologueText;
    [Tooltip("Time delay between each character appearing (in seconds).")]
    [SerializeField] private float charTypeDelay = 0.04f;
    [Tooltip("Pause duration after finishing a line before typing the next line.")]
    [SerializeField] private float linePauseDelay = 0.8f;
    
    [TextArea(3, 6)]
    [SerializeField] private string[] prologueLines = new string[]
    {
        "Long ago, an ancient force awakened in the shadows...",
        "Many tried to stop it, but none succeeded.",
        "Now, the fate of the realm rests in your hands..."
    };

    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private string fullPrologueCombinedText = "";

    [Header("Loading Screen UI")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text loadingText;

    /// <summary>
    /// Triggered when pressing PLAY. Opens prologue if available, otherwise starts loading.
    /// </summary>
    public void PlayGame()
    {
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);

        if (prologuePanel != null && prologueLines.Length > 0)
        {
            ShowPrologue();
        }
        else
        {
            StartLoading();
        }
    }

    #region Prologue Typewriter Logic
    private void ShowPrologue()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (prologuePanel != null) prologuePanel.SetActive(true);

        // Build complete full text string for instant skip/reveal reference
        fullPrologueCombinedText = string.Join("\n\n", prologueLines);

        if (prologueText != null)
        {
            prologueText.text = "";
        }

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        typewriterCoroutine = StartCoroutine(TypewritePrologueCoroutine());
    }

    private IEnumerator TypewritePrologueCoroutine()
    {
        isTyping = true;
        if (prologueText != null) prologueText.text = "";

        for (int i = 0; i < prologueLines.Length; i++)
        {
            string line = prologueLines[i];

            // Type out current line character by character
            for (int c = 0; c < line.Length; c++)
            {
                if (prologueText != null)
                {
                    prologueText.text += line[c];
                }
                yield return new WaitForSeconds(charTypeDelay);
            }

            // Pause briefly after completing each line (unless it's the last line)
            if (i < prologueLines.Length - 1)
            {
                if (prologueText != null) prologueText.text += "\n\n";
                yield return new WaitForSeconds(linePauseDelay);
            }
        }

        isTyping = false;
    }

    /// <summary>
    /// Called when clicking NEXT or tapping screen. 
    /// If text is still typing, instantly completes all text. 
    /// If typing is already done, starts loading the game.
    /// </summary>
    public void NextPrologueLine()
    {
        if (isTyping)
        {
            // Instantly reveal all text on screen
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            if (prologueText != null) prologueText.text = fullPrologueCombinedText;
            isTyping = false;
        }
        else
        {
            // Text is already fully shown, proceed to loading
            if (prologuePanel != null) prologuePanel.SetActive(false);
            StartLoading();
        }
    }

    /// <summary>
    /// Skips the entire prologue and proceeds to loading immediately.
    /// </summary>
    public void SkipPrologue()
    {
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        isTyping = false;
        if (prologuePanel != null) prologuePanel.SetActive(false);
        StartLoading();
    }
    #endregion

    #region Async Loading Logic
    public void StartLoading()
    {
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
        if (prologuePanel != null) prologuePanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(true);

        StartCoroutine(LoadSceneAsyncCoroutine());
    }

    private IEnumerator LoadSceneAsyncCoroutine()
    {
        AsyncOperation asyncOp = !string.IsNullOrEmpty(gameSceneName)
            ? SceneManager.LoadSceneAsync(gameSceneName)
            : SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);

        asyncOp.allowSceneActivation = true;

        while (!asyncOp.isDone)
        {
            float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);

            if (loadingSlider != null)
            {
                loadingSlider.value = progress;
            }

            if (loadingText != null)
            {
                loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
            }

            yield return null;
        }
    }
    #endregion

    #region Options & Quit
    public void OpenOptions()
    {
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion
}
