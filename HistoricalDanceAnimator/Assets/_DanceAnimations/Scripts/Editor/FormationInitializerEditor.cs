using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(FormationInitializer))]
public class FormationInitializerEditor : Editor
{
    private FormationInitializer initializer;
    private AnimatorController animatorController;
    private AnimatorOverrideController overrideController;
    private Dictionary<AnimationClip, AnimationClip> overrideDictionary;

    private void OnEnable()
    {
        initializer = (FormationInitializer)target;

        if (initializer.animator == null)
        {
            Debug.Log($"ERROR: No {nameof(Animator)} found!", target);
            return;
        }

        if (initializer.animator.runtimeAnimatorController == null)
        {
            Debug.Log($"ERROR: No {nameof(AnimatorController)} found on {nameof(Animator)}!", initializer.animator);
            return;
        }

        overrideDictionary = new Dictionary<AnimationClip, AnimationClip>();

        GetControllers();

    }

    private void GetControllers()
    {
        Object assetObject = GetControllerAsset(initializer.animator.runtimeAnimatorController);

        if (assetObject is AnimatorOverrideController)
        {
            overrideController = (AnimatorOverrideController)assetObject;
            assetObject = GetControllerAsset(overrideController.runtimeAnimatorController);

            List<KeyValuePair<AnimationClip, AnimationClip>> keyValuePairs = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
            overrideController.GetOverrides(keyValuePairs);

            foreach (KeyValuePair<AnimationClip, AnimationClip> pair in keyValuePairs)
            {
                overrideDictionary.Add(pair.Key, pair.Value);
            }
        }

        if (assetObject is AnimatorController)
        {
            animatorController = (AnimatorController)assetObject;
            return;
        }

        Debug.Log($"ERROR: Unhandled asset type {assetObject.GetType()} found!", assetObject);
    }

    private Object GetControllerAsset(RuntimeAnimatorController runtimeAnimatorController)
    {
        string assetPath = AssetDatabase.GetAssetPath(runtimeAnimatorController);
        Object assetObject = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

        if (assetObject != null)
            Debug.Log($"Found {assetObject.GetType()} '{assetObject.name}'", assetObject);
        else
            Debug.Log($"ERROR: No asset found at path '{assetPath}'");

        return assetObject;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (animatorController == null)
            return;

        GUILayout.Label("Animator States");

        AnimatorControllerLayer la = animatorController.layers[0];
        Vector2 buttonDimensions = CalculateButtonDimensions(la.stateMachine.states);

        float marginHorizontal = 40f; // Inspector width (Screen.width) doesn't calculate away things like indentation and layouting margins
        int h = Mathf.FloorToInt((Screen.width - marginHorizontal) / buttonDimensions.x);

        int x = 0;
        int y = 0;

        AnimationClip clip, overrideClip;
        foreach (ChildAnimatorState cas in la.stateMachine.states)
        {
            if (x == 0)
                GUILayout.BeginHorizontal();

            clip = null;

            if (overrideController != null)
            {
                if (cas.state.motion is AnimationClip)
                {
                    clip = (AnimationClip)cas.state.motion;

                    if (overrideDictionary.TryGetValue(clip, out overrideClip))
                    {
                        clip = overrideClip;
                    }
                }
            }

            if (GUILayout.Button(cas.state.name, GUILayout.Width(buttonDimensions.x), GUILayout.Height(buttonDimensions.y)))
            {
                if (clip != null)
                {
                    Debug.Log($"Applying clip '{clip.name}' properties, from state '{cas.state.name}'");
                    ApplyClipProperties(clip);
                }
            }

            x++;

            if (x >= h)
            {
                x = 0;
                y++;
                GUILayout.EndHorizontal();
            }
        }

        if (x > 0)
        {
            GUILayout.EndHorizontal();
        }
    }

    private Vector2 CalculateButtonDimensions(ChildAnimatorState[] states)
    {
        Vector2 buttonDimensions;
        float buttonWidth = 80f;
        float buttonHeight = 20f;

        foreach (ChildAnimatorState cas in states)
        {
            buttonDimensions = GUI.skin.button.CalcSize(new GUIContent(cas.state.name));

            if (buttonDimensions.x > buttonWidth)
                buttonWidth = buttonDimensions.x;
            if (buttonDimensions.y > buttonHeight)
                buttonHeight = buttonDimensions.y;
        }

        return new Vector2(buttonWidth, buttonHeight);
    }

    private void ApplyClipProperties(AnimationClip clip)
    {
        clip.SampleAnimation(initializer.gameObject, 0f);
    }
}
