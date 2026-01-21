using System;
using UnityEngine;

// ==========================================
//  PART 1: 核心網路協定 (Protocol Wrappers)
// ==========================================

/// <summary>
/// 1. 基礎外殼：只為了偷看 "type" 是什麼，用於第一階段解析
/// </summary>
[Serializable]
public class BaseMessage
{
    public string type;
}

/// <summary>
/// 2. 萬用外殼：確認 type 後，用這個把 payload 轉成正確的類別
/// </summary>
[Serializable]
public class MessageWrapper<T>
{
    public string type;
    public T data;
}

// ==========================================
//  PART 2: 通用資料結構 (Common Data)
// ==========================================

/// <summary>
/// 基礎位置與旋轉資料，被多個 Event 引用
/// </summary>
[Serializable]
public class PoseData
{
    public Vector3Data position;
    public Vector3Data rotation;
    public Vector3Data scale = new Vector3Data { x = 1, y = 1, z = 1 };
}

[Serializable]
public class GLTFInfo
{
    public string name;
    public string url;
}

/// <summary>
/// 用於 PlayerSync 的陣列索引輔助
/// </summary>
public static class BodyIndex
{
    public const int HEAD = 0;
    public const int LEFT_HAND = 1;
    public const int RIGHT_HAND = 2;
}

// ==========================================
//  PART 3: 實體相關 Payload (Entities)
// ==========================================

/// <summary>
/// 專用於 CreateEntityProgObj (需要 GLTF 資訊)
/// </summary>
[Serializable]
public class CreateProgObjData
{
    public string id;
    public string scriptCode; // 新增：存放 C# 原始碼
    public GLTFInfo gltf;
    public PoseData pose;
}

// NetworkManager 維持你原本的 WebSocket 邏輯，
// 只要確保 JsonUtility 能解析出 scriptCode 即可。

/// <summary>
/// 用於 CreateEntityGeomObj, CreateEntityAnchor 與 UpdateEntity
/// </summary>
[Serializable]
public class EntityData
{
    public string id;
    public PoseData pose;
}

/// <summary>
/// 用於 ClaimEntity (搶) 與 ReleaseEntity (放)
/// </summary>
[Serializable]
public class EntityControlData
{
    public string id;
}

[Serializable]
public class DeleteEntityData
{
    public string id;
}

// ==========================================
//  PART 4: 房間與系統 Payload (Room & System)
// ==========================================

[Serializable]
public class RoomData
{
    public string id;
}

[Serializable]
public class RoomErrorData
{
    public string reason;
}

[Serializable]
public class AudioData
{
    public string pcm; // Base64 encoded buffer
}

[Serializable]
public class ErrorMsg
{
    public string message;
}

[Serializable]
public class TranscriptMsg
{
    public string message;
}