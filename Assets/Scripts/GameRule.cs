using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRule : MonoBehaviour
{
    [Header("Game Rule Settings")]
    [SerializeField] private Transform FoodList;
    [SerializeField] private Transform PlayerGeometry;
    [SerializeField] private int maxRenderedFood = 1;
    [Header("Mesh levels")]
    [SerializeField] private List<GameObject> levelObjects = new List<GameObject>();

    private int lvl = 1;
    private List<GameObject> allFoodItems = new List<GameObject>();

    void Start()
    {
        foreach(Transform child in FoodList)
        {
            child.gameObject.SetActive(false);
            allFoodItems.Add(child.gameObject);
        }
        InvokeRepeating("SpawnFood", 0f, 15f); // Call SpawnFood every 15 seconds (starting immediately)
        UpdatePlayerGeometry();
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
        lvl = Mathf.Clamp(lvl + 1, 0, levelObjects.Count);
        UpdatePlayerGeometry();
        Debug.Log("lvl: " + lvl);
    }

    public void RemoveLvl()
    {
        lvl = Mathf.Clamp(lvl - 1, 0, levelObjects.Count);
        UpdatePlayerGeometry();
        Debug.Log("lvl: " + lvl);
    }

    void UpdatePlayerGeometry()
    {
        // Desactivate every level object
        foreach(GameObject level in levelObjects)
        {
            level.SetActive(false);
        }

        // Activate the current level object
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