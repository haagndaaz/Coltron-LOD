using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controlLOD : MonoBehaviour {

	//VARIABLES

	//Timer variables
	public float timer;
	public List<float> distTimers;
	public float updateWait;
	public List<float> randUpdateWait;
	public float randAmount;
	public Vector3 oldPosition;

	//Distance things
	public float maxDistance;

	//object lists
	public List<GameObject> closeActors;
	public GameObject[] actors;

	void GetActorValues ()
	{
		//first clear List
		randUpdateWait.Clear();

		//Set a list of random values for randUpdateWait
		for (int i = 0; i < closeActors.Count; i++)
		{
			randUpdateWait.Add( Random.Range(updateWait - randAmount, updateWait + randAmount) );
		}
	}

	// Use this for initialization
	void Start () {
		//setup list of all objects with actorLOD component
	    actors = GameObject.FindGameObjectsWithTag("actorLOD");

		//initial object swap
		getActors();
		GetActorValues();
		oldPosition = transform.position;
		for (int i = 0; i < closeActors.Count; i++)
		{
			//checkDist(i);
			float distanceTemp = Vector3.Distance(closeActors[i].transform.position, transform.position);
			closeActors[i].GetComponent<actorLOD>().nDistance = distanceTemp;
			
			setLOD(closeActors[i]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		//only run timers if player is movinf
		if (transform.position != oldPosition)
		{
			//set timer limit to largest randUpdateWait if the list is filled, otherwise use UpdateWait
			float maxTime = 0;
			if (randUpdateWait.Count != 0)
			{
				maxTime = Mathf.Max(randUpdateWait.ToArray());
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
				distTimers[i] += Time.deltaTime;
				if ( distTimers[i] > this.randUpdateWait[i] )
				{
					//checkDist(i);
					float distanceTemp = Vector3.Distance(closeActors[i].transform.position, transform.position);
					closeActors[i].GetComponent<actorLOD>().nDistance = distanceTemp;

					setLOD(closeActors[i]);
					
					distTimers[i] = 0;
				}
			}
		}
	}

	//get all actors within maxDistance radius
	void getActors ()
	{
		//first clear lists
		closeActors.Clear();
		distTimers.Clear();

		//get objects within range with a collider attached
		Collider[] hitActors = Physics.OverlapSphere(transform.position, maxDistance);

		//assign objects with actor LOD to closeActors
		foreach (Collider c in hitActors)
		{
			if ( c.GetComponent<actorLOD>() )
			{
				//add object to closeActors and create a timer for it
				closeActors.Add(c.gameObject);
				distTimers.Add(0);
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
			if ( actorScript.nDistance < (this.maxDistance * (actorScript.LODs[i].distancePercentage * 0.01)) )
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
				if ( actorScript.nDistance < (this.maxDistance * (actorScript.LODs[i-1].distancePercentage * 0.01)) )
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
