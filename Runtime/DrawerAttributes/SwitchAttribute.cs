using System;
using UnityEngine;

namespace THEBADDEST
{
    /// <summary>
    /// Displays a bool field as a modern toggle switch instead of a checkbox
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SwitchAttribute : DrawerAttribute
    {
        /// <summary>
        /// Color of the switch track when ON (default: light purple)
        /// </summary>
        public Color OnColor { get; private set; }

        /// <summary>
        /// Color of the switch track when OFF (default: black)
        /// </summary>
        public Color OffColor { get; private set; }

        /// <summary>
        /// Color of the handle (default: white/light when off, black when on)
        /// </summary>
        public Color HandleColor { get; private set; }

        /// <summary>
        /// Whether to show "True"/"False" text next to the switch
        /// </summary>
        public bool ShowText { get; private set; }

        public SwitchAttribute(
            bool showText = true,
            float onColorR = 0.75f, float onColorG = 0.55f, float onColorB = 0.95f, float onColorA = 1f,
            float offColorR = 0.15f, float offColorG = 0.15f, float offColorB = 0.15f, float offColorA = 1f)
        {
            ShowText = showText;
            OnColor = new Color(onColorR, onColorG, onColorB, onColorA);
            OffColor = new Color(offColorR, offColorG, offColorB, offColorA);
            HandleColor = Color.white;
        }

        public SwitchAttribute(
            bool showText,
            Color onColor,
            Color offColor)
        {
            ShowText = showText;
            OnColor = onColor;
            OffColor = offColor;
            HandleColor = Color.white;
        }
    }
}

