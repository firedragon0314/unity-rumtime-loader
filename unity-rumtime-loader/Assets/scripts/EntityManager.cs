using System.Collections.Generic;
using UnityEngine;
using System; // 修正：正確的引用方式

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance { get; private set; }

    private Dictionary<string, GameObject> _entities = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterEntity(string id, GameObject obj)
    {
        if (!_entities.ContainsKey(id))
        {
            _entities.Add(id, obj);
            Debug.Log($"[EntityManager] 已註冊實體: {id}");
        }
    }

    public GameObject GetEntity(string id)
    {
        if (_entities.TryGetValue(id, out GameObject obj)) return obj;
        return null;
    }

    public void ApplyPose(Transform target, PoseData pose)
    {
        if (target == null || pose == null) return;

        target.localPosition = new Vector3(pose.position.x, pose.position.y, pose.position.z);
        target.localRotation = Quaternion.Euler(pose.rotation.x, pose.rotation.y, pose.rotation.z);
        target.localScale = new Vector3(pose.scale.x, pose.scale.y, pose.scale.z);
    }
}

// --- 資料結構部分 ---
// 注意：如果報錯說 PoseData 已存在，請把下面這段刪除或註解掉

[Serializable]
public class Vector3Data
{
    public float x, y, z;
}