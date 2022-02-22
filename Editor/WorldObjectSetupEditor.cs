using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

[CustomEditor(typeof(WorldObjectSetup))]
public class WorldObjectSetupEditor : Editor
{
    string range = "1000";
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Use this GUI to set up your WorldObject. You will need to remove this script before uploading.", MessageType.Warning);

        EditorGUILayout.HelpBox("Welcome to the WorldObject setup script! " +
            "The button below will add a few parameters and two new layers to your Fx controller. " +
            "It will also create several animations in your animation controller, which will be used to move your object around. " +
            "Once it's set up, feel free to take a look around. Don't worry about breaking anything. " +
            "If your setup is broken, just click the button again to regenerate everything to the correct state.\n\n" +
            "--Oli__".Trim(), MessageType.None);

        range = EditorGUILayout.TextField("Maximum range (meters)", range);

        if (GUILayout.Button("Build Animator"))
        {
            GenerateAnimatorLayers(float.Parse(range));
        }
    }

    void GenerateAnimatorLayers(float range)
    {
        WorldObjectSetup target = (WorldObjectSetup)this.target;
        GameObject avatar = target.gameObject;

        VRCAvatarDescriptor avatarDescriptor = avatar.GetComponent<VRCAvatarDescriptor>();

        avatarDescriptor.customizeAnimationLayers = true;

        if (avatarDescriptor.customizeAnimationLayers == false || avatarDescriptor.baseAnimationLayers[4].isDefault == true)
        {
            throw new Exception("Avatar descriptor must have a custom Fx layer set");
        }

        AnimatorController fxLayer = avatarDescriptor.baseAnimationLayers[4].animatorController as AnimatorController;

        EnsureParameter(fxLayer, "WorldAxis0", AnimatorControllerParameterType.Bool);
        EnsureParameter(fxLayer, "WorldAxis1", AnimatorControllerParameterType.Bool);
        EnsureParameter(fxLayer, "WorldAxisLock", AnimatorControllerParameterType.Bool);
        EnsureParameter(fxLayer, "WorldPosCoarse", AnimatorControllerParameterType.Float);
        EnsureParameter(fxLayer, "WorldPosFine", AnimatorControllerParameterType.Float);

        var coarse = new AnimatorControllerLayer
        {
            name = "WorldSpaceCoarse",
            defaultWeight = 1.0f,
            stateMachine = GenerateStateMachine(fxLayer, false, range)
        };

        var fine = new AnimatorControllerLayer
        {
            name = "WorldSpaceFine",
            defaultWeight = 1.0f,
            stateMachine = GenerateStateMachine(fxLayer, true, range / 255.0f)

        };

        var coarseIdx = Array.FindIndex(fxLayer.layers, layer => layer.name == "WorldSpaceCoarse");

        if (coarseIdx != -1) { fxLayer.RemoveLayer(coarseIdx); }
        fxLayer.AddLayer(coarse);

        var fineIdx = Array.FindIndex(fxLayer.layers, layer => layer.name == "WorldSpaceFine");
        if (fineIdx != -1) { fxLayer.RemoveLayer(fineIdx); }
        fxLayer.AddLayer(fine);
    }

    static string[] axes = { "X", "Y", "Z", "R" };
    static Dictionary<string, Tuple<bool, bool>> axisBinary = new Dictionary<string, Tuple<bool, bool>>
        {
            { "X", (false, false).ToTuple() },
            { "Y", (false, true).ToTuple() },
            { "Z", (true, false).ToTuple() },
            { "R", (true, true).ToTuple() },
        };
    const HideFlags embedHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
    AnimatorStateMachine GenerateStateMachine(AnimatorController controller, bool isFine, float range)
    {
        var existingAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(controller));
        string coarseStr = isFine ? "Fine" : "Coarse";
        var stateMachine = new AnimatorStateMachine
        {
            name = $"WO_{(isFine ? "Fine" : "Coarse")}",
            hideFlags = embedHideFlags
        };
        AssetDatabase.AddObjectToAsset(stateMachine, controller);


        stateMachine.defaultState = stateMachine.AddState("Empty");
        stateMachine.defaultState.writeDefaultValues = false;
        var emptyStateTransition = stateMachine.AddAnyStateTransition(stateMachine.defaultState);
        emptyStateTransition.AddCondition(AnimatorConditionMode.If, 1.0f, "WorldAxisLock");
        emptyStateTransition.duration = 0.0f;

        var path = "WorldObject";

        foreach (var axis in axes)
        {
            var unityAxis = axis;
            var propName = "localPosition";

            path = $"{path}/{axis}Mover";
            if (isFine) { path = $"{path}/{axis}FineMover"; }

            if (axis == "R") {
                range = isFine ? (180.0f / 255.0f) : 180.0f;
                unityAxis = "Y";
                propName = "localRotation";
            }

            var clipPos = GetOrCreateClip(controller, existingAssets, $"WO_{axis}+{(isFine ? "Fine" : "Coarse")}");
            clipPos.SetCurve(path, typeof(Transform), $"{propName}.{unityAxis.ToLower()}", AnimationCurve.Constant(0, 0, range));
            var clipNeg = GetOrCreateClip(controller, existingAssets, $"WO_{axis}-{(isFine ? "Fine" : "Coarse")}");
            clipNeg.SetCurve(path, typeof(Transform), $"{propName}.{unityAxis.ToLower()}", AnimationCurve.Constant(0, 0, -range));
            var clipZero = GetOrCreateClip(controller, existingAssets, $"WO_{axis}0{(isFine ? "Fine" : "Coarse")}");
            clipZero.SetCurve(path, typeof(Transform), $"{propName}.{unityAxis.ToLower()}", AnimationCurve.Constant(0, 0, 0));

            var axisState = stateMachine.AddState(axis);
            axisState.writeDefaultValues = false;
            axisState.motion = new BlendTree
            {
                name = $"WorldPos{axis}{coarseStr}",
                blendParameter = $"WorldPos{coarseStr}",
                blendType = BlendTreeType.Simple1D,
                useAutomaticThresholds = false,
                children = new ChildMotion[]
                {
                    new ChildMotion { threshold = -1.0f, motion = clipNeg, timeScale = 1.0f },
                    new ChildMotion { threshold = -0.0f, motion = clipZero, timeScale = 1.0f },
                    new ChildMotion { threshold = 1.0f, motion = clipPos, timeScale = 1.0f },
                }
            };

            var stateTransition = stateMachine.AddAnyStateTransition(axisState);
            stateTransition.duration = 0.0f;
            var axisBits = axisBinary[axis];
            stateTransition.AddCondition(BoolToConditionMode(axisBits.Item1), 1.0f, "WorldAxis0");
            stateTransition.AddCondition(BoolToConditionMode(axisBits.Item2), 1.0f, "WorldAxis1");
            stateTransition.AddCondition(AnimatorConditionMode.IfNot, 1.0f, "WorldAxisLock");

            if (!isFine)
            {
                path = $"{path}/{axis}FineMover";
            }
        }
        return stateMachine;
    }

    AnimatorConditionMode BoolToConditionMode(bool v)
    {
        if (v)
            return AnimatorConditionMode.If;
        else
            return AnimatorConditionMode.IfNot;
    }

    void EnsureParameter(AnimatorController controller, string paramName, AnimatorControllerParameterType paramType)
    {
        var param = Array.FindIndex(controller.parameters, p => p.name == paramName);
        if (controller.parameters[param].type != paramType)
        {
            controller.RemoveParameter(param);
            param = -1;
        }

        if (param == -1)
        {
            controller.AddParameter(paramName, paramType);
        }
    }

    AnimationClip GetOrCreateClip(AnimatorController controller, UnityEngine.Object[] existingAssets, string name)
    {
        var path = AssetDatabase.GetAssetPath(controller);

        var clip = Array.Find<UnityEngine.Object>(existingAssets, asset => asset.name == name && asset.GetType() == typeof(AnimationClip)) as AnimationClip;
        if (clip == null)
        {
            clip = new AnimationClip() { name = name };
            AssetDatabase.AddObjectToAsset(clip, controller);
        }
        clip.ClearCurves();

        clip.hideFlags = embedHideFlags;
        return clip;
    }
}
