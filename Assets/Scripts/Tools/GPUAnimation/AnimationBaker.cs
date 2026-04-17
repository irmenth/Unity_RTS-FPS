#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class AnimationBaker : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private SkinnedMeshRenderer smr;
    [SerializeField] private AnimationClip clip;
    [SerializeField] private string clipName;
    [SerializeField][Range(1, 60)] private int frame;

    [MenuItem("Tools/GPU Animation/Bake Selected")]
    public static void OpenBaker()
    {
        AnimationBaker baker = FindFirstObjectByType<AnimationBaker>();
        if (baker == null) { Debug.LogError("场景中未找到 AnimationBaker 组件"); return; }
        baker.BakeSync();
    }

    public void BakeSync()
    {
        if (root == null || smr == null || clip == null) return;

        root.SetActive(true);

        int vertexCount = smr.sharedMesh.vertexCount;
        int frameCount = Mathf.CeilToInt(clip.length * frame);

        Texture2D tex = new(frameCount, vertexCount * 2, TextureFormat.RGBAFloat, false, true);
        smr.updateWhenOffscreen = true;
        Mesh bakeMesh = new();

        AnimationMode.StartAnimationMode();

        for (int f = 0; f < frameCount; f++)
        {
            float time = f / (frameCount - 1f) * clip.length;
            AnimationMode.SampleAnimationClip(root, clip, time);
            smr.BakeMesh(bakeMesh, true);

            Vector3[] v = bakeMesh.vertices;
            Vector3[] n = bakeMesh.normals;

            for (int i = 0; i < vertexCount; i++)
            {
                tex.SetPixel(f, i * 2, new Color(v[i].x, v[i].y, v[i].z));
                tex.SetPixel(f, i * 2 + 1, new Color(n[i].x, n[i].y, n[i].z));
            }

            bakeMesh.Clear();
        }

        AnimationMode.StopAnimationMode();

        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();

        string dir = "Assets/Resources/GPUAnimationTexture";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string path = $"{dir}/{clipName}.asset";

        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(tex, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"烘焙完成: {frameCount} 帧 | {vertexCount} 顶点");
    }
}
#endif