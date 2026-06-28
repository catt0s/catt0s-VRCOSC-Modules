using CommandLine.Text;
using FastOSC;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsWPF;
using System;
using System.Drawing.Design;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Settings;
using Windows.Media.Audio;

namespace VRCOSC_Modules
{
    [ModuleTitle("BoopScale - Menu")]
    [ModuleDescription("Scale when a contact is booped with controls from an avatar menu")]
    [ModuleType(ModuleType.Generic)]
    public class BoopScaleMenuControl : Module
    {

        //VARIABLES

        private float startHeight;
        private float endHeight;
        private TimeSpan timeTotal;
        private bool scalingActive = false;
        private DateTime startTime;
        private TimeSpan timeCurrent = TimeSpan.Zero;
        private bool initialized;

        public enum Parameters
        {
            Boop,
            Time,
            Percent,
            Shrink,
            AvatarBusy
        }

        public enum Settings
        {
            Percentage,
            Time
        }


        //INITIALIZE


        protected override void OnPreLoad()
        {
            RegisterParameter<bool>(Parameters.Boop, "Boop", ParameterMode.Read, "Boop Parameter", "True to start a spurt.");
            RegisterParameter<float>(Parameters.Time, "BoopScale/Time", ParameterMode.ReadWrite, "Time Parameter", "Float of time taken");
            RegisterParameter<float>(Parameters.Percent, "BoopScale/Percent", ParameterMode.ReadWrite, "Percentage Parameter", "Percentage to shrink/grow");
            RegisterParameter<bool>(Parameters.Shrink, "BoopScale/Shrink", ParameterMode.ReadWrite, "Shrink Toggle Parameter", "Toggle shrinking");
            RegisterParameter<bool>(Parameters.AvatarBusy, "OSCScalingBusy", ParameterMode.ReadWrite, "Scaling parameter", "Parameter indicating that avatar is currently scaling, prevents overlap with other catt0s modules and can be used to play an animation during.");

            CreateTextBox(Settings.Percentage, "Amount to scale in percentage", "e.g. 50 for 50%, also accepts negatives.", 10f);
            CreateTextBox(Settings.Time, "Time to scale", "Time it takes to reach expected height", 2f);

            GetSetting(Settings.Percentage).OnSettingChange += UpdateAvatarParams;
            GetSetting(Settings.Time).OnSettingChange += UpdateAvatarParams;
            initialized = false;

            base.OnPreLoad();
        }

        protected override async Task<bool> OnModuleStart()
        {
            int retries = 0;
            while (GetClient().Avatar == null)
            {
                retries++;
                Log($"Failed to get avatar in {retries} attempts, retrying in one second...");
                await Task.Delay(1000);
                if (retries >= 10)
                {
                    Log("Failed to initialize values, please set all values on your avatar once!");
                    return true;
                }
            }
            UpdateAvatarParams();
            return true;
        }


        //RUNNING


        protected override void OnAvatarChange(Avatar? avatar)
        {
            UpdateAvatarParams();
            scalingActive = false;

            base.OnAvatarChange(avatar);
        }

        protected override async void OnRegisteredParameterReceived(RegisteredParameter parameter)
        {
            switch (parameter.Lookup)
            {
                case Parameters.Boop:
                    {
                        Log("Boop!");
                        StartScaling((float)GetSettingValue<float>(Settings.Percentage) / 100f, TimeSpan.FromSeconds(GetSettingValue<float>(Settings.Time)));
                        break;
                    }
                case Parameters.Percent:
                    {
                        if ((await FindParameter(Parameters.Shrink)).GetValue<bool>())
                        {
                            GetSetting(Settings.Percentage).OnSettingChange -= UpdateAvatarParams;
                            SetSettingValue<float>(Settings.Percentage, -parameter.GetValue<float>() * 100f);
                            GetSetting(Settings.Percentage).OnSettingChange += UpdateAvatarParams;
                        }
                        else
                        {
                            GetSetting(Settings.Percentage).OnSettingChange -= UpdateAvatarParams;
                            SetSettingValue<float>(Settings.Percentage, parameter.GetValue<float>() * 100f);
                            GetSetting(Settings.Percentage).OnSettingChange += UpdateAvatarParams;
                        }
                        LogDebug($"Set percent to {GetSettingValue<float>(Settings.Time)}.");
                        break;
                    }
                case Parameters.Time:
                    {
                        timeTotal = TimeSpan.FromSeconds(parameter.GetValue<float>());
                        SetSettingValue<float>(Settings.Time, (float)timeTotal.TotalSeconds);
                        LogDebug($"Set time to {parameter.GetValue<float>()} seconds.");
                        break;
                    }
                case Parameters.Shrink:
                    {
                        if (parameter.GetValue<bool>())
                        {
                            SetSettingValue<float>(Settings.Percentage, -MathF.Abs(GetSettingValue<float>(Settings.Percentage)));
                        }
                        else
                        {
                            SetSettingValue<float>(Settings.Percentage, MathF.Abs(GetSettingValue<float>(Settings.Percentage)));
                        }

                        LogDebug($"Set shrink to {parameter.GetValue<bool>()}.");
                        LogDebug($"Set percent to {GetSettingValue<float>(Settings.Percentage)}.");
                        break;
                    }
            }

            base.OnRegisteredParameterReceived(parameter);
        }

        private void UpdateAvatarParams()
        {
            timeTotal = TimeSpan.FromSeconds(GetSettingValue<float>(Settings.Time));
            if (GetSettingValue<float>(Settings.Percentage) < 0f)
                SendParameterAndWait(Parameters.Shrink, true);
            else SendParameterAndWait(Parameters.Shrink, false);

            SendParameterAndWait(Parameters.Percent, MathF.Abs(GetSettingValue<float>(Settings.Percentage) / 100f), true);
            SendParameterAndWait(Parameters.Time, (float)(timeTotal.TotalSeconds), true);

            LogDebug("Updated parameters");
        }


        //UPDATE


        async private void StartScaling(float percent, TimeSpan time)
        {
            VRChatParameter? t = await FindParameter(Parameters.AvatarBusy);
            if (t != null && t.GetValue<bool>() == true)
            {
                LogDebug("Scaling is busy. Try again later!");
                return;
            }

            startTime = DateTime.Now;

            startHeight = GetClient().Avatar.EyeHeight;
            LogDebug($"StartHeight is {startHeight}");

            if (percent == 0)
            {
                Log("No change");
                return;
            }
            if (percent <= -1f)
            {
                percent = -.99999f;
                Log("Cannot shrink beyond 100%.");
            }
            endHeight = startHeight * (1 + percent);
            LogDebug($"End height is {endHeight}");

            scalingActive = true;
            if (t != null)
                SendParameterAndWait(Parameters.AvatarBusy, true);

            LogDebug("Started scaling");
        }

        [ModuleUpdate(ModuleUpdateMode.Custom, false, 20.0)]
        async private void ScalingUpdate()
        {
            if (scalingActive)
            {
                timeCurrent = DateTime.Now - startTime;
                GetClient().Avatar.SetEyeHeight(Single.Lerp(startHeight, endHeight, (float)(timeCurrent / timeTotal)));
                if (timeCurrent >= timeTotal)
                {
                    scalingActive = false;
                    SendParameterAndWait(Parameters.AvatarBusy, false);
                    LogDebug("Finished scaling");
                }
            }
        }

    }
}