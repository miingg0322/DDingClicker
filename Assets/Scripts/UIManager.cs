using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get { return instance; }
        private set { instance = value; }
    }

    [Header("Intro")]
    public Button startGameBtn;
    public Button rankingBtn;
    public Button exitGameBtn;
    public Button rankingCloseBtn;
    public UserRegisterController regController;



    [Header("Main")]
    private Transform menu;
    public Button menuHomeBtn;
    public Button menuCollectionBtn;
    public Button menuRankingBtn;

    public GameObject ranking;
    private ImageTouch mainImage;
    private GameObject collection;

    [Header("PopupCloseBtns")]
    public GameObject newImgPopup;
    public Button newImgCloseBtn;
    private float tweenMoveDist = 300f;

    public GameObject exitPopup;
    private Button mainExitGameBtn;
    private Button exitCloseBtn;

    void Awake()
    {
        if (instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void Start()
    {
        DOTween.Init();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            startGameBtn.onClick.AddListener(OnGameStartBtnClicked);
            rankingBtn.onClick.AddListener(()=>OnRankingBtnClicked(ranking));
            exitGameBtn.onClick.AddListener(OnGameExitConfirmBtnClicked);
            
        }

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            ranking = GameObject.FindGameObjectWithTag("Ranking");
            collection = GameObject.FindGameObjectWithTag("Collection");
            exitPopup = GameObject.FindGameObjectWithTag("Exit");
            mainExitGameBtn = exitPopup.transform.GetChild(0).GetComponent<Button>();
            mainExitGameBtn.onClick.AddListener(() => OnGameExitConfirmBtnClicked());
            exitCloseBtn = exitPopup.transform.GetChild(1).GetComponent<Button>();
            exitCloseBtn.onClick.AddListener(() => OnCancelBtnClicked(exitPopup));
            //mainExitGameBtn.onClick.AddListener(() => OnCancelBtnClicked(exitPopup));

            menu = GameObject.FindGameObjectWithTag("Menu").transform;
            menuHomeBtn = menu.GetChild(0).GetComponent<Button>();
            menuHomeBtn.onClick.AddListener(()=>OnGameExitBtnClicked(exitPopup));
            menuCollectionBtn = menu.GetChild(1).GetComponent<Button>();
            menuCollectionBtn.onClick.AddListener(() => OnCollectionBtnClicked());
            menuRankingBtn = menu.GetChild(2).GetComponent<Button>();
            menuRankingBtn.onClick.AddListener(() => OnRankingBtnClicked(ranking));


            newImgPopup = GameObject.FindGameObjectWithTag("Popup");
            newImgCloseBtn = newImgPopup.transform.GetChild(1).GetComponent<Button>();
            newImgCloseBtn.onClick.AddListener(() => OnNewPopupCloseBtnClick());
            newImgCloseBtn.gameObject.SetActive(false);
            mainImage = GameObject.FindGameObjectWithTag("MainImage").GetComponent<ImageTouch>();

            ranking.SetActive(false);
            exitPopup.SetActive(false);
            newImgPopup.SetActive(false);
        }
    }
    public async void OnGameStartBtnClicked()
    {
        if (PlayerPrefs.HasKey("NickName"))
        {
            bool isLoginSuccess = await DataManager.Instance.AutoLogin();
            if (isLoginSuccess)
            {
                SceneManager.LoadScene(1);
            }
        }
        else
        {
            regController.registerPanel.SetActive(true);
        }
    }

    public void OnGameExitBtnClicked(GameObject exitPopup)
    {
        mainImage.isWating = true;
        exitPopup.SetActive(true);
    }

    public void OnCancelBtnClicked(GameObject popup)
    {
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            mainImage.isWating = false;
        }
        popup.SetActive(false);
    }
    public void OnMenuBackBtnClicked(GameObject popup)
    {
        popup.SetActive(false);
        menuCollectionBtn.interactable = true;
        menuRankingBtn.interactable = true;
        mainImage.isWating = false;
        menuHomeBtn.onClick.RemoveAllListeners();
        menuHomeBtn.onClick.AddListener(()=>OnGameExitBtnClicked(exitPopup));
    }
    public async void OnGameExitConfirmBtnClicked()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Application.Quit();
        }
        else
        {
            bool saveDone = await DataManager.Instance.SetScore();
            if (saveDone)
            {
                Application.Quit();
            }
        }

    }

    public void OnCollectionBtnClicked()
    {
        ranking.SetActive(false);
        collection.SetActive(true);
        menuCollectionBtn.interactable = false;
        menuRankingBtn.interactable = true;
        mainImage.isWating = true;
        DataManager.Instance.GetUserCollection();
        menuHomeBtn.onClick.RemoveAllListeners();
        menuHomeBtn.onClick.AddListener(() => OnMenuBackBtnClicked(collection));
    }

    public void OnRankingBtnClicked(GameObject ranking)
    {
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            collection.SetActive(false);
            menuCollectionBtn.interactable = true;
            menuRankingBtn.interactable = false;
            mainImage.isWating = true;
            menuHomeBtn.onClick.RemoveAllListeners();
            menuHomeBtn.onClick.AddListener(() => OnMenuBackBtnClicked(ranking));
        }

        DataManager.Instance.GetRanking();

    }

    public void NewImgPopup()
    {
        Image img = mainImage.GetComponent<Image>();
        img.fillAmount = 0;
        newImgPopup.SetActive(true);
        GameObject title = newImgPopup.transform.GetChild(0).gameObject;
        Vector2 originPos = title.transform.position;
        GameObject btn = newImgPopup.transform.GetChild(1).gameObject;
        Sequence seq = DOTween.Sequence();
        seq.Append(title.GetComponent<RectTransform>().DOMoveY(originPos.y + tweenMoveDist, 1f));
        seq.Append(mainImage.GetComponent<Image>().DOFillAmount(1f, 1.5f));
        seq.OnComplete(() => btn.SetActive(true));
        seq.Play();
    }
    public void OnNewPopupCloseBtnClick()
    {
        newImgCloseBtn.gameObject.SetActive(false);
        Transform titleTr = newImgPopup.transform.GetChild(0).transform;
        titleTr.position = new Vector2(titleTr.position.x, titleTr.position.y - tweenMoveDist);
        newImgPopup.SetActive(false);
        mainImage.isWating = false;
    }

    public void ActiveRanking()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            rankingCloseBtn.gameObject.SetActive(true);
        }
        ranking.SetActive(true);
    }

    public void ImageChangeTween(Image img)
    {
        Transform tr = img.transform;
        tr.localScale = Vector2.one * 1.2f;
        tr.DOScale(Vector2.one, 0.2f);
    }
}
