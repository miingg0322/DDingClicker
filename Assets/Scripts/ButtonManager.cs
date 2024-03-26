using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    [Header("Menu")]
    public Button menuHomeBtn;
    public Button menuCollectionBtn;
    public Button menuRankingBtn;

    [Header("PopupCloseBtns")]
    public Button newImgCloseBtn;
    public Button registerCloseBtn;
    public Button cannotRegisterCloseBtn;

    [Header("Intro")]
    public Button startGameBtn;
    public Button collectionBtn;
    public Button rankingBtn;
    public Button exitGameBtn;
    void Start()
    {
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            startGameBtn.onClick.AddListener(UIManager.Instance.OnGameStartBtnClicked);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
