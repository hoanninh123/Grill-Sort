using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FoodSlot : MonoBehaviour
{
    private Image item;
    // Start is called before the first frame update
    void Start()
    {
        item = this.transform.GetChild(0).GetComponent<Image>();
        item.gameObject.SetActive(false);
    }
    public void OnSetSlot(Sprite img)
    {
        item.gameObject.SetActive(true);
        item.sprite = img;
        item.SetNativeSize();
    }
    public bool HasFood => item.gameObject.activeInHierarchy;
}
