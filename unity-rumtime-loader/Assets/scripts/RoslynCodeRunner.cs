using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class RoslynAssemblyCompiler : MonoBehaviour
{
    public static RoslynAssemblyCompiler Instance;

    private void Awake() => Instance = this;

    // 核心功能：編譯並掛載
    public async UniTask CompileAndAttach(string code, GameObject targetObject)
    {
        Debug.Log($"[Compiler] 開始編譯腳本到 {targetObject.name}...");

        // 1. 在背景執行緒進行編譯
        byte[] assemblyData = await UniTask.RunOnThreadPool(() => {
            return InternalCompile(code);
        });

        if (assemblyData == null) return;

        // 2. 切換回主執行緒掛載組件
        await UniTask.SwitchToMainThread();

        try
        {
            Assembly assembly = Assembly.Load(assemblyData);
            Type type = assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(MonoBehaviour)));

            if (type != null)
            {
                targetObject.AddComponent(type);
                Debug.Log($"<color=green>[Compiler] 成功掛載: {type.Name}</color>");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Compiler] 掛載失敗: {e.Message}");
        }
    }

    private byte[] InternalCompile(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        string assemblyName = Path.GetRandomFileName();

        // --- 核心修正：自動抓取目前 Unity 運作中所有的參考 ---
        var references = new List<MetadataReference>();

        // 遍歷所有已載入的 Assembly，將它們全部加入編譯參考中
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }
            catch (Exception)
            {
                // 忽略無法讀取的 Assembly
            }
        }

        // 如果你的環境比較特殊，額外確保 netstandard 被加入
        // 部分 Unity 版本需要明確指定這個參考
        var netstandard = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "netstandard");
        if (netstandard != null)
        {
            references.Add(MetadataReference.CreateFromFile(netstandard.Location));
        }

        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using (var ms = new MemoryStream())
        {
            var result = compilation.Emit(ms);
            if (result.Success)
            {
                return ms.ToArray();
            }

            // 輸出詳細錯誤
            foreach (var diag in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
            {
                Debug.LogError($"[Roslyn Error]: {diag.GetMessage()}");
            }
            return null;
        }
    }
}