using UnityEngine;
using System.Collections.Generic;

public class RacingBotAI : MonoBehaviour
{
    public Transform[] checkpoints;          // Массив чекпоинтов на трассе
    public int currentCheckpointIndex = 0;   // Индекс текущего чекпоинта
    public float motorForce = 1500f;         // Сила на двигатель
    public float brakeForce = 3000f;         // Сила торможения
    public float maxSteerAngle = 30f;        // Максимальный угол поворота колес
    public Transform centerOfMass;           // Центр масс машины

    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;

    public Transform playerCar;              // Игрок
    public DifficultyLevel difficultyLevel;  // Уровень сложности бота

    private float horizontalInput;            // Ввод для поворота
    private float verticalInput;              // Ввод для движения вперед/назад
    private float currentSteerAngle;          // Текущий угол поворота
    private float currentBrakeForce;          // Текущая сила торможения
    private bool isBraking;                   // Флаг торможения

    private Rigidbody rb;                     // Rigidbody машины
    private float detectDistance = 10f;       // Расстояние для анализа пространства

    // Перечисление уровней сложности
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition; // Установка центра масс для устойчивости
    }

    private void Update()
    {
        MoveTowardsCheckpoint();  // Движение к следующему чекпоинту
        AnalyzeEnvironment();     // Анализ окружения
        HandleMotor();            // Управление двигателем
        HandleSteering();         // Управление поворотом
        UpdateWheels();           // Обновление положения и вращения колес
    }

    // Движение бота к следующему чекпоинту
    private void MoveTowardsCheckpoint()
    {
        if (checkpoints.Length == 0) return;

        Transform targetCheckpoint = checkpoints[currentCheckpointIndex];
        Vector3 directionToCheckpoint = (targetCheckpoint.position - transform.position).normalized;

        // Автоматический ввод для движения вперед
        verticalInput = 1f;

        // Автоматический ввод для поворота в сторону чекпоинта
        horizontalInput = Vector3.Dot(transform.right, directionToCheckpoint);

        float distanceToCheckpoint = Vector3.Distance(transform.position, targetCheckpoint.position);

        // Переключение на следующий чекпоинт, если боту удалось доехать
        if (distanceToCheckpoint < 5f)
        {
            currentCheckpointIndex = (currentCheckpointIndex + 1) % checkpoints.Length;
        }
    }

    // Анализируем пространство вокруг бота
    private void AnalyzeEnvironment()
    {
        RaycastHit hit;

        // Проверяем наличие препятствий впереди
        if (Physics.Raycast(transform.position, transform.forward, out hit, detectDistance))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                // Если перед нами препятствие, слегка поворачиваем в сторону
                horizontalInput = (hit.point.x < transform.position.x) ? 1f : -1f;
            }
        }

        // Поведение, когда игрок рядом
        float distanceToPlayer = Vector3.Distance(transform.position, playerCar.position);

        if (distanceToPlayer < 15f)
        {
            // Проверяем, есть ли достаточно места для обгона с обеих сторон
            float sideDistance = 5f; // Минимальное расстояние для безопасного обгона

            bool canOvertakeLeft = !Physics.Raycast(transform.position, -transform.right, sideDistance);
            bool canOvertakeRight = !Physics.Raycast(transform.position, transform.right, sideDistance);

            // Попытка обогнать игрока
            if (transform.position.z < playerCar.position.z)
            {
                // Если есть место слева, бот обгоняет слева, если справа, то справа
                if (canOvertakeLeft)
                {
                    horizontalInput = -1f;  // Обгон слева
                }
                else if (canOvertakeRight)
                {
                    horizontalInput = 1f;   // Обгон справа
                }
                else
                {
                    horizontalInput = 0f;   // Нет места для обгона, остаемся за игроком
                }
            }
        }
    }

    // Управляем двигателем и торможением
    private void HandleMotor()
    {
        // Настройка силы мотора в зависимости от уровня сложности
        float difficultyMultiplier = GetDifficultyMultiplier();
        rearLeftWheel.motorTorque = verticalInput * motorForce * difficultyMultiplier;
        rearRightWheel.motorTorque = verticalInput * motorForce * difficultyMultiplier;

        // Применяем торможение
        currentBrakeForce = isBraking ? brakeForce : 10f;
        ApplyBraking();
    }

    // Применение торможения
    private void ApplyBraking()
    {
        frontLeftWheel.brakeTorque = currentBrakeForce;
        frontRightWheel.brakeTorque = currentBrakeForce;
        rearLeftWheel.brakeTorque = currentBrakeForce;
        rearRightWheel.brakeTorque = currentBrakeForce;
    }

    // Управляем углом поворота колес
    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheel.steerAngle = currentSteerAngle;
        frontRightWheel.steerAngle = currentSteerAngle;
    }

    // Обновляем позиции и вращение колес
    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheel, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheel, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheel, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheel, rearRightWheelTransform);
    }

    // Синхронизация положения WheelCollider с визуальными колесами
    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);  // Получаем данные положения и вращения от WheelCollider

        // Обновляем положение и вращение визуального колеса
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    // Возвращает множитель в зависимости от уровня сложности
    private float GetDifficultyMultiplier()
    {
        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                return 0.75f;  // Медленнее и менее агрессивный бот
            case DifficultyLevel.Medium:
                return 1f;     // Обычный бот
            case DifficultyLevel.Hard:
                return 1.25f;  // Быстрый и агрессивный бот
            default:
                return 1f;
        }
    }
}
