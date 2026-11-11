using Oculus.Interaction.Locomotion;
using UnityEngine;

public class ToggleMovementStyle : MonoBehaviour
{
    public MovingSetting movingSetting;
    public void Toggle()
    {
        var current = movingSetting.ControllerMovement.Value;
        movingSetting.ControllerMovement.Value =
            current == MovingSetting.MovementStyle.Teleport
                ? MovingSetting.MovementStyle.Slide
                : MovingSetting.MovementStyle.Teleport;
    }

    public void SetSlide()
    {
        movingSetting.ControllerMovement.Value = MovingSetting.MovementStyle.Slide;
    }

    public void SetTeleport()
    {
        movingSetting.ControllerMovement.Value = MovingSetting.MovementStyle.Teleport;
    }
}