using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;



[RequireComponent(typeof(Collider))]
public class ExitDoor : MonoBehaviour
{
    [Tooltip("Scene to load when the player exits. Leave empty to only fire the event.")]
    [SerializeField] string nextSceneName;

    public UnityEvent onPlayerExited;

    public string NextSceneName { get => nextSceneName; set => nextSceneName = value; }

    bool used;

    void Reset() => GetComponent<Collider>().isTrigger = true;

    void OnTriggerEnter(Collider other)
    {
        
        if (used || other.GetComponentInParent<CharacterController>() == null) return;
        used = true;

        Debug.Log("Player reached the exit.", this);
        onPlayerExited?.Invoke();
        if (!string.IsNullOrEmpty(nextSceneName)) SceneManager.LoadScene(nextSceneName);
    }
}
