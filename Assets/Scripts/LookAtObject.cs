using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtObject : MonoBehaviour
{
    [SerializeField] GameObject _object;

    void Start()
    {
        
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(_object.transform.position - transform.position);        
    }
}
