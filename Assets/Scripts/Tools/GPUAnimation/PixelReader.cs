#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PixelReader : MonoBehaviour
{
    [SerializeField] private Texture2D tex;

    [MenuItem("Tools/GPU Animation/Pixel Reader")]
    public static void OpenPixelReader()
    {
        PixelReader reader = FindAnyObjectByType<PixelReader>();
        if (reader == null) { Debug.LogError("请在 Scene 中添加 PixelReader"); return; }
        reader.Read();
    }

    private void Read()
    {
        for (int v = 0; v < tex.height; v += 3)
        {
            for (int f = 0; f < tex.width; f++)
            {
                Color c = tex.GetPixel(f, v);
                Debug.Log($"{v / 3}: c = {c}");
            }
        }
    }
}
#endif
