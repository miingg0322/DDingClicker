using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Extensions;
using System.Threading.Tasks;
using System;

[FirestoreData]
public class User
{
    [FirestoreProperty]
    public string NickName { get; set; }
    [FirestoreProperty]
    public string Pw { get; set; }
    [FirestoreProperty]
    public int Count { get; set; }

    [FirestoreProperty]
    public bool[] Collection { get; set; }
    [FirestoreProperty]
    public string[] Dates { get; set; }
    [FirestoreProperty]
    public int[] CollectCount { get; set; }
}
public class DataManager : MonoBehaviour
{
    private int totalScore;
    public int TotalScore
    {
        get { return totalScore; }
        set
        {
            totalScore = value;
            totalScoreText.text = totalScore.ToString();
        }
    }
    private int score;
    public int Score
    {
        get { return score; }
        set
        {
            score = value;
            scoreText.text = score.ToString();
            if (score - lastUpdate > UPDATE_CYCLE)
            {
                //Debug.Log("Update");
                UpdateScore();
            }
        }
    }
    private const int UPDATE_CYCLE = 100;
    private int lastUpdate;
    public int LastUpdate
    {
        get { return score; }
        set
        {
            lastUpdate = value;
        }
    }

    public Text scoreText;
    public Text totalScoreText;

    public GameObject rankerPrefab;
    private bool isRankingMade = false;
    public Sprite topRankerSprite;

    private static DataManager instance;
    public static DataManager Instance
    {
        get { return instance; }
        private set { instance = value; }
    }
    public const int COLLECTION_COUNT = 50;
    public const int RANKIKNG_LIMIT = 100;

    private FirebaseFirestore db;
    public User savedUserData;

    [SerializeField]
    private UserRegisterController regController;
    private Collection collection;

    private void Awake()
    {
        if(instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(Instance);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void Start()
    {

    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) 
        {
            db = FirebaseFirestore.DefaultInstance;
        }
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            isRankingMade = false;
            collection = GameObject.FindGameObjectWithTag("Collection").GetComponent<Collection>();
            Transform scoreTr = GameObject.FindGameObjectWithTag("Score").transform;
            totalScoreText = scoreTr.GetChild(0).GetComponent<Text>();
            scoreText = scoreTr.GetChild(1).GetComponent<Text>();
            GetScore();
        }
    }
    public async Task<bool> RegisterNewUser(string name, string pw)
    {
        User user = new User
        {
            NickName = name,
            Pw = pw,
            Count = 0,
            Collection = new bool[COLLECTION_COUNT],
            Dates = new string[COLLECTION_COUNT],
            CollectCount = new int[COLLECTION_COUNT]
        };

        bool result = await db.Collection("Users").Document(name).SetAsync(user).ContinueWithOnMainThread(task =>
        {
            return task.IsCompleted;
        });
        return result;
    }
    public async Task<bool> CheckUserNameExists(string name)
    {
        DocumentReference docRef = db.Collection("Users").Document(name);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists;
    }
    public async Task<bool> AutoLogin()
    {
        string nickName = PlayerPrefs.GetString("NickName");
        string pw = PlayerPrefs.GetString("Pw");
        
        DocumentReference docRef = db.Collection("Users").Document(nickName);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            savedUserData =  snapshot.ConvertTo<User>();
            if (savedUserData.Pw.Equals(pw))
            {
                return true;
            }
        }
        return false;
    }

    public void Login(string inputName, string inputPw)
    {
        DocumentReference documentReference = db.Collection("Users").Document(inputName);
        documentReference.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                User user = snapshot.ConvertTo<User>();
                string pw = Encryptor.SHA256Encryt(inputPw);
                if (user.Pw.Equals(pw))
                {
                    savedUserData = user;
                    PlayerPrefs.SetString("NickName", inputName);
                    PlayerPrefs.SetString("Pw", pw);
                    regController.LoginSuccess();
                }
                else
                {
                    regController.LoginFailed();
                }

            }
            else
            {
                regController.LoginFailed();
            }
        });
    }

    public void GetUserCollection()
    {
        string nickName = savedUserData.NickName;
        DocumentReference docRef = db.Collection("Users").Document(nickName);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            savedUserData = snapshot.ConvertTo<User>();
            if (task.IsCompleted)
            {
                collection.UncoverFoundCollections();
            }
        });
    }
    public void GetRanking()
    {
        Query rankingQuery = db.Collection("Users").OrderByDescending("Count").Limit(RANKIKNG_LIMIT);
        Color firstColor = new Color(1f, 220f/255f, 0, 1f);
        Color secondColor = new Color(188f / 255f, 188f / 255f, 188f / 255f, 1f);
        Color thirdColor = new Color(1f, 176f / 255f, 107f / 255f, 1f);

        rankingQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot rankingSnapshot = task.Result;

            if (isRankingMade)
            {
                GameObject[] rankers;
                Transform rankersTr = UIManager.Instance.ranking.transform.GetChild(1).GetChild(0);
                rankers = new GameObject[rankersTr.childCount];
                for (int i = 0; i < rankersTr.childCount; i++)
                {
                    rankers[i] = rankersTr.GetChild(i).gameObject;
                }

                for (int i = 0; i < rankingSnapshot.Count; i++)
                {
                    User user = rankingSnapshot[i].ConvertTo<User>();                   
                    if(i < rankers.Length)
                    {
                        InsertRankerData(rankers[i].transform, user);
                    }
                    else
                    {
                        GameObject ranker = Instantiate(rankerPrefab);
                        Transform rankerTr = ranker.transform;
                        rankerTr.SetParent(rankersTr, false);
                        rankerTr.localScale = transform.root.localScale;
                        rankerTr.Find("Icon").GetComponent<Image>().transform.GetComponentInChildren<Text>().text = (i + 1).ToString();
                        InsertRankerData(rankerTr, user);
                    }
                }
            }
            else
            {

                for (int i = 0; i < rankingSnapshot.Count; i++)
                {
                    User user = rankingSnapshot[i].ConvertTo<User>();
                    GameObject ranker = Instantiate(rankerPrefab);
                    Transform rankerTr = ranker.transform;
                    rankerTr.SetParent(UIManager.Instance.ranking.transform.GetChild(1).GetChild(0));
                    rankerTr.localScale = transform.root.localScale;

                    Image icon = rankerTr.Find("Icon").GetComponent<Image>();
                    if (i < 3)
                    {                       
                        icon.sprite = topRankerSprite;
                        switch (i)
                        {
                            case 0:
                                icon.color = firstColor;
                                break;
                            case 1:
                                icon.color = secondColor;
                                break;
                            case 2:
                                icon.color = thirdColor;
                                break;
                            default:
                                break;
                        }
                    }
                    rankerTr.Find("Icon").GetComponent<Image>().transform.GetComponentInChildren<Text>().text = (i + 1).ToString();
                    InsertRankerData(rankerTr, user);
                }
            }

            if (task.IsCompleted)
            {
                UIManager.Instance.ActiveRanking();
                isRankingMade = true;
            }
        });
    }
    private void InsertRankerData(Transform rankerTr, User user)
    {
        rankerTr.Find("Name").GetComponent<Text>().text = user.NickName;
        rankerTr.Find("Count").GetComponent<Text>().text = $"{user.Count}회";
        int collectionCount = 0;
        foreach (bool isCollected in user.Collection)
        {
            if (isCollected)
                collectionCount++;
        }
        rankerTr.Find("Collection").GetComponent<Text>().text = $"{collectionCount}개 수집";
    }
    public void SetCollection(int index)
    {
        DateTime krNow = DateTime.Now;
        krNow = krNow.AddHours(9);
        string date = $"{krNow.Year}/{krNow.Month}/{krNow.Day}";
        DocumentReference docRef = db.Collection("Users").Document(savedUserData.NickName);
        bool[] collectionToUpdate = savedUserData.Collection;
        string[] datesToUpdate = savedUserData.Dates;
        int[] countsToUpdate = savedUserData.CollectCount;

        collectionToUpdate[index] = true;
        datesToUpdate[index] = date;
        countsToUpdate[index] = TotalScore;


        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "Count", TotalScore},
            {"Collection",  collectionToUpdate},
            {"Dates",  datesToUpdate},
            {"CollectCount", countsToUpdate }
        };
        docRef.UpdateAsync(updates).ContinueWithOnMainThread(task=>
        {
            if (task.IsCompleted)
            {
                LastUpdate = Score;
            }
        });
    }

    public void GetScore()
    {
        DocumentReference docRef = db.Collection("Users").Document(savedUserData.NickName);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            savedUserData = snapshot.ConvertTo<User>();
            if (task.IsCompleted)
            {
                TotalScore = savedUserData.Count;
                Score = 0;
            }
        });
    }

    public async Task<bool> SetScore()
    {
        DocumentReference docRef = db.Collection("Users").Document(savedUserData.NickName);
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            {"Count",  TotalScore}
        };
        bool result = await docRef.UpdateAsync(updates).ContinueWithOnMainThread(task=>
        {
            return task.IsCompleted;
        });
        if (result)
        {
            lastUpdate = Score;
            //Debug.Log("SetScore");
        }
        return result;
    }

    private async void UpdateScore()
    {
        await SetScore();
    }
}
