using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GrillStation : MonoBehaviour
{
    [SerializeField] private Transform slotStation;
    [SerializeField] private Transform trayStation;
    // Start is called before the first frame update
    List<TrayItem> totalTray;
    List<FoodSlot> totalSlots;
    void Awake()
    {
        totalSlots = Utils.GetListFromChild<FoodSlot>(slotStation);
        totalTray = Utils.GetListFromChild<TrayItem>(trayStation);
    }
    public void OnInitGrill(int totalTray,List<Sprite> listFood)
    {
        List<Sprite> listSlot = Utils.TakeAndRemoveRandom<Sprite>(listFood, totalTray);
        for(int i = 0; i < listSlot.Count; i++)
        {
            FoodSlot slot = RandomSlot();
            slot.OnSetSlot(listSlot[i]);
        }
    }
    public FoodSlot RandomSlot()
    {
    rerand: int n = Random.Range(0, totalSlots.Count);
        if (totalSlots[n].HasFood) goto rerand;
        return totalSlots[n];
    }

}
