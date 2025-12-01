using UnityEngine;
using UnityEngine.UI;

public class SettingsAudioUI : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Toggle muteToggle;

    private float cachedMasterVolume = 1f;

    private void Start()
    {
        // Ensure sliders are 0–1
        masterSlider.minValue = 0f;
        masterSlider.maxValue = 1f;
        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;

        // --- Load saved values (or defaults) ---
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        bool muted = PlayerPrefs.GetInt("Muted", 0) == 1;

        cachedMasterVolume = master;

        // Set UI to saved values
        masterSlider.value = master;
        musicSlider.value = music;
        muteToggle.isOn = muted;

        // Apply audio state BEFORE wiring listeners
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SetVolume(music);

        if (muted)
            AudioListener.volume = 0f;
        else
            AudioListener.volume = master;

        // Now wire up events
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        muteToggle.onValueChanged.AddListener(OnMuteChanged);
    }

    private void OnMasterChanged(float value)
    {
        cachedMasterVolume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);

        if (!muteToggle.isOn)
            AudioListener.volume = value;
    }

    private void OnMusicChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);

        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SetVolume(value);
    }

    private void OnMuteChanged(bool isOn)
    {
        PlayerPrefs.SetInt("Muted", isOn ? 1 : 0);

        if (isOn)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = cachedMasterVolume;
        }
    }
}