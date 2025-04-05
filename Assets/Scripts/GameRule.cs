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

    [Header("Mesh levels")]
    [SerializeField] private List<GameObject> levelObjects = new List<GameObject>();

    [Header("Map Layout")]
    // [SerializeField] private Transform ObjectList;
    [SerializeField] private List<ObjectPool> objectPools = new List<ObjectPool>();
    // [SerializeField] private int maxRenderedObjects = 5;

    private int lvl = 1;
    private List<GameObject> allFoodItems = new List<GameObject>();
    private List<GameObject> allObjectItems = new List<GameObject>();
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
        playingHUD.SetActive(true);
        
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

        InvokeRepeating("SpawnFood", 0f, 15f); // Call SpawnFood every 15 seconds (starting immediately)
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
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
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

        bool usingGamepad = Gamepad.current != null;
        if(usingGamepad)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
    }

    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
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
        PlayerStats.Instance.StopCountingTime();
        
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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = (Gamepad.current == null);
        }
        else if(state == State.GamePaused)
        {
            state = State.GamePlaying;
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void SpawnFood()
    {
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
        UpdatePlayerGeometry();

        PlayerStats.Instance.AddScore(100);
    
        if(lvl == 3)
        {
            Exit.gameObject.SetActive(true);
        }
    }

    public void RemoveLvl()
    {
        lvl = Mathf.Clamp(lvl - 1, 0, levelObjects.Count);
        UpdatePlayerGeometry();

        if (lvl == 0)
        {
            PlayerStats.Instance.isGameLoose = true;
            SetGameOver();
        }
    }

    void UpdatePlayerGeometry()
    {
        // Desactivate every level mesh
        foreach(GameObject level in levelObjects)
        {
            level.SetActive(false);
        }

        // Activate the current level mesh
        if(lvl > 0 && lvl <= levelObjects.Count)
        {
            levelObjects[lvl - 1].SetActive(true);
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
}