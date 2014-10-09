using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controlLOD : MonoBehaviour {
	
	//VARIABLES
	
	//Timer variables
	public float timer;
	public float updateWait;
	public float randAmount;
	public Vector3 oldPosition;
	
	//Distance things
	public float maxDistance;
	public float fadeDistance = 5;
	public float errorAllowance = 2;
	
	//object lists
	public List<GameObject> closeActors;
	public List<GameObject> actors;
	
	void GetActorValues ()
	{
		//first clear List
		//randUpdateWait.Clear();
		
		//Set a list of random values for randUpdateWait
		for (int i = 0; i < actors.Count; i++)
		{
			actors[i].GetComponent<actorLOD>().randUpdateWait = Random.Range(updateWait - randAmount, updateWait + randAmount);
		}
	}
	
	// Use this for initialization
	void Start () {
		//setup list of all objects with actorLOD component
		GameObject[] tempActors = GameObject.FindGameObjectsWithTag("actorLOD");
		foreach (GameObject o in tempActors)
		{
			if (o.GetComponent<actorLOD>())
			{
				o.GetComponent<actorLOD>().distance = maxDistance;
				o.GetComponent<actorLOD>().oldDistance = maxDistance;
				actors.Add ( o );
			}
		}
		
		//initial object swap
		getActors();
		GetActorValues();
		oldPosition = transform.position;
		for (int i = 0; i < closeActors.Count; i++)
		{
			//checkDist(i);
			float distanceTemp = Vector3.Distance(closeActors[i].transform.position, transform.position);
			closeActors[i].GetComponent<actorLOD>().distance = distanceTemp;
			
			setLOD(closeActors[i]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		//only run timers if player is movinf
		if (transform.position != oldPosition)
		{
			//set timer limit to largest number if the list is filled, otherwise use UpdateWait
			float maxTime = 0;
			if (closeActors.Count != 0)
			{
				maxTime = updateWait + randAmount;
			}
			else
			{
				maxTime = updateWait;
			}
			
			//Timer for getActors
			timer += Time.deltaTime;
			if (timer > maxTime)
			{
				getActors();
				GetActorValues();
				oldPosition = transform.position;
				timer = 0;
			}
			
			//distance timers
			for (int i = 0; i < closeActors.Count; i++)
			{
				actorLOD actorScript = closeActors[i].GetComponent<actorLOD>();
				
				actorScript.distTimer += Time.deltaTime;
				if ( actorScript.distTimer > actorScript.randUpdateWait )
				{
					//checkDist(i);
					float distanceTemp = Vector3.Distance(closeActors[i].transform.position, transform.position);
					closeActors[i].GetComponent<actorLOD>().distance = distanceTemp;
					
					//setLOD(closeActors[i]);
					
					actorScript.distTimer = 0;
				}
			}
		}
		
		//FADING
		for (int i = 0; i < closeActors.Count; i++)
		{
			//get actorLOD script reference
			actorLOD actorScript = closeActors[i].GetComponent<actorLOD>();
			
			if ( actorScript.oldDistance != actorScript.distance)
			{
				for (int l = 0; l < actorScript.LODs.Length; l++)
				{
					//lerp oldDistance to new distance so it looks like a smooth transition
					actorScript.oldDistance = Mathf.Lerp( actorScript.oldDistance, actorScript.distance, Time.deltaTime );
					
					//calculate nDistance differently if it is the last LOD in the list
					if ( l == actorScript.LODs.Length - 1 )
					{
						float newMaxDistance = (float) ( maxDistance * (actorScript.LODs[l].distancePercentage * 0.01) ) - errorAllowance;
						float newMinDistance = (float) ( maxDistance * (actorScript.LODs[l].distancePercentage * 0.01) ) - fadeDistance - errorAllowance;
						
						actorScript.LODs[l].nDistance = (float) Mathf.Clamp01( (actorScript.oldDistance - newMaxDistance) / (newMinDistance - newMaxDistance) );
					}
					else // calculate the nDistance this way fr every other LOD
					{
						float newMaxDistance = (float) ( maxDistance * (actorScript.LODs[l].distancePercentage * 0.01) ) - errorAllowance;
						float newMinDistance = (float) ( maxDistance * (actorScript.LODs[l].distancePercentage * 0.01) ) + fadeDistance - errorAllowance;
						float newMaxDistanceLast = (float) ( maxDistance * (actorScript.LODs[l].distancePercentage * 0.01) ) + (fadeDistance/2) - errorAllowance;
						
						actorScript.LODs[l].nDistance = (float) Mathf.Clamp01( (actorScript.oldDistance - newMinDistance) / (newMaxDistance - newMinDistance) );
						actorScript.LODs[l+1].nDistance = (float) Mathf.Clamp01( (actorScript.oldDistance - newMaxDistanceLast) / (newMinDistance - newMaxDistanceLast) );
					}
					
					//affect the materials on the objects
					if ( actorScript.LODs[l].nDistance > 0.05 & actorScript.LODs[l].nDistance < 0.95 )
					{
						//cycle through and turn on mesh renderers
						foreach ( GameObject o in actorScript.LODs[l].lodObjects )
						{
							o.renderer.enabled = true;
							
							foreach (Material m in o.renderer.materials)
							{
								if (m.HasProperty("_Cutoff")) //code for fading cutout shaders
								{
									m.SetFloat("_Cutoff", 1 - actorScript.LODs[l].nDistance);
								}
								else //code for fading regular alpha shaders
								{
									Color oldColor = m.GetColor("_Color");
									Color newColor = new Color( oldColor.r, oldColor.g, oldColor.b, actorScript.LODs[l].nDistance);
									m.color = newColor;
								}
							}
						}
					}
					if ( actorScript.LODs[l].nDistance < 0.05 )
					{
						//cycle through and turn off mesh renderers
						foreach ( GameObject o in actorScript.LODs[l].lodObjects )
						{
							o.renderer.enabled = false;
						}
					}

					if ( actorScript.LODs[l].nDistance > 1 )
					{
						//cycle through and turn off mesh renderers
						foreach ( GameObject o in actorScript.LODs[l].lodObjects )
						{
							o.renderer.enabled = true;
						}
					}
				}
			}
		}
	}
	
	//get all actors within maxDistance radius
	void getActors ()
	{
		//first clear lists
		closeActors.Clear();
		
		//get objects within range with a collider attached
		Collider[] hitActors = Physics.OverlapSphere(transform.position, maxDistance);
		
		//assign objects with actor LOD to closeActors
		foreach (Collider c in hitActors)
		{
			if ( c.GetComponent<actorLOD>() )
			{
				//add object to closeActors and create a timer for it
				closeActors.Add(c.gameObject);
			}
		}
	}
	
	//basic LOD swap
	void setLOD (GameObject LODobj)
	{
		//get actorLOD script reference
		actorLOD actorScript = LODobj.GetComponent<actorLOD>();
		
		//cycle through LODS based on distance
		for (int i = 0; i < actorScript.LODs.Length; i++)
		{
			//check to see if within distance to swap LOD
			if ( actorScript.distance < (this.maxDistance * (actorScript.LODs[i].distancePercentage * 0.01)) - errorAllowance )
			{
				//cycle through and turn on mesh renderers
				foreach ( GameObject o in actorScript.LODs[i].lodObjects )
				{
					o.renderer.enabled = true;
				}
			}
			else
			{
				//cycle through and turn off mesh renderers
				foreach ( GameObject o in actorScript.LODs[i].lodObjects )
				{
					o.renderer.enabled = false;
				}
			}
			
			//check to see if within the range of the LOD below it, then hide objects
			if (i != 0 )
			{
				if ( actorScript.distance < (this.maxDistance * (actorScript.LODs[i-1].distancePercentage * 0.01)) - errorAllowance )
				{
					//cycle through and turn on mesh renderers
					foreach ( GameObject o in actorScript.LODs[i].lodObjects )
					{
						o.renderer.enabled = false;
					}
				}
			}
		}
	}
}
