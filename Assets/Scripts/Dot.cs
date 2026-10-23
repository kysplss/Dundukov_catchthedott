using UnityEngine;

/// <summary>
/// Точка, которую нужно поймать. Умеет перемещаться в случайное место.
/// </summary>
public class Dot : MonoBehaviour
{
    // Границы области появления (в мировых единицах)
    [SerializeField] private float minX = -4f;
    [SerializeField] private float maxX = 4f;
    [SerializeField] private float minY = -7f;
    [SerializeField] private float maxY = 7f;

    private void Start()
    {
        // В первый момент сразу ставим точку в случайное место
        MoveToRandomPosition();
    }

    /// <summary>Переместить точку в случайную позицию внутри заданных границ.</summary>
    public void MoveToRandomPosition()
    {
        float x = Random.Range(minX, maxX); // для float конец включается
        float y = Random.Range(minY, maxY);
        transform.position = new Vector2(x, y);
    }
}
