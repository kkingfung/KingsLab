using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MetaballContainer : MonoBehaviour {
    [Range(0.02f,0.1f)]
    public float resolution;
    [Range(1f, 5f)]
    public float threshold;
    public ComputeShader computeShader;
    public bool calculateNormals;

    private CubeGrid grid;

    public void Start() {
        this.grid = new CubeGrid(this, this.computeShader);
    }

    public void Update() {
        this.grid.evaluateAll(this.GetComponentsInChildren<MetaBall>());

        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = this.grid.vertices.ToArray();
        mesh.triangles = this.grid.getTriangles();

        if(this.calculateNormals) {
            mesh.RecalculateNormals();
        }
    }

    public void OnApplicationQuit() {
        grid.Release();
    }
}