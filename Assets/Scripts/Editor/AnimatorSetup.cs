using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

#if UNITY_EDITOR
public class AnimatorSetup : EditorWindow
{
    private AnimatorController controller;
    private GameObject modelPrefab;

    [MenuItem("Window/Animation/Setup Animator")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorSetup>("Animator Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Animator Controller Setup", EditorStyles.boldLabel);

        controller = (AnimatorController)EditorGUILayout.ObjectField(
            "Animator Controller",
            controller,
            typeof(AnimatorController),
            false
        );

        modelPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Model Prefab",
            modelPrefab,
            typeof(GameObject),
            false
        );

        if (GUILayout.Button("Setup Animator"))
        {
            if (controller != null && modelPrefab != null)
            {
                SetupAnimator();
            }
            else
            {
                Debug.LogError("Please assign both Animator Controller and Model Prefab");
            }
        }
    }

    void SetupAnimator()
    {
        // Limpiar el controller
        while (controller.layers[0].stateMachine.states.Length > 0)
        {
            controller.layers[0].stateMachine.RemoveState(
                controller.layers[0].stateMachine.states[0].state
            );
        }

        var rootStateMachine = controller.layers[0].stateMachine;

        // Crear estado Idle
        var idleState = rootStateMachine.AddState("Idle");
        rootStateMachine.defaultState = idleState;

        // Obtener las animaciones del modelo
        string modelPath = AssetDatabase.GetAssetPath(modelPrefab);
        var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath);

        foreach (var obj in clips)
        {
            if (obj is AnimationClip clip)
            {
                // No procesar clips que empiecen con "__"
                if (clip.name.StartsWith("__")) continue;

                // Crear estado para la animación
                var state = rootStateMachine.AddState(clip.name);
                state.motion = clip;

                // Crear parámetro trigger
                string triggerName = clip.name.Replace(" ", "_").Replace("|", "_");
                controller.AddParameter(triggerName, AnimatorControllerParameterType.Trigger);

                // Crear transición desde Any State
                var transition = rootStateMachine.AddAnyStateTransition(state);
                transition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
                transition.duration = 0.25f;
                transition.hasExitTime = false;

                // Crear transición de vuelta a Idle
                var returnTransition = state.AddTransition(idleState);
                returnTransition.hasExitTime = true;
                returnTransition.exitTime = 0.9f;
                returnTransition.duration = 0.1f;

                Debug.Log($"Added state and transitions for: {clip.name}");
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }
}
#endif