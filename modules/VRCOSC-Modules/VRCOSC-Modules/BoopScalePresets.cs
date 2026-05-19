//using CommandLine.Text;
//using System;
//using System.Drawing.Design;
//using System.Threading.Tasks;
//using VRCOSC.App.SDK.Modules;
//using VRCOSC.App.SDK.Parameters;
//using VRCOSC.App.SDK.VRChat;

//namespace VRCOSC_Modules
//{
//    [ModuleTitle("BoopScale - Presets")]
//    [ModuleDescription("Scale when a contact is booped to a preset height")]
//    [ModuleType(ModuleType.Generic)]
//    public class BoopScalePresets : Module
//    {
//        private float startHeight;
//        private float endHeight;
//        private float percent;
//        private bool busyScaling;
//        private bool shrink;
//        private float currentTime;
//        private float expectedTime;
//        public enum Parameters
//        {
//            Boop,
//            Time,
//            Zero
//        }
//        public enum Settings
//        {
//            Preset1,
//            Preset2,
//            Preset3,
//            Preset4,
//            Preset5,
//            Preset6,
//            Preset7,
//            Preset8
//        }

//        protected override void OnPreLoad()
//        {
//            CreateTextBox(Settings.Preset1, "Preset 1 height", "Percentage to shrink to", 85);
//            CreateTextBox(Settings.Preset1, "Preset 2 height", "Percentage to shrink to", 66);
//            CreateTextBox(Settings.Preset1, "Preset 3 height", "Percentage to shrink to", 50);
//            CreateTextBox(Settings.Preset1, "Preset 4 height", "Percentage to shrink to", 35);
//            CreateTextBox(Settings.Preset1, "Preset 5 height", "Percentage to shrink to", 17);
//            CreateTextBox(Settings.Preset1, "Preset 6 height", "Percentage to shrink to", 12);
//            CreateTextBox(Settings.Preset1, "Preset 7 height", "Percentage to shrink to", 1);
//            CreateTextBox(Settings.Preset1, "Preset 8 height", "Percentage to shrink to", "0.1");

//            RegisterParameter<bool>(Parameters.Boop, "Boop", ParameterMode.Read, "Boop Parameter", "True to start a spurt.");
//            RegisterParameter<int>(Parameters.Time, "BoopScale/Time", ParameterMode.Read, "Time Parameter", "Float of time taken");
//            RegisterParameter<bool>(Parameters.Zero, "BoopScale/Zero", ParameterMode.Read, "Reset Parameter", "Sets current height as baseline");
//            base.OnPreLoad();
//        }

//        protected override Task<bool> OnModuleStart()
//        {
//            expectedTime = 3f;
//            Log("Postload complete");

//            busyScaling = false;
//            currentTime = 0f;
//            return base.OnModuleStart();
//        }

//        protected override void OnAvatarChange(Avatar? avatar)
//        {
//            busyScaling = false;
//            currentTime = 0;
//            base.OnAvatarChange(avatar);
//        }

//        protected override void OnRegisteredParameterReceived(RegisteredParameter parameter)
//        {
//            Log("Recieved Parameter");
//            switch (parameter.Lookup)
//            {
//                case Parameters.Boop:
//                    {
//                        if (!busyScaling)
//                        {
//                            startHeight = GetClient().Avatar.EyeHeight;
//                            Log($"StartHeight is {startHeight}");
//                            if (percent == 0)
//                            {
//                                Log("No change");
//                                return;
//                            }
//                            if (percent <= -1f)
//                            {
//                                percent = -.99999999f;
//                                Log("Cannot shrink beyond 100%.");
//                            }
//                            endHeight = startHeight * (1 + percent);
//                            Log($"End height is {endHeight}");
//                            busyScaling = true;
//                        }
//                        break;
//                    }
//                case Parameters.Time:
//                    {
//                        busyScaling = false;
//                        expectedTime = parameter.GetValue<float>();
//                        break;
//                    }
//            }
//                    base.OnRegisteredParameterReceived(parameter);
//            }

//            [ModuleUpdate(ModuleUpdateMode.Custom, false, 20.0)]
//            private void ScalingUpdate()
//            {
//                if (busyScaling)
//                {
//                    currentTime += 20f;
//                    GetClient().Avatar.SetEyeHeight(Single.Lerp(startHeight, endHeight, currentTime / (expectedTime * 1000)));
//                    if (currentTime >= expectedTime * 1000)
//                    {
//                        currentTime = 0;
//                        busyScaling = false;
//                    }

//                }
//            }
//        }
//    }