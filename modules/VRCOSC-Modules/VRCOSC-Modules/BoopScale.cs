using CommandLine.Text;
using System;
using System.Drawing.Design;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;

namespace VRCOSC_Modules
{
    [ModuleTitle("BoopScale")]
    [ModuleDescription("Scale when a contact is booped")]
    [ModuleType(ModuleType.Generic)]
    public class BoopScale : Module
    {
        private float startHeight;
        private float endHeight;
        private bool busyScaling;
        private float currentTime;
        private int expectedTime;
        public enum Parameters
        {
            Boop,
        }
        public enum Settings
        {
            Percentage,
            Time,
        }

        protected override void OnPreLoad()
        {
            CreateTextBox(Settings.Percentage, "Amount to scale in percentage", "e.g. 50 for 50%, also accepts negatives.", 10f);
            CreateTextBox(Settings.Time, "Time to scale", "Time it takes to reach expected height", 2f);

            RegisterParameter<bool>(Parameters.Boop, "Boop", ParameterMode.Read, "Boop Parameter", "True to start a spurt.");
            base.OnPreLoad();
        }

        protected override Task<bool> OnModuleStart()
        {
            expectedTime = GetSettingValue<int>(Settings.Time);
            Log("Postload complete");

            busyScaling = false;
            currentTime = 0;
            return base.OnModuleStart();
        }

        protected override void OnAvatarChange(Avatar? avatar)
        {
            busyScaling = false;
            currentTime = 0;
            base.OnAvatarChange(avatar);
        }

        protected override void OnRegisteredParameterReceived(RegisteredParameter parameter)
        {
            Log("Recieved Parameter");

            if (!busyScaling)
            {
                Log("BusyScaling false");
                startHeight = GetClient().Avatar.EyeHeight;
                Log($"StartHeight is {startHeight}");
                if (GetSettingValue<float>(Settings.Percentage) == 0)
                {
                    Log("No change");
                    return;
                }
                if (GetSettingValue<float>(Settings.Percentage) <= -100f)
                {
                    SetSettingValue<float>(Settings.Percentage, -99.999999f);
                    Log("Cannot shrink beyond 100%.");
                }
                endHeight = startHeight * (1 + (GetSettingValue<float>(Settings.Percentage) / 100));
                Log($"EndHeight is {endHeight}");
                busyScaling = true;
            }
            base.OnRegisteredParameterReceived(parameter);
        }

        [ModuleUpdate(ModuleUpdateMode.Custom, false, 20.0)]
        private void ScalingUpdate()
        {
            if (busyScaling)
            {

                currentTime += 20f;
                GetClient().Avatar.SetEyeHeight(Single.Lerp(startHeight, endHeight, currentTime / (expectedTime * 1000)));
                if (currentTime >= expectedTime * 1000)
                {
                    currentTime = 0;
                    busyScaling = false;
                }

            }
        }
    }
}
