using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ViitorCloud.GameHelper.Util
{
	[CreateAssetMenu(fileName = "ResourceDB", menuName = "ResourcesManager/DB")]
	public class ResourcesDatabase : ScriptableObject {

		[SerializeField]
		[FormerlySerializedAsAttribute("m_resourcesData")]
		private List<ResourcesData> resourcesData_;
		[SerializeField]
		[FormerlySerializedAsAttribute("m_resourcesType")]
		private List<ResourceType> resourcesType_;

		public bool TryGetPrefabVariantId(string id, out string newPathId){
			foreach(var itemData in resourcesData_){
				if(itemData.Id == id){
					newPathId = itemData.Path;
					return true;
				}
			}

			newPathId = null;
			return false;
		}

		public bool GetResourceType(string id, out ControllerType type)
		{
			foreach (var itemData in resourcesType_)
			{
				if(itemData.Id == id){
					type = itemData.Type;
					return true;
				}
			}

			type = ControllerType.Resources;
			return false;
		}

		[Serializable]
		struct ResourcesData{
			public string Id;
			public string Path;
		}

		[Serializable]
		struct ResourceType{
			public string Id;
			public ControllerType Type;
		}	
	}
}
