using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using AnimatorAsCode.V0;
using AnimatorAsCodeFramework.Examples;


[CustomEditor(typeof(WorldObjectSetup))]
public class WorldObjectSetupEditor : Editor
{
    private const string SystemName = "WorldObject";

    WorldObjectSetup my;
    AacFlBase aac;

    const HideFlags embedHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;

    static Dictionary<string, Tuple<bool, bool>> axisBinary = new Dictionary<string, Tuple<bool, bool>>
        {
            { "X", (false, false).ToTuple() },
            { "Y", (false, true).ToTuple() },
            { "Z", (true, false).ToTuple() },
            { "R", (true, true).ToTuple() },
        };

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Welcome to the WorldObject setup script! " +
            "The Create button below will add a few parameters and two new layers to your Fx controller. " +
            "It will also create several animations in your animation controller, which will be used to move your object around. " +
            "Once it's set up, feel free to take a look around. Don't worry about breaking anything. " +
            "If your setup is broken, just click the button again to regenerate everything to the correct state.".Trim(), MessageType.None);

        this.DrawDefaultInspector();


        if (GUILayout.Button("Create"))
        {
            Create();
        }
        if (GUILayout.Button("Remove"))
        {
            Remove();
        }
    }

    private void Create()
    {
        my = (WorldObjectSetup)this.target;

        if (my.avatar.customizeAnimationLayers == false || my.avatar.baseAnimationLayers[4].isDefault == true)
        {
            throw new Exception("Avatar descriptor must have a custom Fx layer set");
        }

        aac = AacV0.Create(new AacConfiguration
        {
            SystemName = SystemName,
            AvatarDescriptor = my.avatar,
            AnimatorRoot = my.avatar.transform,
            DefaultValueRoot = my.avatar.transform,
            AssetContainer = (AnimatorController)my.assetContainer,
            AssetKey = "WorldObject",
            DefaultsProvider = new AacDefaultsProvider(writeDefaults: false)
        });
        aac.ClearPreviousAssets();

        var coarse = aac.CreateSupportingFxLayer("Coarse");
        var coarseEmpty = coarse.NewState("Empty");
        var fine = aac.CreateSupportingFxLayer("Fine");
        var fineEmpty = fine.NewState("Empty");

        var posCoarse = coarse.FloatParameter("WO_PosCoarse");
        var posFine = coarse.FloatParameter("WO_PosFine");
        var axis0 = coarse.BoolParameter("WO_Axis0");
        var axis1 = coarse.BoolParameter("WO_Axis1");
        var axisLock = coarse.BoolParameter("WO_AxisLock");

        

        coarseEmpty.TransitionsFromAny()
            .When(axisLock.IsEqualTo(true));

        fineEmpty.TransitionsFromAny()
            .When(axisLock.IsEqualTo(true));

        Transform parent = my.worldObject.transform;
        foreach (var axis in axisBinary)
        {
            var coarseState = coarse.NewState(axis.Key);
            var fineState = fine.NewState(axis.Key);

            coarseState.TransitionsFromAny()
                .When(axis0.IsEqualTo(axis.Value.Item1))
                .And(axis1.IsEqualTo(axis.Value.Item2))
                .And(axisLock.IsEqualTo(false));


            fineState.TransitionsFromAny()
                .When(axis0.IsEqualTo(axis.Value.Item1))
                .And(axis1.IsEqualTo(axis.Value.Item2))
                .And(axisLock.IsEqualTo(false));

            var coarseMover = parent.Find($"{axis.Key}CoarseMover");
            var fineMover = coarseMover.Find($"{axis.Key}FineMover");

            coarseState.WithAnimation(MakeBlendTree(axis.Key, coarseMover.transform, posCoarse, false));
            fineState.WithAnimation(MakeBlendTree(axis.Key, fineMover.transform, posFine, true));

            parent = fineMover;
        }

        if (my.avatar.expressionParameters.CalcTotalCost() > VRCExpressionParameters.MAX_PARAMETER_COST)
        {
            Debug.LogWarning("WorldObject has created additional expression parameters, and now they use too much memory. You will need to delete some.");
        }

        Debug.Log("Done! WorldObject is now set up.");
    }

    BlendTree MakeBlendTree(string axis, Transform mover, AacFlFloatParameter param, bool isFine)
    {
        var unityAxis = axis.ToLower();
        var propName = "m_LocalPosition";
        var range = 1000f;

        if (axis == "R")
        {
            range = 180f;
            unityAxis = "y";
            propName = "localEulerAnglesRaw";
        }

        var scaleFactor = isFine ? (1f / 255) : 1f;

        var clipNeg = aac.NewClip($"{axis}-{(isFine ? "F" : "C")}").Animating(anim => anim.Animates(mover, typeof(Transform),  $"{propName}.{unityAxis}").WithOneFrame(-range * scaleFactor));
        clipNeg.Clip.hideFlags = embedHideFlags;
        var clipPos = aac.NewClip($"{axis}+{(isFine ? "F" : "C")}").Animating(anim => anim.Animates(mover, typeof(Transform), $"{propName}.{unityAxis}").WithOneFrame(range * scaleFactor));
        clipPos.Clip.hideFlags = embedHideFlags;
        var clipZero = aac.NewClip($"{axis}0{(isFine ? "F" : "C")}").Animating(anim => anim.Animates(mover, typeof(Transform), $"{propName}.{unityAxis}").WithOneFrame(0));
        clipZero.Clip.hideFlags = embedHideFlags;

        var blend = aac.NewBlendTreeAsRaw();
        blend.blendType = BlendTreeType.Simple1D;
        blend.blendParameter = param.Name;
        blend.useAutomaticThresholds = false;
        blend.children = new ChildMotion[]
            {
                    new ChildMotion { threshold = -1.0f, motion = clipNeg.Clip, timeScale = 1.0f },
                    new ChildMotion { threshold = -0.0f, motion = clipZero.Clip, timeScale = 1.0f },
                    new ChildMotion { threshold = 1.0f, motion = clipPos.Clip, timeScale = 1.0f },
            };

        return blend;
    }


    private void Remove()
    {
        aac.ClearPreviousAssets();
    }
   
}
