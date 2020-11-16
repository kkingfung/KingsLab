using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Entities;

public class BoidDots : MonoBehaviour
{
    private BoidSpawnerDots boidSpawnerDots;
    private Vector3 prevPos;
    private int entityID = -1;

    // Start is called before the first frame update
    void Start()
    {
        if (boidSpawnerDots == null) boidSpawnerDots = FindObjectOfType<BoidSpawnerDots>();
        prevPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != prevPos)
        {
            Vector3 direction = transform.position - prevPos;
            transform.forward = direction;
            prevPos = transform.position;
        }
    }

    public void Init(BoidSpawnerDots boidSpawnerDots,int entityID)
    {
        this.boidSpawnerDots = boidSpawnerDots;
        this.entityID = entityID;
    }
}
