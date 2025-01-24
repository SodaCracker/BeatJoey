/* 
 * Unless otherwise licensed, this file cannot be copied or redistributed in any format without the explicit consent of the author.
 * (c) Preet Kamal Singh Minhas, http://marchingbytes.com
 * contact@marchingbytes.com
 */
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MarchingBytes
{

    [System.Serializable]
    public class PoolInfo
    {
        public string poolName;
        public GameObject prefab;
        public int poolSize;
        public bool fixedSize;
        public Transform parent;
    }

    class Pool
    {
        private Stack<PoolObject> availableObjStack = new Stack<PoolObject>();

        private bool fixedSize;
        private GameObject poolObjectPrefab;
        private int poolSize;
        private string poolName;

        private Transform poolRoot;

        public Pool(string poolName, GameObject poolObjectPrefab, int initialCount, bool fixedSize, Transform pool)
        {
            this.poolName = poolName;
            this.poolObjectPrefab = poolObjectPrefab;
            this.poolSize = initialCount;
            this.fixedSize = fixedSize;
            this.poolRoot = pool;

        }

        public void InitPool()
        {
            //populate the pool
            for (int index = 0; index < poolSize; index++)
            {
                AddObjectToPool(NewObjectInstance(index));
            }
        }

        //o(1)
        private void AddObjectToPool(PoolObject po)
        {
            //add to pool
            po.gameObject.SetActive(false);
            availableObjStack.Push(po);
            po.isPooled = true;
            po.transform.SetParent(poolRoot, false);
        }

        private PoolObject NewObjectInstance(int i = 0)
        {
            GameObject go = (GameObject)GameObject.Instantiate(poolObjectPrefab);
            PoolObject po = go.GetComponent<PoolObject>();
            if (po == null)
            {
                po = go.AddComponent<PoolObject>();
            }
            go.name = i.ToString();
            //set name
            po.poolName = poolName;

            if (EventOnPoolObjectCreated != null)
            {
                EventOnPoolObjectCreated(poolName, go);
            }
            return po;
        }

        //o(1)
        public GameObject NextAvailableObject(Vector3 position, Quaternion rotation)
        {
            PoolObject po = null;
            if (availableObjStack.Count > 0)
            {
                po = availableObjStack.Pop();
            }
            else if (fixedSize == false)
            {
                //increment size var, this is for info purpose only
                poolSize++;
                Debug.Log(string.Format("Growing pool {0}. New size: {1}", poolName, poolSize));
                //create new object
                po = NewObjectInstance(poolSize);
            }
            else
            {
                Debug.LogWarning("No object available & cannot grow pool: " + poolName);
            }

            GameObject result = null;
            if (po != null)
            {
                po.isPooled = false;
                result = po.gameObject;
                result.SetActive(true);

                result.transform.position = position;
                result.transform.rotation = rotation;
            }

            return result;
        }

        public GameObject NextAvailableObject()
        {
            PoolObject po = null;
            if (availableObjStack.Count > 0)
            {
                po = availableObjStack.Pop();
            }
            else if (fixedSize == false)
            {
                //increment size var, this is for info purpose only
                poolSize++;
                Debug.Log(string.Format("Growing pool {0}. New size: {1}", poolName, poolSize));
                //create new object
                po = NewObjectInstance(poolSize);
            }
            else
            {
                Debug.LogWarning("No object available & cannot grow pool: " + poolName);
            }

            GameObject result = null;
            if (po != null)
            {
                po.isPooled = false;
                result = po.gameObject;
                result.SetActive(true);
            }

            return result;
        }

        //o(1)
        public void ReturnObjectToPool(PoolObject po)
        {

            if (poolName.Equals(po.poolName))
            {

                /* we could have used availableObjStack.Contains(po) to check if this object is in pool.
				 * While that would have been more robust, it would have made this method O(n) 
				 */
                if (po.isPooled)
                {
                    Debug.LogWarning(po.gameObject.name + " is already in pool. Why are you trying to return it again? Check usage.");
                }
                else
                {
                    AddObjectToPool(po);
                }

            }
            else
            {
                Debug.LogError(string.Format("Trying to add object to incorrect pool {0} {1}", po.poolName, poolName));
            }
        }

        public Stack<PoolObject> GetObjectPoolStack()
        {
            return availableObjStack;
        }

        public event Action<string, GameObject> EventOnPoolObjectCreated;

    }
}
