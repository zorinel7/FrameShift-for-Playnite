using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FrameShift
{
    /// <summary>
    /// WPF Button compatible with Playnite Fullscreen controller confirmation (A).
    /// </summary>
    public class FrameGamepadButton : Button
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsPlayniteConfirmation(e))
            {
                OnClick();
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
        }

        private static bool IsPlayniteConfirmation(KeyEventArgs e)
        {
            if (e == null)
                return false;

            try
            {
                var eventType = e.GetType();
                if (!string.Equals(eventType.Name, "GameControllerInputEventArgs", StringComparison.Ordinal))
                    return false;

                var buttonProperty = eventType.GetProperty(
                    "Button",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (buttonProperty == null)
                    return false;

                var controllerButton = buttonProperty.GetValue(e, null);
                var gestureType = eventType.Assembly.GetType("Playnite.Input.GameControllerGesture");
                if (gestureType == null)
                    return false;

                var confirmationProperty = gestureType.GetProperty(
                    "ConfirmationBinding",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (confirmationProperty == null)
                    return false;

                var confirmationButton = confirmationProperty.GetValue(null, null);
                return Equals(controllerButton, confirmationButton);
            }
            catch
            {
                return false;
            }
        }
    }
}