using CommandLine.Text;
using System;
using System.Drawing.Design;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;

[ModuleTitle("Smooth Scale Trigger")]
[ModuleDescription("Single scaling burst from a trigger")]
[ModuleType(ModuleType.Generic)]
public class SmoothScaleModule : Module
{
    private float startHeight;
    private float endHeight;
    private bool busyScaling;
    private float currentTime;
    private float expectedTime;
    private bool avatarShrink;

    public enum Parameters
    {
        ScaleTrigger,
        ShrinkOption,
        PercentageOption,
        TimeOption
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

        RegisterParameter<bool>(Parameters.ScaleTrigger, "ScaleTrigger/Trigger", ParameterMode.Read, "Trigger Parameter", "True to start a spurt.");
        RegisterParameter<bool>(Parameters.ShrinkOption, "ScaleTrigger/Shrink", ParameterMode.ReadWrite, "WIP: Parameter to set shrinking from menu", "Sending this parameter will set the setting in VRCOSC.");
        RegisterParameter<float>(Parameters.PercentageOption, "ScaleTrigger/Percent", ParameterMode.ReadWrite, "WIP: Parameter to set percentage from menu", "Sending this parameter will set the setting in VRCOSC.");
        RegisterParameter<float>(Parameters.TimeOption, "ScaleTrigger/Time", ParameterMode.ReadWrite, "WIP: Parameter to set time from menu", "Sending this parameter will set the setting in VRCOSC.");
        base.OnPreLoad();
    }

    protected override Task<bool> OnModuleStart()
    {
        expectedTime = GetSettingValue<float>(Settings.Time);
        Log("Postload complete");

        busyScaling = false;
        currentTime = 0;
        SendParameter(Parameters.PercentageOption, GetSettingValue<float>(Settings.Percentage) / 100f);
        SendParameter(Parameters.TimeOption, GetSettingValue<float>(Settings.Time));
        if (GetSettingValue<float>(Settings.Percentage) < 0)
        {
            avatarShrink = true;
            SendParameter(Parameters.ShrinkOption, true);
        }
        else
        {
            avatarShrink = true;
            SendParameter(Parameters.ShrinkOption, true);
        }
        return base.OnModuleStart();
    }

    protected override void OnAvatarChange(Avatar? avatar)
    {
        busyScaling = false;
        currentTime = 0;
        SendParameter(Parameters.PercentageOption, GetSettingValue<float>(Settings.Percentage) / 100);
        SendParameter(Parameters.TimeOption, GetSettingValue<float>(Settings.Time));
        if (GetSettingValue<float>(Settings.Percentage) < 0)
            avatarShrink = true;
        base.OnAvatarChange(avatar);
    }

    protected override void OnRegisteredParameterReceived(RegisteredParameter parameter)
    {
        Log("Recieved Parameter");

        switch (parameter.Lookup)
        {
            case Parameters.ScaleTrigger:
                {
                    if (!busyScaling)
                    {
                        Log("BusyScaling false");
                        startHeight = GetClient().Avatar.EyeHeight;
                        Log($"StartHeight is {startHeight}");
                        if (GetSettingValue<float>(Settings.Percentage) == 0)
                            return;
                        if (GetSettingValue<float>(Settings.Percentage) <= -100f)
                            SetSettingValue<float>(Settings.Percentage, -99.999999f);
                        endHeight = startHeight * (1 + (GetSettingValue<float>(Settings.Percentage) / 100));
                        Log($"EndHeight is {endHeight}");
                        busyScaling = true;
                    }

                    break;
                }
            case Parameters.PercentageOption:
                {
                    if (avatarShrink)
                        SetSettingValue<float>(Settings.Percentage, -parameter.GetValue<float>() * 100);
                    else
                        SetSettingValue<float>(Settings.Percentage, parameter.GetValue<float>() * 100);
                    if (parameter.GetValue<float>() == 0)
                        SetSettingValue<float>(Settings.Percentage, 0.00001f);


                    break;
                }
            case Parameters.TimeOption:
                {
                    SetSettingValue<float>(Settings.Time, parameter.GetValue<float>());
                    if (parameter.GetValue<float>() == 0)
                        SetSettingValue<float>(Settings.Time, .0001f);
                    break;
                }
            case Parameters.ShrinkOption:
                {
                    Log($"Recieved parameter shrink at {parameter.GetValue<bool>()}");
                    avatarShrink = parameter.GetValue<bool>();
                    if (avatarShrink)
                    {
                        SetSettingValue<float>(Settings.Percentage, -MathF.Abs(GetSettingValue<float>(Settings.Percentage)));
                        SendParameter(Parameters.PercentageOption, -MathF.Abs(GetSettingValue<float>(Settings.Percentage) / 100));
                    }
                    else
                    {
                        SetSettingValue<float>(Settings.Percentage, MathF.Abs(GetSettingValue<float>(Settings.Percentage)));
                        SendParameter(Parameters.PercentageOption, MathF.Abs(GetSettingValue<float>(Settings.Percentage) / 100));
                    }

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