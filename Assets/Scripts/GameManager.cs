using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelData
    {
        public int levelNumber;
        public int totalFoodTypes; // _totalFood
        public int totalFoodSets;  // _allFood
        public int totalGrills;    // _totalGrill
    }

    [System.Serializable]
    public class LevelDatabase
    {
        public List<LevelData> levels;
    }

    private static GameManager _instance;
    public static GameManager Instance => _instance;

    [SerializeField] private int _allFood;
    [SerializeField] private int _totalFood; // tong so loai thuc an
    [SerializeField] private int _totalGrill; // tong so bep
    [SerializeField] private Transform _gridGrill;

    private List<GrillStation> _listGrills;
    private float _avgTray; // gia tri trung binh thuc an cho 1 dia
    private List<Sprite> _totalSpriteFood;
    private LevelDatabase _levelDb;
    private int _currentLevel = 1;

    private void Awake()
    {
        _listGrills = Utils.GetListInChild<GrillStation>(_gridGrill);
        Sprite[] loadedSprite = Resources.LoadAll<Sprite>("Items");
        _totalSpriteFood = loadedSprite.ToList();
        _instance = this;

        LoadLevelDataFromJSON();
    }

    void Start()
    {
        _currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLeverText(_currentLevel);
        }

        ApplyLevelData(_currentLevel);
        OnInitLevel();
        GameEvents.RaiseFoodCountChanged(_allFood);
    }

    private void LoadLevelDataFromJSON()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("levels");
        if (jsonFile != null)
        {
            _levelDb = JsonUtility.FromJson<LevelDatabase>(jsonFile.text);
        }
        else
        {
            Debug.LogWarning("File levels.json not found in Resources folder! Falling back to Inspector settings.");
        }
    }

    private void ApplyLevelData(int level)
    {
        if (_levelDb == null || _levelDb.levels == null || _levelDb.levels.Count == 0) return;

        // Clamp index so if player level exceeds defined levels, use the last level config
        int levelIndex = Mathf.Clamp(level - 1, 0, _levelDb.levels.Count - 1);
        LevelData data = _levelDb.levels[levelIndex];

        _totalFood = Mathf.Min(data.totalFoodTypes, _totalSpriteFood.Count);
        _allFood = data.totalFoodSets;
        _totalGrill = Mathf.Min(data.totalGrills, _listGrills.Count);
    }

    private void OnInitLevel()
    {
        int actualTakeFood = Mathf.Min(_totalFood, _totalSpriteFood.Count);
        List<Sprite> takeFood = _totalSpriteFood.OrderBy(x => Random.value).Take(actualTakeFood).ToList();
        List<Sprite> useFood = new List<Sprite>();

        for (int i = 0; i < _allFood; i++)
        {
            int n = i % takeFood.Count;
            for (int j = 0; j < 3; j++)
                useFood.Add(takeFood[n]);
        }

        _avgTray = Random.Range(1.4f, 2f);
        int totalTray = Mathf.RoundToInt(useFood.Count / _avgTray);

        int activeGrills = Mathf.Min(_totalGrill, _listGrills.Count);
        List<int> trayPerGrill = this.DistributeEvelyn(activeGrills, totalTray);
        List<int> foodPerGrill = this.DistributeEvelyn(activeGrills, useFood.Count);

        for (int i = 0; i < _listGrills.Count; i++)
        {
            bool activeGrill = i < activeGrills;
            _listGrills[i].gameObject.SetActive(activeGrill);

            if (activeGrill)
            {
                List<Sprite> lisFood = Utils.TakeAndRemoveRandom<Sprite>(useFood, foodPerGrill[i]);
                _listGrills[i].OnInitGrill(trayPerGrill[i], lisFood);
            }
        }
    }

    private List<int> DistributeEvelyn(int grillCount, int totalTrays)
    {
        List<int> result = new List<int>();

        if (grillCount <= 0) return result;

        // tinh trung binh so luong dia
        float avg = (float)totalTrays / grillCount;
        int low = Mathf.FloorToInt(avg);
        int high = Mathf.CeilToInt(avg);

        int hightCount = totalTrays - low * grillCount;
        int lowCount = grillCount - hightCount;

        for (int i = 0; i < lowCount; i++)
            result.Add(low);

        for (int i = 0; i < hightCount; i++)
            result.Add(high);

        // dao vi tri
        for (int i = 0; i < result.Count; i++)
        {
            int rand = Random.Range(i, result.Count);
            (result[i], result[rand]) = (result[rand], result[i]);
        }

        return result;
    }

    public void OnMinusFood()
    {
        --_allFood;
        GameEvents.RaiseFoodCountChanged(_allFood);

        if (_allFood <= 0)
        {
            // Tăng level và lưu lại khi hoàn thành màn chơi
            int nextLevel = _currentLevel + 1;
            PlayerPrefs.SetInt("CurrentLevel", nextLevel);
            PlayerPrefs.Save();

            UIManager.Instance.HandleGameCompleted(); 
        }
    }

    public void OnCheckAndShake()
    {
        Dictionary<string, List<FoodSlot>> groups = new Dictionary<string, List<FoodSlot>>();

        foreach (var grill in _listGrills)
        {
            if (grill.gameObject.activeInHierarchy)
            {
                for (int i = 0; i < grill.TotalSlot.Count; i++)
                {
                    FoodSlot slot = grill.TotalSlot[i];
                    if (slot.HasFood)
                    {
                        string name = slot.GetSpriteFood.name;
                        if (!groups.ContainsKey(name))
                            groups.Add(name, new List<FoodSlot>());

                        groups[name].Add(slot);
                    }
                }
            }
        }

        foreach (var kvp in groups)
        {
            if (kvp.Value.Count >= 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    kvp.Value[i].DoShake();
                }

                return;
            }
        }
    }
}