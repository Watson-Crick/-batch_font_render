using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Texture2D = UnityEngine.Texture2D;
using UnityEngine;
using System.IO;
using UnityEditor;

public class Utils
{
    public static void SaveTexture(Texture2D tex)
    {
        if (tex == null)
        {
            Debug.LogError("运行时纹理为空，无法保存！");
            return;
        }

        byte[] pngData = tex.EncodeToPNG();
        if (pngData == null)
        {
            Debug.LogError("无法将纹理编码为 PNG 格式！");
            return;
        }

        string folderPath = "Assets/SavedTextures";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "SavedTexture.png");

        File.WriteAllBytes(filePath, pngData);
        Debug.Log($"纹理已保存到: {filePath}");

        AssetDatabase.Refresh();
    }
}
