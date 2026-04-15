using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance => _instance;

    [Header("Play Button")]
    [SerializeField] private GameObject _panelHome;
    [SerializeField] private GameObject _panelGame;
    [SerializeField] private GameObject _panelWin;
    [SerializeField] private string _mainSceneName = "Main";
    [SerializeField] private string _gameSceneName = "Game";

    private void Awake()
    {
        _instance = this;
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

    public void OnClickPlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_gameSceneName);
    }

    public void OnClickBackToMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_mainSceneName);
    }

    public void OnClickPlayInSameScene()
    {
        Time.timeScale = 1f;

        if (_panelHome != null)
            _panelHome.SetActive(false);

        if (_panelGame != null)
            _panelGame.SetActive(true);

        if (_panelWin != null)
            _panelWin.SetActive(false);
    }

    private void HandleFoodCountChanged(int remainFood)
    {
        // Hook this for counter text later without coupling UIManager to GameManagers.
    }

    private void HandleGameCompleted()
    {
        if (_panelWin != null)
            _panelWin.SetActive(true);

        if (_panelGame != null)
            _panelGame.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
