using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    private Dictionary<GameObject, List<GameObject>> _poolDictionary = new Dictionary<GameObject, List<GameObject>>();

    public GameObject GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // Create a new pool for this prefab if it doesn't exist
        if (!_poolDictionary.ContainsKey(prefab))
        {
            _poolDictionary.Add(prefab, new List<GameObject>());
        }

        // Check for an inactive projectile in the pool
        List<GameObject> pool = _poolDictionary[prefab];
        foreach (GameObject projectileObject in pool)
        {
            if (!projectileObject.activeInHierarchy)
            {
                projectileObject.transform.position = position;
                projectileObject.transform.rotation = rotation;
                projectileObject.SetActive(true);
                return projectileObject;
            }
        }

        // No inactive projectile found, create a new one
        GameObject newProjectile = Instantiate(prefab, position, rotation, transform);
        pool.Add(newProjectile);
        return newProjectile;
    }

    public void ReturnToPool(GameObject projectile)
    {
        projectile.SetActive(false);
    }
} 