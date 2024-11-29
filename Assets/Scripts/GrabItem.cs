using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using StarterAssets;
using UnityEngine;
using TMPro;

[RequireComponent (typeof(Collider))]

public class GrabItem : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    private const string ITEM_TAG = "Item";

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != PLAYER_TAG)
        {
            Debug.Log($"This was not the player entering");
        } else
        {
            Debug.Log("Player entering is trigger");
            if (transform.parent != null)
            {
                transform.parent.gameObject.SetActive(false);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag != PLAYER_TAG)
        {
            Debug.Log("This was not the player leaving");
        } else
        {
            Debug.Log("Player leaving is trigger");
        }
    }
}
