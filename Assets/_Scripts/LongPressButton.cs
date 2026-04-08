using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEngine.Events.UnityEvent OnButtonPressed;
    public UnityEngine.Events.UnityEvent OnButtonReleased;
    
    private Image image;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color pressedColor;
    private bool isPressed;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        image.color = pressedColor;
        OnButtonPressed?.Invoke();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        image.color = defaultColor;
        OnButtonReleased?.Invoke();
    }
}
