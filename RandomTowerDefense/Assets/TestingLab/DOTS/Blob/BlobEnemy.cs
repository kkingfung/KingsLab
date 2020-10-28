using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class BlobEnemy : MonoBehaviour, IDeclareReferencedPrefabs
{
    public Transform[] transformArray;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        for (int i = 0; i < transformArray.Length; i++)
        {
            referencedPrefabs.Add(transformArray[i].gameObject);
        }
    }
}