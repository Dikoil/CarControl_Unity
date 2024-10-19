using UnityEngine;

public class WheelGroundContact : MonoBehaviour
{
    public Transform[] wheels; // Список трансформов колес
    public float rayLength = 1.5f; // Длина луча для проверки
    public LayerMask groundLayer;  // Слой земли (установите слой для земли)
    public float adjustSpeed = 10f; // Скорость подстройки колеса к земле

    void Update()
    {
        foreach (Transform wheel in wheels)
        {
            KeepWheelOnGround(wheel);
        }
    }

    void KeepWheelOnGround(Transform wheel)
    {
        // Создаем луч от колеса вниз
        Ray ray = new Ray(wheel.position, -wheel.transform.up);
        RaycastHit hit;

        // Если луч попадает в землю
        if (Physics.Raycast(ray, out hit, rayLength, groundLayer))
        {
            // Поднимаем или опускаем колесо до точки контакта с поверхностью
            Vector3 targetPosition = hit.point + (wheel.up * 0.1f); // Немного приподнимаем для контакта
            wheel.position = Vector3.Lerp(wheel.position, targetPosition, Time.deltaTime * adjustSpeed);
        }
    }
}
