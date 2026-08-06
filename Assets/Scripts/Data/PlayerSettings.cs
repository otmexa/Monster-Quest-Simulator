using System;

[Serializable]
public class PlayerSettings
{
    public float masterVolume = 1.0f;
    public float musicVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public int graphicsQualityLevel = 2; // e.g., 0=Low, 1=Medium, 2=High
    public float mouseSensitivity = 1.0f;
}
