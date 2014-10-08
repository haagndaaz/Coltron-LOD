using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class actorLOD : MonoBehaviour { 

	//VARIABLES
	public float distance;
	public float oldDistance;
	public float distTimer = 0;
	public float randUpdateWait;

	//LOD class
	[System.Serializable]
	public class coltronLOD 
	{
		[SerializeField] public string name;
		[SerializeField] public GameObject[] lodObjects;
		[SerializeField] public float distancePercentage;
		[SerializeField] public float nDistance;
		[SerializeField] public List<Material> origMaterials;
		[SerializeField] public float[] origAlpha;
		[SerializeField] public bool fade = true;
		[SerializeField] public Shader[] fadeShader;
	}
	public coltronLOD[] LODs;

	// Use this for initialization
	void Start () {
		getLODvalues();

		//Cycle through and set intial values for each LOD
		foreach ( coltronLOD c in LODs)
		{			
			//clear lists
			c.origMaterials.Clear();

			//cycle thorugh LOD objects
			for (int i = 0; i < c.lodObjects.Length; i++)
			{
				//add to lists
				c.origMaterials.Add(c.lodObjects[i].renderer.sharedMaterial);

				//cycle through all objects in each LOD and turn off renderer
				c.lodObjects[i].renderer.enabled = false;
			}

			//turn all objects alpha to totally faded to begin
			foreach (GameObject o in c.lodObjects)
			{
				if (o.renderer.material.HasProperty("_Cutoff"))
				{
					o.renderer.material.SetFloat("_Cutoff", 1);
				}
				else
				{
					Color oldColor = o.renderer.material.GetColor("_Color");
					Color newColor = new Color( oldColor.r, oldColor.g, oldColor.b, 0);
					o.renderer.material.color = newColor;
				}
			}
		}
	}

	//get intial values values
	void getLODvalues()
	{

		foreach ( coltronLOD c in LODs)
		{
			//clear lists
			c.origMaterials.Clear();

			//cycle through all objects in each LOD
			for (int i = 0; i < c.lodObjects.Length; i++)
			{
				//add to lists
				c.origMaterials.Add(c.lodObjects[i].renderer.sharedMaterial);
			}
		}
	}
}
