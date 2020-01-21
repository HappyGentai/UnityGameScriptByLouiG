using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MiniBeatGameEditor : EditorWindow
{
    [MenuItem("LouiG/MiniBeatGame/CreateStage")]
    public static void CreateBasicBeatGame()
    {
        GameObject stage = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MiniBeatGameByLouiG/Prefabs/MiniGame_Beat.prefab");
        Instantiate(stage);
    }
}
