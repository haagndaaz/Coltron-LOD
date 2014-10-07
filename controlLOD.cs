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
	public float errorAllowance;

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

					setLOD(closeActors[i]);
					
					actorScript.distTimer = 0;
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
			if ( actorScript.distance < (this.maxDistance * (actorScript.LODs[i].distancePercentage * 0.01)) )
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
				if ( actorScript.distance < (this.maxDistance * (actorScript.LODs[i-1].distancePercentage * 0.01)) )
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
