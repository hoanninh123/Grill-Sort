using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance => _instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject _panelGame;
    [SerializeField] private GameObject _panelWin;
    
    [Header("Scene Loading")]
    [SerializeField] private string _mainSceneName = "Main";
    [SerializeField] private string _gameSceneName = "Game";

    [Header("Win Screen Buttons")]
    [SerializeField] private RectTransform _replayButton;
    [SerializeField] private RectTransform _homeButton;

    public static bool shouldPlayImmediately = false;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        // Deactivate the EndGame UI when the game starts
        if (_panelWin != null)
            _panelWin.SetActive(false);

        // Programmatically bind buttons so they are guaranteed to work even if not bound in the Inspector!
        if (_replayButton != null)
        {
            UnityEngine.UI.Button btn = _replayButton.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnClickReplay);
            }
        }

        if (_homeButton != null)
        {
            UnityEngine.UI.Button btn = _homeButton.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnClickBackToMain);
            }
        }

        // If reloading via Play Again, bypass the home screen and jump straight to gameplay!
        if (shouldPlayImmediately)
        {
            shouldPlayImmediately = false; // Reset the flag
            OnClickPlayInSameScene();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnGameCompleted += HandleGameCompleted;
        GameEvents.OnFoodCountChanged += HandleFoodCountChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameCompleted -= HandleGameCompleted;
        GameEvents.OnFoodCountChanged -= HandleFoodCountChanged;
    }

    /// <summary>
    /// Loads the Game Scene (separate scene)
    /// </summary>
    public void OnClickPlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_gameSceneName);
    }

    /// <summary>
    /// Loads the Main Menu Scene (separate scene)
    /// </summary>
    public void OnClickBackToMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_mainSceneName);
    }

    /// <summary>
    /// Switches screens in the same scene to start the game
    /// </summary>
    public void OnClickPlayInSameScene()
    {
        Time.timeScale = 1f;

        if (_panelGame != null)
            _panelGame.SetActive(true);

        if (_panelWin != null)
            _panelWin.SetActive(false);
    }

    /// <summary>
    /// Cleanly reloads the scene and restarts the game
    /// </summary>
    public void OnClickReplay()
    {
        Time.timeScale = 1f;
        shouldPlayImmediately = true; // Signal the newly loaded UIManager to play instantly
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void HandleFoodCountChanged(int remainFood)
    {
        // Hook this for counter text later if needed
    }

    public void HandleGameCompleted()
    {
        // Switch screens instantly when player completes the level
        if (_panelWin != null)
        {
            _panelWin.SetActive(true);
            _panelWin.transform.SetAsLastSibling(); // Force Victory screen to the absolute front so nothing blocks clicks!

            // Ensure the CanvasGroup is fully interactable if present
            CanvasGroup canvasGroup = _panelWin.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        if (_panelGame != null)
            _panelGame.SetActive(false);
    }
}
