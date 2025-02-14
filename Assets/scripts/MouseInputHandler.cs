using UnityEngine;

public class MouseInputHandler : MonoBehaviour
{
    private Vector2 mousePosition;
    private bool[] mouseButtons = new bool[3];

    void Update()
    {
        // Use the stored mouse position and button states in your game logic
        // For example:
        if (mouseButtons[0]) // Left click
        {
            Debug.Log("Left mouse button is down");
            // Handle left click
        }
        if (mouseButtons[1]) // Right click
        {
            Debug.Log("Right mouse button is down");
            // Handle right click
        }
        if (mouseButtons[2]) // Middle click
        {
            Debug.Log("Middle mouse button is down");
            // Handle middle click
        }

        // Use mousePosition for any logic that requires mouse position
        Debug.Log($"Mouse position: {mousePosition}");
    }

    public void HandleMouseEvent(string eventData)
    {
        string[] data = eventData.Split(',');
        if (data.Length == 4)
        {
            string eventType = data[0];
            float x = float.Parse(data[1]);
            float y = float.Parse(data[2]);
            int button = int.Parse(data[3]);

            mousePosition = new Vector2(x, y);

            if (eventType == "down")
            {
                mouseButtons[button] = true;
            }
            else if (eventType == "up")
            {
                mouseButtons[button] = false;
            }
        }
    }

    public void HandleMouseMove(string positionData)
    {
        string[] positions = positionData.Split(',');
        if (positions.Length == 2)
        {
            float x = float.Parse(positions[0]);
            float y = float.Parse(positions[1]);
            mousePosition = new Vector2(x, y);
        }
    }
}