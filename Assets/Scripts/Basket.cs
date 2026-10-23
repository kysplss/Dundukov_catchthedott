using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Корзина: едет к месту тапа и "ловит" точку по расстоянию (без физики).
/// Ввод работает и со старым Input Manager, и с новым Input System.
/// </summary>
public class Basket : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;    // скорость корзины
    [SerializeField] private float catchRadius = 0.8f; // радиус поимки
    [SerializeField] private Dot dot;                  // ссылка на точку

    private Vector2 targetPosition; // куда едет корзина
    private bool isCaught = false;  // защита от двойного засчитывания

    private void Start()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        // Если игра закончилась, ничего не делаем
        if (!GameManager.Instance.IsRunning) return;

        // Тап (на ПК - клик мышью)
        if (TryGetTap(out Vector2 screenPos))
        {
            // Переводим экранные координаты в мировые
            Vector3 wp = Camera.main.ScreenToWorldPoint(screenPos);
            targetPosition = new Vector2(wp.x, wp.y);
        }

        // Плавное движение с постоянной скоростью
        transform.position = Vector2.MoveTowards(
            transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Проверка попадания по расстоянию
        if (!isCaught &&
            Vector2.Distance(transform.position, dot.transform.position) < catchRadius)
        {
            isCaught = true;
            GameManager.Instance.AddScore(1);
            dot.MoveToRandomPosition();
            Invoke(nameof(ResetCatch), 0.2f);
        }
    }

    /// <summary>Возвращает true в кадр, когда игрок тапнул/кликнул, и позицию в пикселях.</summary>
    private bool TryGetTap(out Vector2 screenPos)
    {
#if ENABLE_INPUT_SYSTEM
        // Новый Input System (Unity 6 по умолчанию)
        var touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
        {
            screenPos = touch.primaryTouch.position.ReadValue();
            return true;
        }
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            screenPos = mouse.position.ReadValue();
            return true;
        }
        screenPos = default;
        return false;
#else
        // Старый Input Manager
        if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }
        screenPos = default;
        return false;
#endif
    }

    private void ResetCatch()
    {
        isCaught = false;
    }
}