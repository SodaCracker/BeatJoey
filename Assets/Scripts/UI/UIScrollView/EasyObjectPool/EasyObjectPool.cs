/* 
 * Unless otherwise licensed, this file cannot be copied or redistributed in any format without the explicit consent of the author.
 * (c) Preet Kamal Singh Minhas, http://marchingbytes.com
 * contact@marchingbytes.com
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingBytes
{

    /// <summary>
    /// Easy object pool.
    /// </summary>
    [AddComponentMenu("ScrollRect/EasyObjectPool")]
	public class EasyObjectPool : MonoBehaviour 
    {

		//public static EasyObjectPool instance;
		[Header("Editing Pool Info value at runtime has no effect")]
		public PoolInfo[] poolInfo;

		//mapping of pool name vs list
		private Dictionary<string, Pool> poolDictionary  = new Dictionary<string, Pool>();
		
		// Use this for initialization
		void Awake () 
        {
			//set instance
			//instance = this;
			//check for duplicate names
			CheckForDuplicatePoolNames();
			//create pools
			//CreatePools();
		}
		
		private void CheckForDuplicatePoolNames()
        {
			for (int index = 0; index < poolInfo.Length; index++) {
				string poolName = poolInfo[index].poolName;
				if(poolName.Length == 0) {
					Debug.LogError(string.Format("Pool {0} does not have a name!",index));
				}
				for (int internalIndex = index + 1; internalIndex < poolInfo.Length; internalIndex++) {
					if(poolName.Equals(poolInfo[internalIndex].poolName)) {
						Debug.LogError(string.Format("Pool {0} & {1} have the same name. Assign different names.", index, internalIndex));
					}
				}
			}
		}

		public void CreatePools()
        {
			foreach (PoolInfo currentPoolInfo in poolInfo) 
            {
				if(currentPoolInfo.prefab == null)
                    Debug.LogError(string.Format("EasyObjectPool creating pool error,pool prefab is null,pool name: {0},pool path: {1}", currentPoolInfo.poolName,
                        GetFullPath(gameObject)));

                var parent = currentPoolInfo.parent != null ? currentPoolInfo.parent : transform;

                Pool pool = new Pool(currentPoolInfo.poolName, currentPoolInfo.prefab, 
				                     currentPoolInfo.poolSize, currentPoolInfo.fixedSize, parent);

                pool.EventOnPoolObjectCreated += OnPoolObjectCreated;

                pool.InitPool();
				
				Debug.Log("Creating pool: " + currentPoolInfo.poolName);
				//add to mapping dict
				poolDictionary[currentPoolInfo.poolName] = pool;

			}
		}

        public string GetFullPath(GameObject gameObject)
        {
            // 初始化路径为GameObject自身的名称
            string path = gameObject.name;
            // 循环遍历父对象，构建完整路径
            while (gameObject.transform.parent != null)
            {
                // 移动到上一级父对象
                gameObject = gameObject.transform.parent.gameObject;
                // 将当前父对象的名称添加到路径的开头
                path = gameObject.name + "/" + path;
            }
            return path;
        }


        /* Returns an available object from the pool 
		OR 
		null in case the pool does not have any object available & can grow size is false.
		*/
        public GameObject GetObjectFromPool(string poolName, Vector3 position, Quaternion rotation) 
        {
			GameObject result = null;
			
			if(poolDictionary.ContainsKey(poolName)) {
				Pool pool = poolDictionary[poolName];
				result = pool.NextAvailableObject(position,rotation);
				//scenario when no available object is found in pool
				if(result == null) {
					Debug.LogWarning("No object available in pool. Consider setting fixedSize to false.: " + poolName);
				}
				
			} else {
				Debug.LogError("Invalid pool name specified: " + poolName);
			}
			
			return result;
		}
		
		public GameObject GetObjectFromPool(string poolName) 
        {
			GameObject result = null;
			
			if(poolDictionary.ContainsKey(poolName)) {
				Pool pool = poolDictionary[poolName];
				result = pool.NextAvailableObject();
				//scenario when no available object is found in pool
				if(result == null) {
					Debug.LogWarning("No object available in pool. Consider setting fixedSize to false.: " + poolName);
				}
				
			} else {
				Debug.LogError("Invalid pool name specified: " + poolName);
			}
			
			return result;
		}

		public void ReturnObjectToPool(GameObject go) {
			PoolObject po = go.GetComponent<PoolObject>();
			if(po == null) {
				Debug.LogWarning("Specified object is not a pooled instance: " + go.name);
			} else {
				if(poolDictionary.ContainsKey(po.poolName)) {
					Pool pool = poolDictionary[po.poolName];
					pool.ReturnObjectToPool(po);
				} else {
					Debug.LogWarning("No pool available with name: " + po.poolName);
				}
			}
		}

        /// <summary>
        /// 根据名字获取poolStack
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public Stack<PoolObject> GetObjectPoolByName(string poolName)
        {
            Pool pool = null;
            if (poolDictionary.TryGetValue(poolName, out pool))
            {
                return pool.GetObjectPoolStack();
            }
            return null;
        }


        /// <summary>
        /// 根据名字获取poolInfo
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public PoolInfo GetPoolInfoByName(string poolName)
        {
            foreach (var info in poolInfo)
            {
                if (info.poolName == poolName)
                {
                    return info;
                }
            }

            return null;
        }

        protected void OnPoolObjectCreated(string poolName, GameObject obj)
        {
            if (EventOnPoolObjectCreated != null)
            {
                EventOnPoolObjectCreated(poolName, obj);
            }
        }

        public event Action<string, GameObject> EventOnPoolObjectCreated;
	}
}
