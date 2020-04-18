using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArrowFlightSettings", menuName = "ScriptableObjects/Settings/ArrowFlightSettings", order = 1)]
public class ArrowFlightSettings : ScriptableObject {

    [Header("Arrow flight settings")]
    
    #region SerializedFields
    
    [SerializeField] private float m_arrowSpeed = 10f;
    
    [SerializeField] private float m_arrowStartOffset = 10f;

    [Tooltip("The ballistic curve of the arrows flight")]
    [SerializeField] private AnimationCurve m_arrowFlightCurve = new AnimationCurve(
        new[] {
            new Keyframe(0.0f, 0f),
            new Keyframe(1.0f, 0f),
        });

    [Tooltip("Maximal random angle given to the ballistic curve")]
    [SerializeField] private float m_maximalTrajectoryDeviationAngle = 0f;
    
    [Tooltip("Added flight range given to the arrow if the target was missed")]
    [SerializeField] private float m_missExtendedFlightLength = 20f;
    
    #endregion
    
    #region PublicFields
    
    public float ArrowSpeed => m_arrowSpeed;
    
    public float ArrowStartOffset => m_arrowStartOffset;

    public AnimationCurve ArrowFlightCurve => m_arrowFlightCurve;

    public float MaximalTrajectoryDeviationAngle => m_maximalTrajectoryDeviationAngle;
    
    public float MissExtendedFlightLength => m_missExtendedFlightLength;
    
    #endregion
    
    #region PublicMethods
    
#if UNITY_EDITOR
    public static ArrowFlightSettings Load() {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ArrowFlightSettings");
        if (guids.Length == 0) {
            Debug.LogWarning("Could not find ArrowFlightSettings asset. Will use default settings instead.");
            return ScriptableObject.CreateInstance<ArrowFlightSettings>();
        } else {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<ArrowFlightSettings>(path);
        }
    }
#endif
    
    #endregion
}