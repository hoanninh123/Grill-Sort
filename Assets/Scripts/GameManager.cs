using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]private int _foodTotal;
    [SerializeField]private int _grillTotal;
    [SerializeField]private Transform grid;
    List<GrillStation> _listGrills;
    private float avgTray;
    List<Sprite> resSprite;

    // Start is called before the first frame update
    void Awake()
    {
        Sprite[] resourceSprite = Resources.LoadAll<Sprite>("item");
        resSprite = resourceSprite.ToList<Sprite>();
        _listGrills = Utils.GetListFromChild<GrillStation>(grid);
    }

    // Update is called once per frame
    void Start()
    {
        OnInitLever();
    }
    public void OnInitLever()
    {
        List<Sprite> takefood = resSprite.OrderBy(x => Random.value).Take(_foodTotal).ToList();
        List<Sprite> usefood = new List<Sprite>();
        for(int i = 0; i < takefood.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                usefood.Add(takefood[i]);
            }
        }
        for (int i = 0; i < takefood.Count; i++)
        {
            int rand = Random.Range(0, takefood.Count);
            (usefood[i], usefood[rand]) = (usefood[rand], usefood[i]);
        }
        //tinh trung binh khay
        avgTray = Random.Range(1.5f, 2.5f);
        int totalTray = usefood.Count / (int)avgTray;
        //chia deu
        List<int> trayPerGrill = distributeEvenly(_grillTotal, totalTray);
        List<int> foodPerGrill = distributeEvenly(_grillTotal, usefood.Count);
        //Displays
        for (int i = 0; i < _foodTotal; i++)
        {
            bool activeGrill = i < _grillTotal;
            _listGrills[i].gameObject.SetActive(activeGrill);

            if (activeGrill)
            {
                List<Sprite> lisFood = Utils.TakeAndRemoveRandom<Sprite>(usefood, foodPerGrill[i]);
                _listGrills[i].OnInitGrill(trayPerGrill[i], lisFood);

            }
        }
    }
    //chia deu
    public List<int> distributeEvenly(int totalGrill,int totalTray){
        List<int> result = new List<int>();
        //chia so luong dia
        int low = totalTray / totalGrill;
        int high = low + 1;
        //chia dia ra grill
        int highCount = totalTray - low * totalGrill;
        int lowCount = totalGrill - highCount;
        //
        for(int i = 0; i < highCount; i++)
        {
            result.Add(high);
        }
        for (int i = 0; i < lowCount; i++)
        {
            result.Add(low);
        }
        for(int i = 0; i < result.Count; i++)
        {
            int rand = Random.Range(0, result.Count);
            (result[i], result[rand]) = (result[rand], result[i]);
        }
        return result;
    }
}
