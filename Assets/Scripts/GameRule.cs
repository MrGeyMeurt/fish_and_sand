using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;
using System;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ObjectPool
{
    public string poolName;
    public Transform parentTransform;
    public int maxRendered;
    [HideInInspector] public List<GameObject> items = new List<GameObject>();
}

public class GameRule : MonoBehaviour
{
    [Header("Game Rule Settings")]
    [SerializeField] private StarterAssetsInputs _input;
    [SerializeField] private Transform PlayerGeometry;
    [SerializeField] private GameObject SecondEnemy;
    [SerializeField] private GameObject ThirdEnemy;
    [SerializeField] private Transform FoodPool;
    [SerializeField] private Transform CameraPool;
    [SerializeField] private GameObject Exit;
    [SerializeField] private int maxRenderedFood = 1;
    [SerializeField] private float WaitingToStartTimer = 2f;
    [SerializeField] private float CountdownToStartTimer = 5f;
    [SerializeField] private float GamePlayingTimer = 300f;
    [SerializeField] private float GameOverTimer = 20f;

    [Header("Game Over Screen")]
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private float gameOverDelay = 5f;
    
    [Header("UI")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject playingHUD;
    [SerializeField] private GameObject goalHUD;
    [SerializeField] private GameObject gameOverHUD;
    [SerializeField] private GameObject countDownText;
    [SerializeField] private TMP_Text levelMessageText;

    [Header("Mesh levels")]
    [SerializeField] private List<GameObject> levelObjects = new List<GameObject>();

    [Header("Map Layout")]
    // [SerializeField] private Transform ObjectList;
    [SerializeField] private List<ObjectPool> objectPools = new List<ObjectPool>();
    // [SerializeField] private int maxRenderedObjects = 5;

    private int lvl = 1;
    private List<GameObject> allFoodItems = new List<GameObject>();
    private List<GameObject> allObjectItems = new List<GameObject>();
    private Dictionary<int, Camera> priorityCameras;
    public static GameRule Instance { get; private set; }
    public event EventHandler OnStateChanged;
    private enum State { 
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
        GamePaused
    }
    private State state;
    private CharacterController _controller;
    private ThirdPersonController _playerController;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        {
            _input = player.GetComponent<StarterAssetsInputs>();
            _playerController = player.GetComponent<ThirdPersonController>();
        }
        else
        {
            Debug.LogError("Player introuvable dans la scène !");
        }

        state = State.WaitingToStart;
    }

    void Start()
    {
        Exit.gameObject.SetActive(false);
        countDownText.SetActive(true);
        playingHUD.SetActive(true);
        gameOverHUD.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        
        foreach(Transform child in FoodPool)
        {
            child.gameObject.SetActive(false);
            allFoodItems.Add(child.gameObject);
        }

        foreach(ObjectPool pool in objectPools)
        {
            foreach(Transform child in pool.parentTransform)
            {
                child.gameObject.SetActive(false);
                pool.items.Add(child.gameObject);
            }
        }

        priorityCameras = new Dictionary<int, Camera>();
        int childIndex = 0;
        foreach (Transform child in CameraPool)
        {
            var cam = child.GetComponent<Camera>();
            if (cam != null)
            {
                priorityCameras.Add(childIndex, cam);
            }
            childIndex++;
        }

        InvokeRepeating("SpawnFood", 0f, 15f); // Call SpawnFood every 15 seconds (starting immediately)
        UpdateLevelText();
        UpdatePlayerGeometry();
        MapLayout();
    }

    private void Update()
    {
        switch(state)
        {
            case State.WaitingToStart:
                WaitingToStartTimer -= Time.deltaTime;
                if(WaitingToStartTimer <= 0f)
                {
                    state = State.CountdownToStart;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                _input.pause = false;
                break;
            case State.CountdownToStart:
                CountdownToStartTimer -= Time.deltaTime;
                if(CountdownToStartTimer <= 0f)
                {
                    state = State.GamePlaying;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                _input.pause = false;
                break;
            case State.GamePlaying:
                GamePlayingTimer -= Time.deltaTime;
                if(_input.pause)
                {
                    _input.pause = false;
                    GamePause();
                }
                if(GamePlayingTimer <= 0f)
                {
                    SetGameOver();
                }
                break;
            case State.GameOver:
                GameOverTimer -= Time.deltaTime;
                if(_input.pause)
                {
                    _input.pause = false;
                    GamePause();
                }

                playingHUD.SetActive(false);

                int[] timeThresholds = { 15, 10, 5 };
                int[] priorities = { 2, 3, 4 };

                for (int i = 0; i < timeThresholds.Length; i++)
                {
                    if (GameOverTimer <= timeThresholds[i])
                    {
                        int childIndex = 0;
                        foreach (Transform child in CameraPool)
                        {
                            if (childIndex == i)
                            {
                                var camera = child.GetComponent<Camera>();
                                if (camera != null)
                                {
                                    camera.depth = priorities[i];
                                }
                            }
                            childIndex++;
                        }
                    }
                }

                if(GameOverTimer <= 0f)
                {
                    SceneManager.LoadScene("MainMenu");
                }
                break;
            case State.GamePaused:
                if(_input.pause)
                {
                    _input.pause = false;
                    GamePause();
                }
                break;
        }
    }

    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }

    public bool IsGamePaused()
    {
        return state == State.GamePaused;
    }

    public bool IsGameOver()
    {
        return state == State.GameOver;
    }

    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer()
    {
        return CountdownToStartTimer;
    }

    public void SetGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        state = State.GameOver;
        goalHUD.SetActive(false);
        PlayerStats.Instance.StopCountingTime();

        if(_playerController != null)
        {
            _playerController.TriggerDeath();
        }
        
        yield return new WaitForSeconds(gameOverDelay);
        gameOverUI.ShowGameOver();
    }

    public void GamePause()
    {
        if(state != State.GamePaused)
        {
            state = State.GamePaused;
            Time.timeScale = 0f;
            pauseMenu.SetActive(true);
        }
        else if(state == State.GamePaused)
        {
            state = State.GamePlaying;
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    void SpawnFood()
    {
        if(lvl >= 3) return;

        if(CountActiveFood() >= maxRenderedFood) return;

        // Filter all the inactive food items
        List<GameObject> availableFood = allFoodItems.FindAll(food => !food.activeSelf);

        if(availableFood.Count > 0)
        {
            // Choose a random index from the list and activate it
            int randomIndex = UnityEngine.Random.Range(0, availableFood.Count);
            availableFood[randomIndex].SetActive(true);
        }
    }

        public int CountActiveFood()
    {
        int count = 0;
        foreach(GameObject food in allFoodItems)
        {
            if(food.activeSelf) count++;
        }
        return count;
    }

    void MapLayout()
    {
        foreach(ObjectPool pool in objectPools)
        {
            foreach(GameObject item in pool.items)
            {
                item.SetActive(false);
            }

            List<GameObject> availableItems = new List<GameObject>(pool.items);
            for(int i = 0; i < pool.maxRendered; i++)
            {
                if(availableItems.Count == 0) break;
                
                int randomIndex = UnityEngine.Random.Range(0, availableItems.Count);
                availableItems[randomIndex].SetActive(true);
                availableItems.RemoveAt(randomIndex);
            }
        }
    }

    public void AddLvl()
    {
        lvl = Mathf.Clamp(lvl + 1, 0, levelObjects.Count);
        UpdateLevelText();
        UpdatePlayerGeometry();

        PlayerStats.Instance.AddScore(100);

        if(lvl == 2)
        {
            SecondEnemy.SetActive(true);
        }
    
        if(lvl == 3)
        {
            Exit.gameObject.SetActive(true);
            ThirdEnemy.SetActive(true);

            foreach(GameObject food in allFoodItems)
            {
                food.SetActive(false);
            }
        }
    }

    public void RemoveLvl()
    {
        lvl = Mathf.Clamp(lvl - 1, 0, levelObjects.Count);
        UpdatePlayerGeometry();
        UpdateLevelText();

        if (lvl == 0)
        {
            PlayerStats.Instance.isGameLoose = true;
            SetGameOver();
        }
    }

    private void UpdateLevelText()
    {
        if (levelMessageText == null) return;

        switch(lvl)
        {
            case 0:
                levelMessageText.text = string.Empty;
                break;
            case 1:
                levelMessageText.text = "Eat pizza to evolve";
                break;
            case 2:
                levelMessageText.text = "One pizza left to eat";
                break;
            case 3:
                levelMessageText.text = "Find the big container to get out";
                break;
            default:
                levelMessageText.text = string.Empty;
                break;
        }
    }

    void UpdatePlayerGeometry()
    {
        // Deactivate every level mesh except level 0
        foreach (GameObject level in levelObjects)
        {
            level.SetActive(false);
        }

        // Activate the current level mesh
        if (lvl > 0 && lvl <= levelObjects.Count)
        {
            levelObjects[lvl - 1].SetActive(true);
        }

        // Ensure level 0 is always active
        if (lvl == 0)
        {
            levelObjects[0].SetActive(true);
        }

        if (lvl != 3)
        {
            Exit.gameObject.SetActive(false);
        }
    }
}