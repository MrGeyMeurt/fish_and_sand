using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGame : MonoBehaviour
{
    
    [SerializeField] private GameRule gameRule;

    void OnTriggerEnter(Collider other)
    {
        gameRule?.SetGameOver();
    }
}
