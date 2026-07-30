using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using DG.Tweening;

public class GrillStation : MonoBehaviour
{
    [SerializeField] private Transform _trayContainer;
    [SerializeField] private Transform _slotContainer;


    private List<Trayitem> _totalTrays;
    private List<FoodSlot> _totalSlot;

    private Stack<Trayitem> _stackTrays = new Stack<Trayitem>();

    public List<FoodSlot> TotalSlot => _totalSlot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _totalTrays = Utils.GetListInChild<Trayitem>(_trayContainer);
        _totalSlot = Utils.GetListInChild<FoodSlot>(_slotContainer);
    }

    public void OnInitGrill(int totalTray, List<Sprite> listFood)
    {
        _stackTrays.Clear();

        // xu ly set gia tri cho bep truoc
        int maxSlots = Mathf.Min(_totalSlot.Count, listFood.Count);
        int foodCount = maxSlots > 0 ? Random.Range(1, maxSlots + 1) : 0;
        List<Sprite> list = listFood;
        List<Sprite> listSlot = Utils.TakeAndRemoveRandom<Sprite>(list, foodCount);

        for (int i = 0; i < listSlot.Count; i++)
        {
            FoodSlot slot = this.RandomSlot();
            if (slot != null)
            {
                slot.OnSetSlot(listSlot[i]);
            }
        }

        // xu ly dia
        List<List<Sprite>> remainFood = new List<List<Sprite>>();

        int targetTrayCount = Mathf.Max(1, totalTray - 1);
        for (int i = 0; i < targetTrayCount; i++)
        {
            if (listFood.Count > 0)
            {
                remainFood.Add(new List<Sprite>());
                remainFood[i].Add(listFood[0]);
                listFood.RemoveAt(0);
            }
        }

        // dam bao remainFood khong bi rong neu con listFood
        if (listFood.Count > 0 && remainFood.Count == 0)
        {
            remainFood.Add(new List<Sprite>());
        }

        while (listFood.Count > 0)
        {
            var validTrays = remainFood.Where(t => t.Count < 4).ToList();
            if (validTrays.Count == 0)
            {
                if (remainFood.Count < _totalTrays.Count)
                {
                    var newTray = new List<Sprite>();
                    remainFood.Add(newTray);
                    validTrays.Add(newTray);
                }
                else
                {
                    var leastTray = remainFood.OrderBy(t => t.Count).First();
                    leastTray.Add(listFood[0]);
                    listFood.RemoveAt(0);
                    continue;
                }
            }

            int rans = Random.Range(0, validTrays.Count);
            validTrays[rans].Add(listFood[0]);
            listFood.RemoveAt(0);
        }

        for (int i = 0; i < _totalTrays.Count; i++)
        {
            bool active = i < remainFood.Count && remainFood[i].Count > 0;
            _totalTrays[i].gameObject.SetActive(active);

            if (active)
            {
                _totalTrays[i].OnSetFood(remainFood[i]);
                Trayitem item = _totalTrays[i];
                _stackTrays.Push(item);
            }
        }
    }

    private FoodSlot RandomSlot()
    {
        if (_totalSlot == null || _totalSlot.Count == 0) return null;
        if (!_totalSlot.Any(s => !s.HasFood)) return null;

    reRand: int n = Random.Range(0, _totalSlot.Count);
        if (_totalSlot[n].HasFood) goto reRand;

        return _totalSlot[n];
    }

    public FoodSlot GetSlotNull()
    {
        FoodSlot tmp = null;

        for (int i = 0; i < _totalSlot.Count; i++)
        {
            if (!_totalSlot[i].HasFood)
            {
                if (tmp == null)
                {
                    tmp = _totalSlot[i];
                }
                else
                {
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    float x1 = Mathf.Abs(mousePos.x - tmp.transform.position.x);
                    float x2 = Mathf.Abs(mousePos.x - _totalSlot[i].transform.position.x);

                    if (x2 < x1)
                        tmp = _totalSlot[i];
                }
            }
        }

        return tmp;
    }

    private bool HasGrillEmpty()
    {
        for (int i = 0; i < _totalSlot.Count; i++)
        {
            if (_totalSlot[i].HasFood)
                return false;
        }

        return true;
    }

    public void OnCheckMerge()
    {
        if (this.GetSlotNull() == null) // kiem tra xem so luong slot du 3 item chua, neu chua du thi no == null
        {
            if (this.CanMerge())
            {
                Debug.Log("Complete Grill");

                StartCoroutine(IEMerge());

                this.OnPrepareTray(false);
                GameManager.Instance?.OnMinusFood();
            }
        }

        IEnumerator IEMerge()
        {
            for (int i = 0; i < _totalSlot.Count; i++)
            {
                _totalSlot[i].OnFadeOut();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void OnCheckPrepareTray()
    {
        if (this.HasGrillEmpty())
        {
            this.OnPrepareTray(true);
        }
    }

    private void OnPrepareTray(bool isNow)
    {
        StartCoroutine(IEPrepare());

        IEnumerator IEPrepare()
        {
            if (!isNow)
                yield return new WaitForSeconds(0.95f);

            if (_stackTrays.Count > 0)
            {
                Trayitem item = _stackTrays.Pop();

                for (int i = 0; i < item.FoodList.Count; i++)
                {
                    Image img = item.FoodList[i];
                    if (img.gameObject.activeInHierarchy)
                    {
                        _totalSlot[i].OnPrepareItem(img);
                        img.gameObject.SetActive(false);
                        yield return new WaitForSeconds(0.1f);
                    }
                }

                CanvasGroup canvas = item.GetComponent<CanvasGroup>();
                if (canvas == null)
                {
                    canvas = item.gameObject.AddComponent<CanvasGroup>();
                }

                canvas.DOFade(0f, 0.5f).OnComplete(() =>
                {
                    item.gameObject.SetActive(false);
                    canvas.alpha = 1f;
                });

            }
        }
    }

    private bool CanMerge()
    {
        string name = _totalSlot[0].GetSpriteFood.name;

        for (int i = 1; i < _totalSlot.Count; i++)
        {
            if (_totalSlot[i].GetSpriteFood.name != name)
                return false;
        }

        return true;
    }
}