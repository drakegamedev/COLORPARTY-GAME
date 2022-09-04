using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For objects that will be put in the Object Pooler
public class Poolable : MonoBehaviour
{
    // Returns object back to the Object Pooler
    public void ReturnToPool()
    {
        transform.SetParent(ObjectPooler.Instance.transform);
        gameObject.SetActive(false);
    }
}