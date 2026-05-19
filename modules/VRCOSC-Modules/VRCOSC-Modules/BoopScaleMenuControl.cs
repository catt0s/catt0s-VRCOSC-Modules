using CommandLine.Text;
using System;
using System.Drawing.Design;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;

namespace VRCOSC_Modules
{
    [ModuleTitle("BoopScale - Menu")]
    [ModuleDescription("Scale when a contact is booped with controls from an avatar menu")]
    [ModuleType(ModuleType.Generic)]
    public class BoopScaleMenuControl : Module
    {
        private float startHeight;
        private float endHeight;
        private float percent;
        private bool busyScaling;
        private bool shrink;
        private float currentTime;
        private float expectedTime;
        public enum Parameters
        {
            Boop,
            Time,
            Percent,
            Shrink
        }

        protected override void OnPreLoad()
        {
            RegisterParameter<bool>(Parameters.Boop, "Boop", ParameterMode.Read, "Boop Parameter", "True to start a spurt.");
            RegisterParameter<float>(Parameters.Time, "BoopScale/Time", ParameterMode.Read, "Time Parameter", "Float of time taken");
            RegisterParameter<float>(Parameters.Percent, "BoopScale/Percent", ParameterMode.Read, "Percentage Parameter", "Percentage to shrink/grow");
            RegisterParameter<bool>(Parameters.Shrink, "BoopScale/Shrink", ParameterMode.Read, "Shrink Toggle Parameter", "Toggle shrinking");
            base.OnPreLoad();
        }

        protected override Task<bool> OnModuleStart()
        {
            expectedTime = 0f;
            Log("Postload complete");

            busyScaling = false;
            currentTime = 0f;
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
            switch (parameter.Lookup)
            {
                case Parameters.Boop:
                    {
                        if (!busyScaling)
                        {
                            startHeight = GetClient().Avatar.EyeHeight;
                            Log($"StartHeight is {startHeight}");
                            if (percent == 0)
                            {
                                Log("No change");
                                return;
                            }
                            if (percent <= -1f)
                            {
                                percent = -.99999999f;
                                Log("Cannot shrink beyond 100%.");
                            }
                            endHeight = startHeight * (1 + percent);
                            Log($"End height is {endHeight}");
                            busyScaling = true;
                        }
                        break;
                    }
                case Parameters.Percent:
                    {
                        busyScaling = false;
                        percent = parameter.GetValue<float>();
                        break;
                    }
                case Parameters.Time:
                    {
                        busyScaling = false;
                        expectedTime = parameter.GetValue<float>();
                        break;
                    }
                case Parameters.Shrink:
                    {
                        shrink = parameter.GetValue<bool>();
                        if (shrink)
                            percent = -MathF.Abs(percent);
                        else
                            percent = MathF.Abs(percent);
                        break;
                    }
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