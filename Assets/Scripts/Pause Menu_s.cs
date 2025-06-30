using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu_s : MonoBehaviour
{
    [SerializeField] string nameScene;
    [SerializeField] string url = "https://yourwebsite.com";
    [SerializeField] GameObject panel;
    [SerializeField] Image image;
    [SerializeField] Toggle _toggle;
    [SerializeField] bool _paused;
    [SerializeField] bool _settings = false;
    [SerializeField] bool _toggleClick;
    [SerializeField] Sprite[] sprites;
    [SerializeField] GameObject[] buttonsMenu;
    [SerializeField] GameObject[] buttonsSettings;
    
    private void Start()
    {
        SettingsOnOff();
        if (_toggle != null) { _toggle.isOn = _toggleClick; }
    }
    public void Pause()
    {
        
        _paused = !_paused;
        
    }
    public void Settings()
    {
        _settings = !_settings;
        SettingsOnOff();
    }
    void SettingsOnOff()
    {
        if (_settings)
        {
            if (buttonsMenu != null || buttonsSettings != null)
            {
                for (int i = 0; i < buttonsMenu.Length; i++)
                {
                    buttonsMenu[i].SetActive(false);
                }
                for (int i = 0;i < buttonsSettings.Length; i++)
                {
                    buttonsSettings[i].SetActive(true);
                }
            }
        }
        else if(!_settings)
        {
            if (buttonsMenu != null || buttonsSettings != null)
            {
                for (int i = 0; i < buttonsMenu.Length; i++)
                {
                    buttonsMenu[i].SetActive(true);
                }
                for (int i = 0; i < buttonsSettings.Length; i++)
                {
                    buttonsSettings[i].SetActive(false);
                }
            }
        }
    }
    private void Update()
    {
        if (_paused && panel != null)
        {
            panel.SetActive(true);Time.timeScale = 0;
            if (image != null && sprites != null)
            {
                image.sprite = sprites[0];
            }
        }
        else if (!_paused && panel != null) 
        { 
            panel.SetActive(false); Time.timeScale = 1.0f;
            if (image != null && sprites != null)
            {
                image.sprite = sprites[1];
            }
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(nameScene);
    }

    public void ExitGame()
    {
        Application.OpenURL(url);
    }
}
