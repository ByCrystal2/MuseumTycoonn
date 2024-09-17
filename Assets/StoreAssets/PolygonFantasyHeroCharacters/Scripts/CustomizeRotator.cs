using UnityEngine;
using UnityEngine.EventSystems;

public class CustomizeRotator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float rotationSpeed = 5f; 

    private bool isDragging = false; 
    private Vector2 previousMousePosition; 

    public CustomizeHandler customizeHandler;
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        previousMousePosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void Update()
    {
        if (isDragging)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 deltaMousePosition = currentMousePosition - previousMousePosition;

            float rotationX = deltaMousePosition.y * rotationSpeed * Time.deltaTime; 
            float rotationY = -deltaMousePosition.x * rotationSpeed * Time.deltaTime;

            customizeHandler.RotateCurrentActiveCamera(rotationX, rotationY);

            previousMousePosition = currentMousePosition;
        }
    }
}
