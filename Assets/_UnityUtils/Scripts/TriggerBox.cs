using System;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TriggerBox : MonoBehaviour
{
	public UnityAction<Collider> onTriggerEnter;
	public UnityAction<Collider> onTriggerExit;

	protected virtual void Awake()
	{
		#if UNITY_EDITOR
		onTriggerEnter += other =>
		{
			if (other.CompareTag("Player"))
			{
				IS_TRIGGER = true;
			}
			
		};
		onTriggerExit += other =>
		{
			if (other.CompareTag("Player"))
			{
				IS_TRIGGER = false;
			}
		};
		#endif
	}

	private void OnTriggerEnter(Collider other)
	{
		onTriggerEnter?.Invoke(other);
	}

	private void OnTriggerExit(Collider other)
	{
		onTriggerExit?.Invoke(other);
	}
	
	
	#if UNITY_EDITOR

	[Header("EDITOR")]
	public BoxCollider boxCollider;

	private bool IS_TRIGGER;

	private MeshFilter _meshFilter;
	private MeshRenderer _meshRenderer;

	[Button]
	public void TOGGLE_EDIT_MODE()
	{
		if (_meshFilter)
		{
			DestroyImmediate(_meshFilter);
			DestroyImmediate(_meshRenderer);
			if(DebugLogger.isDebugLog) this.Log("EDIT_MODE: DISABLE!!!");
			return;
		}
		
		if (!transform.TryGetComponent<Collider>(out Collider colliderTemp))
		{
			if(DebugLogger.isDebugLog) this.LogError("Cant fount collider Component !!!");
			return;
		}
		if (transform.localScale != colliderTemp.bounds.size)
		{
			if(DebugLogger.isDebugLog) this.LogError("EDIT_MODE only working with transform scale match collider size.");
			return;
		}

		var tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		
		_meshFilter = gameObject.AddComponent<MeshFilter>();
		_meshFilter.sharedMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
		
		_meshRenderer = gameObject.AddComponent<MeshRenderer>();
		
		_meshRenderer.sharedMaterial = tempCube.GetComponent<MeshRenderer>().sharedMaterial;
		
		DestroyImmediate(tempCube);
		
		if(DebugLogger.isDebugLog) this.Log("EDIT_MODE: ENABLE!!!");
		
	}
	
	public void OnDrawGizmos()
	{
		if(!DEV_DEBUG_MANAGER.ENABLE_TRIGGER_BOX_DRAW) return;
		if (boxCollider)
		{
			Gizmos.color = IS_TRIGGER ? new Color(0f, 1f, 0f, 0.3f) : new Color(1f, 0f, 0f, 0.3f);
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(boxCollider.center, boxCollider.size);
			
			Gizmos.color = IS_TRIGGER ? Color.green : Color.red;
			Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
		}
		else
		{
			boxCollider = GetComponent<BoxCollider>();
		}
	}

	#endif

}
