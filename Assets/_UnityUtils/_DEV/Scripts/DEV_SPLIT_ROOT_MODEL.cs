#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

#endif
using UnityEngine;

public class DEV_SPLIT_ROOT_MODEL : MonoBehaviour
{
	#if UNITY_EDITOR
	public Transform sourceModel;
	public Transform outPutModel;

	public DEV_FORMATION formation;
	public Object prefabOutfit;

	public bool FORCE_CLEAR_STORAGE = true;
	public List<GameObject> storage = new List<GameObject>();
	

	[Button]
	public void DEV_SPLIT_MODEL()                              
	{
		if (FORCE_CLEAR_STORAGE)
		{
			if (storage.Count != 0)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					DestroyImmediate(storage[i]);
				}
			}
			storage.Clear();
		}
		if (formation == null) formation = gameObject.AddComponent<DEV_FORMATION>();
		if (outPutModel == null) outPutModel = transform;
		formation.childTransforms = new List<Transform>();
		for (int i = 0; i < sourceModel.childCount; i++)
		{
			var mesh = sourceModel.GetChild(i).GetComponent<SkinnedMeshRenderer>();
			if (mesh)
			{
				GameObject obj;
				if (prefabOutfit)
				{
					var model = new GameObject();
					obj = PrefabUtility.InstantiatePrefab(prefabOutfit, outPutModel) as GameObject;
					model.transform.SetParent(obj.transform.GetChild(0));
					obj.transform.SetParent(outPutModel);
					storage.Add(obj);
					var meshRender  = model.AddComponent<MeshRenderer>();
					meshRender.sharedMaterials = mesh.sharedMaterials;
					var meshFiller =  model.AddComponent<MeshFilter>();
					meshFiller.sharedMesh = mesh.sharedMesh;
					
					model.transform.localScale = Vector3.one;
					model.transform.SetLocalPositionAndRotation(Vector3.zero , Quaternion.identity);

					string[] keys = { "Mira", "Rumi", "zoey" };

					if (keys.Any(k => mesh.sharedMaterials[0].name.Contains(k)))
					{
						obj.transform.SetSiblingIndex(0);
					}
					else
					{
						formation.childTransforms.Add(obj.transform);
					}
				}
				else
				{
					obj = new GameObject();
					obj.transform.SetParent(outPutModel);
					storage.Add(obj);
					var meshRender  = obj.AddComponent<MeshRenderer>();
					meshRender.sharedMaterials = mesh.sharedMaterials;
					var meshFiller =  obj.AddComponent<MeshFilter>();
					meshFiller.sharedMesh = mesh.sharedMesh;

					string[] keys = { "Mira", "Rumi", "zoey" };

					if (keys.Any(k => mesh.sharedMaterials[0].name.Contains(k)))
					{
						obj.name = mesh.sharedMaterials[0].name;
						obj.transform.SetSiblingIndex(0);
					}
					else
					{
						formation.childTransforms.Add(obj.transform);
					}
				}
			}
			else
			{
				continue;
			}
		}

		formation.formationType = DEV_FORMATION.FORMATION_TYPE.HORIZONTAL;
		formation.offsetCircle = 5f;
		formation.DEV_FORMATION_GROUP_POS();
	}
	#endif
}
