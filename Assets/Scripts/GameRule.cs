using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRule : MonoBehaviour
{
    [SerializeField] private Transform FoodList;
    [SerializeField] private int maxRenderedFood = 1;

    private int lvl = 0;
    private List<GameObject> allFoodItems = new List<GameObject>();

    void Start()
    {
        foreach(Transform child in FoodList)
        {
            child.gameObject.SetActive(false);
            allFoodItems.Add(child.gameObject);
        }
        InvokeRepeating("SpawnFood", 0f, 15f); // Call SpawnFood every 15 seconds (starting immediately)
    }

    void SpawnFood()
    {
        if(CountActiveFood() >= maxRenderedFood) return;

        // Filter all the inactive food items
        List<GameObject> availableFood = allFoodItems.FindAll(food => !food.activeSelf);

        if(availableFood.Count > 0)
        {
            // Choose a random index from the list and activate it
            int randomIndex = Random.Range(0, availableFood.Count);
            availableFood[randomIndex].SetActive(true);
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
        int count = 0;
        foreach(GameObject food in allFoodItems)
        {
            if(food.activeSelf) count++;
        }
        return count;
    }
}