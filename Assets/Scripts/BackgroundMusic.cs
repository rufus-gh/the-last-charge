using UnityEngine;




[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    static BackgroundMusic instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        var source = GetComponent<AudioSource>();
        source.loop = true;
        source.spatialBlend = 0f; 
        source.ignoreListenerPause = true;
        if (!source.isPlaying) source.Play();
    }
}
