using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour {

    BoidSettings settings;

    public List<Boid> prefab;
    public float spawnRadius = 10;
    public int spawnCount = 10;

    void Awake () {
        for (int i = 0; i < spawnCount; ++i) {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            int rand = Random.Range(0, prefab.Count);
            Boid boid = Instantiate (prefab[rand]);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;
            boid.transform.parent = this.transform;
        }
    }
}