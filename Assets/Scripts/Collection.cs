using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class Collection : MonoBehaviour
{
    public List<GameObject> collections;
    public ImageTouch imgTouch;
    private bool isCollectionMade = false;
    public GameObject collectionPrefab;
    Transform content;
    public GameObject detailPanel;
    public GameObject detailPanelBtns;
    public Text detailTitle;
    public Image detailImg;
    private Button prevBtn;
    private Button backBtn;
    private Button nextBtn;
    [SerializeField]
    private int curIndex = 0;
    private List<string> collectedTitles = new List<string>();
    private List<Sprite> collectedSprites = new List<Sprite>();

    void Start()
    {
        content = transform.GetChild(0).GetChild(0);
        //GameObject[] collectionArray = GameObject.FindGameObjectsWithTag("CollectionPanel");
        //collections = collectionArray.ToList();
        //collections.Sort(SortByName);
        prevBtn = detailPanelBtns.transform.GetChild(0).GetComponent<Button>();
        backBtn = detailPanelBtns.transform.GetChild(1).GetComponent<Button>();
        nextBtn = detailPanelBtns.transform.GetChild(2).GetComponent<Button>();

        prevBtn.onClick.AddListener(() => OnDetailBtnClicked(curIndex - 1));
        nextBtn.onClick.AddListener(() => OnDetailBtnClicked(curIndex + 1));
        gameObject.SetActive(false);
    }

    private int SortByName(GameObject go1, GameObject go2)
    {
        int go1Num = Convert.ToInt32(go1.name.Split('_')[1]);
        int go2Num = Convert.ToInt32(go2.name.Split('_')[1]);
        return go1Num.CompareTo(go2Num);
    }

    public void UncoverFoundCollections()
    {
        collectedSprites.Clear();
        collectedTitles.Clear();
        var isFoundArray = DataManager.Instance.savedUserData.Collection;
        var foundDates = DataManager.Instance.savedUserData.Dates;
        var collectCount = DataManager.Instance.savedUserData.CollectCount;
        if (!isCollectionMade)
        {
            for (int i = 0; i < DataManager.COLLECTION_COUNT; i++)
            {
                GameObject collection = Instantiate(collectionPrefab);
                collection.transform.SetParent(content, false);
                collections.Add(collection);
            }
            isCollectionMade = true;
        }
        for (int i = 0; i < collections.Count; i++)
        {
            if (isFoundArray[i])
            {                
                Transform collectionTr = collections[i].transform;
                Sprite sprite = imgTouch.sprites[i];
                string collectionName = sprite.name.Split('_')[1];
                collectedSprites.Add(sprite);
                collectedTitles.Add(collectionName);
                collectionTr.Find("Cover").gameObject.SetActive(false);
                collectionTr.Find("IconBack").GetChild(0).GetComponent<Image>().sprite = sprite;
                collectionTr.Find("Title").GetComponent<Text>().text = string.Format("[{0}]", collectionName);
                collectionTr.Find("Count").GetComponent<Text>().text = string.Format("{0}¹øÂ° Å¸°Ý¿¡ È¹µæ", collectCount[i].ToString());
                collectionTr.Find("Medal").GetChild(0).GetComponent<Text>().text = foundDates[i];
                Button collectionBtn = collectionTr.GetComponent<Button>();
                collectionBtn.interactable = true;
                collectionBtn.onClick.AddListener(() => OnCollectionBtnClick(collectionName));
            }
        }
        gameObject.SetActive(true);
    }

    private void OnCollectionBtnClick(string title)
    {
        curIndex = collectedTitles.IndexOf(title);
        detailTitle.text = collectedTitles[curIndex];
        detailImg.sprite = collectedSprites[curIndex];
        detailPanel.SetActive(true);
    }
    private void OnDetailBtnClicked(int nextIndex)
    {
        if(nextIndex < 0 || nextIndex >= collectedTitles.Count)
        {
            //Debug.Log(nextIndex);
            return;
        }

        detailTitle.text = collectedTitles[nextIndex];
        detailImg.sprite = collectedSprites[nextIndex];
        curIndex = nextIndex;
    }
}
