using UnityEngine;


public class SpinningBeacon : MonoBehaviour
{
    [SerializeField] float degreesPerSecond = 220f;

    void Update() => transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.Self);
}
