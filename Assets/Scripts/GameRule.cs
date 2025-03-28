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
        InvokeRepeating("SpawnFood", 0f, 10f); // Call SpawnFood every 10 seconds (starting immediately)
    }

    void SpawnFood()
    {
        if (CountActiveFood() < maxRenderedFood)
        {
           
        }
    }

    public void AddLvl()
    {
        lvl++;
        Debug.Log("lvl: " + lvl);
    }

    public void RemoveLvl()
    {
        lvl--;
        Debug.Log("lvl: " + lvl);
    }

    public int CountActiveFood()
    {
        Debug.Log("Active food count: " + GameObject.FindGameObjectsWithTag("Food").Length);
        return GameObject.FindGameObjectsWithTag("Food").Length;
    }
}