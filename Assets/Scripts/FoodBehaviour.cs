using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBehavior : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<GameRule>().AddLvl();
            FindObjectOfType<GameRule>().CountActiveFood();
            gameObject.SetActive(false);
        }
    }
}
