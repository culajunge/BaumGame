using UnityEngine;
using TMPro;

public class UiMenuDebugger : MonoBehaviour
{
    [SerializeField] private TMP_Text mouseText;
    [SerializeField] private TMP_Text consoleText;

    void Update()
    {
        mouseText.text = "Mouse X: " + Input.mousePosition.x + "\nMouse Y: " + Input.mousePosition.y +
                         "\nMouse Button: " + Input.GetMouseButton(0) + "\nTouch Count: " + Input.touchCount;
    }

    public void Log(string arg)
    {
        consoleText.text += "\n" + arg;
    }
}