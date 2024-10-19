using UnityEngine;

public class CarControl : MonoBehaviour
{
    public float motorForce = 1500f;       // Сила на двигатель
    public float brakeForce = 3000f;       // Сила торможения
    public float maxSteerAngle = 30f;      // Максимальный угол поворота колес
    public Transform centerOfMass;         // Центр масс машины

    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;

    private float horizontalInput;          // Ввод для поворота
    private float verticalInput;            // Ввод для движения вперед/назад
    private float currentSteerAngle;        // Текущий угол поворота
    private float currentBrakeForce;        // Текущая сила торможения
    private bool isBraking;                 // Флаг торможения

    private Rigidbody rb;                   // Rigidbody машины

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition; // Установка центра масс для устойчивости
    }

    private void Update()
    {
        GetInput();            // Получение ввода с клавиатуры
        HandleMotor();         // Управление двигателем
        HandleSteering();      // Управление поворотом
        UpdateWheels();        // Обновление положения и вращения колес
    }

    // Получаем пользовательский ввод
    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);
    }

    // Управляем двигателем и торможением
    private void HandleMotor()
    {
        // Применяем моторную силу
        rearLeftWheel.motorTorque = verticalInput * motorForce;
        rearRightWheel.motorTorque = verticalInput * motorForce;

        currentBrakeForce = isBraking ? brakeForce : 0f;  // Если тормозим, увеличиваем силу тормоза
        ApplyBraking();                                   // Применяем торможение
    }

    // Применение торможения
    private void ApplyBraking()
    {
        frontLeftWheel.brakeTorque = currentBrakeForce;
        frontRightWheel.brakeTorque = currentBrakeForce;
        rearLeftWheel.brakeTorque = currentBrakeForce;
        rearRightWheel.brakeTorque = currentBrakeForce;
    }

    // Управление углом поворота колес
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
}