using Cysharp.Threading.Tasks; // 改用 UniTask
using GLTFast;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CreateProgObj : MonoBehaviour
{
    public bool usePhysics = true;

    private void Start()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnCreateProgObj += (data) => HandleCreateProgObj(data).Forget();
    }

    private async UniTaskVoid HandleCreateProgObj(CreateProgObjData data)
    {
        Debug.Log($"[CreateProgObj] 下載模型: {data.gltf.url}");

        var gltf = new GltfImport();
        bool success = await gltf.Load(data.gltf.url);

        if (!success || this == null) return;

        GameObject newObj = new GameObject($"ProgObj_{data.gltf.name}");
        await gltf.InstantiateMainSceneAsync(newObj.transform);

        // --- 基礎設定 (Collider, RB, XR) ---
        SetupComponents(newObj, data);

        // --- 核心改動：如果有傳入程式碼，則呼叫編譯器 ---
        // 假設你的 CreateProgObjData 裡面有一個 string 欄位叫 scriptCode
        if (!string.IsNullOrEmpty(data.scriptCode))
        {
            await RoslynAssemblyCompiler.Instance.CompileAndAttach(data.scriptCode, newObj);
        }
    }

    private void SetupComponents(GameObject newObj, CreateProgObjData data)
    {
        EntityManager.Instance.ApplyPose(newObj.transform, data.pose);
        EntityManager.Instance.RegisterEntity(data.id, newObj);
    }
}