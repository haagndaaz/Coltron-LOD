using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class actorLOD : MonoBehaviour { 

	//VARIABLES
	public float distance;
	public float distTimer = 0;
	public float randUpdateWait;

	//LOD class
	[System.Serializable]
	public class coltronLOD 
	{
		[SerializeField] public string name;
		[SerializeField] public GameObject[] lodObjects;
		[SerializeField] public float distancePercentage;
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
