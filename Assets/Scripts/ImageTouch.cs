using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageTouch : MonoBehaviour
{
    private Image img;
    public List<Sprite> basicSprites = new List<Sprite>();
    public List<Sprite> basicClickSprites = new List<Sprite>();

    public const int touchAreaHeight = 1325;
    private float screenWidth;
    private float screenHeight;
    private float menuHeight;
    public RectTransform menu;

    public Sprite[] sprites;

    private static int PERMIL = 5;

    public bool isWating = false;
    public GameObject newPopup;
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    void Start()
    {
        img = GetComponent<Image>();
        menuHeight = menu.sizeDelta.y;
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        
        sprites = Resources.LoadAll<Sprite>("Collections");      
    }

    private void Update()
    {
        if (!isWating)
        {
            if (Application.isEditor)
            {
                if (Input.mousePosition.y > menuHeight)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        TouchImage(Input.mousePosition);    
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        int randIdx = Random.Range(0, basicSprites.Count);
                        img.sprite = basicSprites[randIdx];
                        UIManager.Instance.ImageChangeTween(img);

                    }
                }

            }
            else
            {
                if (Input.touchCount == 1)
                {
                    Touch touch = Input.GetTouch(0);
                    if (touch.position.y > menuHeight)
                    {
                        if (touch.phase == TouchPhase.Began)
                        {
                            TouchImage(touch.position);
                        }
                        else if (touch.phase == TouchPhase.Ended)
                        {
                            if (!isWating)
                            {
                                int randIdx = Random.Range(0, basicSprites.Count);
                                img.sprite = basicSprites[randIdx];
                                UIManager.Instance.ImageChangeTween(img);

                            }
                        }
                    }
                }
            }
        }
 
    }
    public void TouchImage(Vector2 touchPos)
    {
        int rand = Random.Range(0, 1000);
        if (rand < PERMIL)
        {
            GetSpecialPicture();
        }
        else
        {
            if (touchPos.y > touchAreaHeight)
            {
                img.sprite = basicClickSprites[((int)Direction.Up)];
            }
            else if (touchPos.y < screenHeight - touchAreaHeight)
            {
                img.sprite = basicClickSprites[((int)Direction.Down)];
            }
            else if (touchPos.x < screenWidth * 0.5f)
            {
                img.sprite = basicClickSprites[((int)Direction.Left)];
            }
            else if (touchPos.x > screenWidth * 0.5f)
            {
                img.sprite = basicClickSprites[((int)Direction.Right)];
            }
        }
        DataManager.Instance.Score++;
        DataManager.Instance.TotalScore++;
    }

    private void GetSpecialPicture()
    {
        int randIdx = Random.Range(0, sprites.Length);
        img.sprite = sprites[randIdx];
        if (!DataManager.Instance.savedUserData.Collection[randIdx])
        {
            isWating = true;
            DataManager.Instance.SetCollection(randIdx);
            UIManager.Instance.NewImgPopup();
        }
    }


}