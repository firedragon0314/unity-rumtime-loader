using UnityEngine;
using UnityEngine.InputSystem; // 必須引用新版輸入系統
using Cysharp.Threading.Tasks;

public class MockServerSimulator : MonoBehaviour
{
    [Header("輸入設定")]
    [Tooltip("在 Inspector 點擊 '+' 並綁定按鍵 (例如 <Keyboard>/t)")]
    public InputAction triggerAction;

    [Header("測試設定")]
    public string testModelUrl = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF-Binary/Duck.glb";

    private void OnEnable()
    {
        // 新版輸入系統必須手動啟用 Action
        triggerAction.Enable();
    }

    private void OnDisable()
    {
        triggerAction.Disable();
    }

    void Update()
    {
        // 檢查本幀是否按下按鍵
        if (triggerAction.WasPressedThisFrame())
        {
            SimulateCreateProgObj();
        }
    }

    public void SimulateCreateProgObj()
    {
        Debug.Log("<color=orange>[MockServer] 模擬發送 CreateEntityProgObj...</color>");

        var data = new CreateProgObjData();
        data.id = "mock_entity_" + Random.Range(100, 999);

        // 根據您的 DataModels 結構填寫
        data.gltf = new GLTFInfo
        {
            url = testModelUrl,
            name = "TestDuck"
        };

        data.pose = new PoseData
        {
            position = new Vector3Data { x = 0, y = 1, z = 2 },
            rotation = new Vector3Data { x = 0, y = 0, z = 0 },
            scale = new Vector3Data { x = 1, y = 1, z = 1 }
        };

        data.scriptCode = @"
using UnityEngine;
public class RotatingDuck : MonoBehaviour {
    void Update() {
        transform.Rotate(Vector3.up, 100 * Time.deltaTime);
    }
}";

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.TriggerMockCreateEvent(data);
        }
    }
}