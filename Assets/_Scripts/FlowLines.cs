using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowLines : MonoBehaviour {
    public Color baseColor;
    public Material material;

    public Vector3[] points;
    public Color[] colors;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void RenderLines(Vector3[] points, Color[] colors)
    {
        if (!ValidateInput(points, colors))
        {
            Debug.Log("Not Valid Input");
            return; 
        }

        material.SetPass(0);
        GL.Begin(GL.TRIANGLE_STRIP);
        GL.Color(baseColor);
        GL.Vertex(transform.position);
       
       // Debug.Log(points.Length);
        for (int id = 0; id != points.Length; id++)
        {           
            GL.Color(colors[id]);
            GL.Vertex(points[id]);      
        }
        GL.End();
    }

    private bool ValidateInput(Vector3[] points, Color[] color)
    {
        return points!=null && color!=null && points.Length == color.Length;
    }
    void OnDrawGizmos()
    {
        RenderLines(points,colors);
    }
}
