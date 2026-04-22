using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class RTTDisplayer : MonoBehaviour
{
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        text.text = $"RTT: {Client.instance.smoothRTT:0.0} ms";
    }
}
