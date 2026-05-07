using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class DEV_COLLIDER_PROCESSING : MonoBehaviour
{
	public bool logTriggerEnter = true;
	public bool logTriggerExit = true;
	public bool logCollisionEnter = true;
	public bool logCollisionExit = true;

	public void OnTriggerEnter(Collider other)
	{
		if (logTriggerEnter)
		{
			Debug.Log(other.name);
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (logTriggerExit)
		{
			Debug.Log(other.name);
		}
		
	}

	public void OnCollisionEnter(Collision other)
	{
		if (logCollisionEnter)
		{
			Debug.Log(other.gameObject.name);
		}
		
	}
	
	public void OnCollisionExit(Collision other)
	{
		if (logCollisionExit)
		{
			Debug.Log(other.gameObject.name);
		}
		
	}

	public void OnParticleCollision(GameObject other)
	{
		Debug.Log(other.name);
	}

	public void OnParticleTrigger()
	{
		Debug.Log("Trigger");
	}


}
