using Oculus.Interaction.Locomotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public Transform settingsRoot;

    [Header("Toggles")]
    public Toggle teleportToggle;
    public Toggle slideToggle;
    public Toggle snapToggle;
    public Toggle smoothToggle;

    [Header("Sliders")]
    public Slider snapAngleSlider;
    public Slider smoothVelocitySlider;
    public Slider vignetteSlider;

    [Header("Textos opcionales")]
    public TMP_Text snapAngleLabel;
    public TMP_Text smoothVelocityLabel;
    public TMP_Text vignetteLabel;

    static readonly string[] vignetteNames = { "Off", "Low", "Medium", "High" };

    MovingSetting moving;
    TurningSetting turning;
    ComfortVignetteSetting[] comfort;

    void Awake()
    {
        moving = settingsRoot.GetComponentInChildren<MovingSetting>(true);
        turning = settingsRoot.GetComponentInChildren<TurningSetting>(true);
        comfort = settingsRoot.GetComponentsInChildren<ComfortVignetteSetting>(true);
    }
    void OnEnable()
    {
        if (moving == null || turning == null) return;

        bool isTeleport = moving.ControllerMovement.Value == MovingSetting.MovementStyle.Teleport;
        if (teleportToggle != null) teleportToggle.SetIsOnWithoutNotify(isTeleport);
        if (slideToggle != null) slideToggle.SetIsOnWithoutNotify(!isTeleport);

        bool isSnap = turning.ControllerTurn.Value == TurningSetting.RotationStyle.Snap;
        if (snapToggle != null) snapToggle.SetIsOnWithoutNotify(isSnap);
        if (smoothToggle != null) smoothToggle.SetIsOnWithoutNotify(!isSnap);

        float snap = turning.RotationSnapAngle.Value;
        if (snapAngleSlider != null) snapAngleSlider.SetValueWithoutNotify(snap);
        if (snapAngleLabel != null) snapAngleLabel.text = Mathf.RoundToInt(snap) + "\u00B0";

        float vel = turning.RotationSmoothVelocity.Value;
        if (smoothVelocitySlider != null) smoothVelocitySlider.SetValueWithoutNotify(vel);
        if (smoothVelocityLabel != null) smoothVelocityLabel.text = Mathf.RoundToInt(vel) + "\u00B0/s";

        if (comfort != null && comfort.Length > 0)
        {
            int level = Mathf.Clamp((int)comfort[0].ComfortLevel.Value, 0, 3);
            if (vignetteSlider != null) vignetteSlider.SetValueWithoutNotify(level);
            if (vignetteLabel != null) vignetteLabel.text = vignetteNames[level];
        }
    }
    public void OnTeleportToggle(bool on)
    {
        if (on) moving.ControllerMovement.Value = MovingSetting.MovementStyle.Teleport;
    }

    public void OnSlideToggle(bool on)
    {
        if (on) moving.ControllerMovement.Value = MovingSetting.MovementStyle.Slide;
    }

    public void OnSnapToggle(bool on)
    {
        if (on) turning.ControllerTurn.Value = TurningSetting.RotationStyle.Snap;
    }

    public void OnSmoothToggle(bool on)
    {
        if (on) turning.ControllerTurn.Value = TurningSetting.RotationStyle.Smooth;
    }

    public void SetSnapAngle(float degrees)
    {
        turning.RotationSnapAngle.Value = degrees;
        if (snapAngleLabel != null) snapAngleLabel.text = Mathf.RoundToInt(degrees) + "\u00B0";
    }

    public void SetSmoothVelocity(float degreesPerSecond)
    {
        turning.RotationSmoothVelocity.Value = degreesPerSecond;
        if (smoothVelocityLabel != null) smoothVelocityLabel.text = Mathf.RoundToInt(degreesPerSecond) + "\u00B0/s";
    }

    public void SetVignetteLevel(float level)
    {
        int i = Mathf.Clamp(Mathf.RoundToInt(level), 0, 3);
        var value = (ComfortVignetteSetting.ComfortAssistance)i;
        foreach (var c in comfort) c.ComfortLevel.Value = value;

        if (vignetteLabel != null) vignetteLabel.text = vignetteNames[i];
    }
}