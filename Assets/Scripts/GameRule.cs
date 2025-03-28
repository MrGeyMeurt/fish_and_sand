using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRule : MonoBehaviour
{
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private int maxRenderedFood = 2;

    private int lvl = 0;

    void Start()
    {
        InvokeRepeating("SpawnFood", 0f, 2f); // call SpawnFood every 2 seconds (starting immediately)
    }

    void SpawnFood()
    {
        if (CountActiveFood() < maxRenderedFood)
        {
            
        }
    }

    int CountActiveFood()
    {
        Debug.Log("Active food count: " + GameObject.FindGameObjectsWithTag("Food").Length);
        return GameObject.FindGameObjectsWithTag("Food").Length;
    }

    public void AddLvl()
    {
        lvl++;
        Debug.Log("lvl: " + lvl);
    }
}