using Oculus.Interaction.Locomotion;
using UnityEngine;

public class VRJumpCrouch : MonoBehaviour
{
    [SerializeField] private FirstPersonLocomotor _locomotor;

    [Header("Salto")]
    [SerializeField] private OVRInput.Button _jumpButton = OVRInput.Button.One;

    private bool _crouching;

    private void Reset() => _locomotor = GetComponent<FirstPersonLocomotor>();

    private void Update()
    {
        if (_locomotor == null) return;

        if (OVRInput.GetDown(_jumpButton, OVRInput.Controller.RTouch))
            _locomotor.Jump();

        bool pressed = OVRInput.Get(OVRInput.RawButton.RThumbstick);

        if (pressed != _crouching)
        {
            _crouching = pressed;
            _locomotor.Crouch(_crouching);
        }
    }
}