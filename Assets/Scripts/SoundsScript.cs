using UnityEngine;
using UnityEngine.UI;

public class SoundsScript : MonoBehaviour
{
    [SerializeField] AudioListener audioListener;
    [SerializeField] Image image;
    [SerializeField] Sprite[] sprite;

    void Start()
    {
        if (audioListener != null)
        {
            CheckPersistence(audioListener.gameObject);
        }
    }
    void CheckPersistence(GameObject obj)
    {
        if (obj.scene.rootCount == 0)
        {
            Debug.LogWarning($"⚠️ Объект {obj.name} является persistent!");
        }
    }
    public void Mute()
    {
        if (audioListener != null)
        {
            bool isMuted = !AudioListener.pause;
            AudioListener.pause = isMuted;

            image.sprite = isMuted ? sprite[1] : sprite[0];
        }
    }

}
