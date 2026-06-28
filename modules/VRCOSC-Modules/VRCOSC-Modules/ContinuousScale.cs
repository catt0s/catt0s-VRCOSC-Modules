using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;
using Windows.Globalization.NumberFormatting;

namespace VRCOSC_Modules
{

    [ModuleTitle("ContinuousScale")]
    [ModuleDescription("Continuous scaling while a contact is active")]
    [ModuleType(ModuleType.Generic)]
    public class ContinuousScale : Module
    {
        public enum Parameters
        {
            Contact,
            AvatarBusy
        }
        enum Settings
        {
            ScaleRate,
        }

        bool contactActive = false;

        public void OnPreload()
        {
            RegisterParameter<bool>(Parameters.Contact, "Headpat", VRCOSC.App.SDK.Parameters.ParameterMode.Read, "Parameter", "Scaling is active while this parameter is true.");
            RegisterParameter<bool>(Parameters.AvatarBusy, "OSCScalingBusy", ParameterMode.ReadWrite, "Scaling parameter", "Parameter indicating that avatar is currently scaling, prevents overlap with other catt0s modules and can be used to play an animation during.");
            CreateTextBox(Settings.ScaleRate, "Scaling rate", "Very roughly percent per second", 5f);
        }

        protected override void OnAvatarChange(Avatar? avatar)
        {
            contactActive = false;
            base.OnAvatarChange(avatar);
        }

        protected override void OnRegisteredParameterReceived(RegisteredParameter parameter)
        {
            switch (parameter.Lookup)
            {
                case Parameters.AvatarBusy:
                    {
                        return;
                    }

                case Parameters.Contact:
                    {
                        try
                        {
                            if (parameter.GetValue<bool>())
                                contactActive = true;
                            else
                                contactActive = false;
                        }
                        catch (Exception e) { Log($"Parameter cast failed: {e}"); }
                        break;
                    }
                    
            }

            base.OnRegisteredParameterReceived(parameter);
        }

        [ModuleUpdate(ModuleUpdateMode.Custom, false, 20.0)]
        private void ScalingUpdate()
        {
            if (contactActive)
                GetClient().Avatar.SetEyeHeight(GetClient().Avatar.EyeHeight * (1f + (GetSettingValue<float>(Settings.ScaleRate) / 50f) ) );
        }
    }
}
