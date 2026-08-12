//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Documents;
//using VRCOSC.App.Nodes.Types.Events;
//using VRCOSC.App.SDK.Modules;
//using VRCOSC.App.SDK.Parameters;

//namespace VRCOSC_Modules
//{
//    [ModuleTitle("SizeRemoteModule")]
//    [ModuleDescription("Companion OSC for the remote prefab.")]
//    [ModuleType(ModuleType.Generic)]

//    public class SizeRemoteModule : Module
//    {
//        public enum Parameters
//        {
//            Shrink,
//            Grow,
//            Lock,
//            Bit1,
//            Bit2,
//            Bit3,
//            Safety,
//            Key,
//            AvatarBusy
//        }
//        public enum Settings
//        {
//            LowerLimit,
//            UpperLimit,
//            SafeSize,
//            Key
//        }

//        ushort power;
//        bool scalingActive = false;
//        bool avatarScalingBusy = false;
//        private DateTime startTime;
//        private TimeSpan currentTime = TimeSpan.Zero;



//        protected override void OnPreLoad()
//        {
//            RegisterParameter<bool>(Parameters.Shrink, "SizeRemote/Shrink", ParameterMode.Read, "Shrink Trigger", "Shrink trigger parameter");
//            RegisterParameter<bool>(Parameters.Grow, "SizeRemote/Grow", ParameterMode.Read, "Grow Trigger", "Grow trigger parameter");
//            RegisterParameter<bool>(Parameters.Bit1, "SizeRemote/Bit1", ParameterMode.Read, "Power bit 1", "Power communication parameter");
//            RegisterParameter<bool>(Parameters.Bit2, "SizeRemote/Bit2", ParameterMode.Read, "Power bit 2", "Power communication parameter");
//            RegisterParameter<bool>(Parameters.Bit3, "SizeRemote/Bit3", ParameterMode.Read, "Power bit 3", "Power communication parameter");
//            RegisterParameter<bool>(Parameters.Safety, "SizeRemote/Safety", ParameterMode.Read, "Safe Trigger", "Resets size to a set safe size, used to prevent unusable UI");
//            RegisterParameter<bool>(Parameters.Key, "SizeRemote/Key", ParameterMode.Read, "Key name", "Sets the key required if enabled");
//            RegisterParameter<bool>(Parameters.AvatarBusy, "OSCScalingBusy", ParameterMode.ReadWrite, "Scaling parameter", "Parameter indicating that avatar is currently scaling, prevents overlap with other catt0s modules and allows for an animation to play during.");

//            CreateTextBox(Settings.UpperLimit, "Maximum size", "Maximum size you can be set to, limit is 10000", "3000");
//            CreateTextBox(Settings.LowerLimit, "Minimum size", "Minimum size you can be set to, limit is 0.01", "0.075");
//            CreateTextBox(Settings.SafeSize, "Safety reset size", "Size to reset to when safety is used, if installed", "2");
//            CreateToggle(Settings.Key, "Key", "Check for the matching key, allows for whitelisting who can trigger.", false);

//            base.OnPreLoad();
//        }

//        protected override Task<bool> OnModuleStart()
//        {

//            return base.OnModuleStart();
//        }
//        protected override void OnRegisteredParameterReceived(RegisteredParameter parameter)
//        {
//            switch (parameter.Lookup)
//            {
//                case Parameters.Grow:
//                    {

//                        break;
//                    }

//                case Parameters.Shrink:
//                    {

//                        break;
//                    }

//                case Parameters.Safety:
//                    {

//                        break;
//                    }

//                case Parameters.Key:
//                    {

//                        break;
//                    }
//            }


//        }

//        protected override void OnAnyParameterReceived(VRChatParameter parameter)
//        {

//            if (parameter.Name == "/avatar/eyeheight")

//                base.OnAnyParameterReceived(parameter);
//        }


//        //UPDATE


//        async private void StartScaling(float percent, TimeSpan time)
//        {
//            VRChatParameter? t = await FindParameter(Parameters.AvatarBusy);
//            if (t != null && t.GetValue<bool>() == true)
//            {
//                LogDebug("Scaling is busy. Try again later!");
//                return;
//            }

//            startTime = DateTime.Now;

//            startHeight = GetClient().Avatar.EyeHeight;
//            LogDebug($"StartHeight is {startHeight}");

//            if (percent == 0)
//            {
//                Log("No change");
//                return;
//            }
//            if (percent <= -1f)
//            {
//                percent = -.99999f;
//                Log("Cannot shrink beyond 100%.");
//            }
//            endHeight = startHeight * (1 + percent);
//            LogDebug($"End height is {endHeight}");

//            scalingActive = true;
//            if (t != null)
//                SendParameterAndWait(Parameters.AvatarBusy, true);

//            LogDebug("Started scaling");
//        }

//        [ModuleUpdate(ModuleUpdateMode.Custom, false, 20.0)]
//        async private void ScalingUpdate()
//        {
//            if (scalingActive)
//            {
//                timeCurrent = DateTime.Now - startTime;
//                GetClient().Avatar.SetEyeHeight(Single.Lerp(startHeight, endHeight, (float)(timeCurrent / timeTotal)));
//                if (timeCurrent >= timeTotal)
//                {
//                    scalingActive = false;
//                    SendParameterAndWait(Parameters.AvatarBusy, false);
//                    LogDebug("Finished scaling");
//                }
//            }
//        }
//    }
//}
