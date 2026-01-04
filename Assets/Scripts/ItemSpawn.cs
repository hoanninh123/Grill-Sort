using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrayItem : MonoBehaviour
{
    List<Image> foodList ;
    // Start is called before the first frame update
    void Start()
    {

        foodList = Utils.GetListFromChild<Image>(this.transform);
        for(int i = 0; i < foodList.Count; i++)
        {
            foodList[i].gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SpawnFoodItem(List<Sprite> item)
    {
        if(foodList.Count >= item.Count)
        {
            for(int i = 0; i < item.Count; i++)
            {
                Image slot = RandomSlot();
                slot.gameObject.SetActive(true);
                slot.sprite = item[i];
                slot.SetNativeSize();

            }
        }
    }
    public Image RandomSlot()
    {
    rerand: int n = Random.Range(0, foodList.Count);
        if (foodList[n].gameObject.activeInHierarchy) goto rerand;
        return foodList[n];
    }
}
