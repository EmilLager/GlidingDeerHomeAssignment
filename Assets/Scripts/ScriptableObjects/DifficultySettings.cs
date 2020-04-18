using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DifficultySettings", menuName = "ScriptableObjects/Settings/DifficultySettings", order = 2)]
public class DifficultySettings : ScriptableObject {

    #region SerializedFields
    
    [Header("Difficulty settings")]
    
    [SerializeField] private float m_barIndicatorSpeed = 1f;
    
    [Header("Bar timing settings")]
    [SerializeField] private List<BarTimingZone> m_barTimings;
    
    #endregion
    
    #region PublicFields
    
    public float BarIndicatorSpeed => m_barIndicatorSpeed;
    
    #endregion
    
    #region PublicMethods

    public TargetAreas GetTargetArea(float barPosition) {

        float centeredValue = Mathf.Abs(barPosition - 0.5f);
        
        for (int i = 0; i < m_barTimings.Count; i++) {
            if (centeredValue > m_barTimings[i].MinTiming && centeredValue <= m_barTimings[i].MaxTiming) {
                return m_barTimings[i].Area;
            }
        }
        return TargetAreas.Miss;
    }
    
    public Color GetTargetAreaColor(float barPosition) {

        float centeredValue = Mathf.Abs(barPosition - 0.5f);
        
        for (int i = 0; i < m_barTimings.Count; i++) {
            if (centeredValue > m_barTimings[i].MinTiming && centeredValue <= m_barTimings[i].MaxTiming) {
                return m_barTimings[i].IndicatorColor;
            }
        }
        return Color.gray;
    }

#if UNITY_EDITOR
    public static DifficultySettings Load() {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:DifficultySettings");
        if (guids.Length == 0) {
            Debug.LogWarning("Could not find DifficultySettings asset. Will use default settings instead.");
            return ScriptableObject.CreateInstance<DifficultySettings>();
        } else {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<DifficultySettings>(path);
        }
    }
#endif
    
    #endregion
    
    #region UnityCallbacks
    
#if UNITY_EDITOR
    private void OnValidate() {
                
        //Make sure all values are present and only once
        foreach (TargetAreas area in Enum.GetValues(typeof(TargetAreas))) {
            
            int areaCount = m_barTimings.Count(zone => zone.Area == area);
            
            if (areaCount == 0) { //Add if missing
                m_barTimings.Add(new BarTimingZone(area));
            } else if(areaCount > 1){ //Remove all but 1 if too many
                for (int i = 0; i < areaCount - 1; i++) {
                    m_barTimings.Remove(m_barTimings.FindLast(zone => zone.Area == area));
                }
            }
        }
        
        //Sort
        m_barTimings = m_barTimings.OrderBy(area => area.Area).ToList();

        //Validate radius
        m_barTimings[0].SetMinTiming(0f);
        
        for (int i = 1; i < m_barTimings.Count; i++) {
            m_barTimings[i].SetMinTiming(m_barTimings[i-1].MaxTiming);
        }
    }
#endif
    #endregion
    
    [System.Serializable]
    private class BarTimingZone {
        [SerializeField] private TargetAreas m_targetArea;
        [SerializeField] private float m_minTiming;
        [SerializeField] private float m_maxTiming;
        [SerializeField] private Color m_indicatorColor;

        public TargetAreas Area => m_targetArea;
        public float MinTiming => m_minTiming;
        public float MaxTiming => m_maxTiming;

        public Color IndicatorColor => m_indicatorColor;
        
        public BarTimingZone(TargetAreas area) {
            m_targetArea = area;
        }

        public void SetMinTiming(float minimalTiming) {
            m_minTiming = minimalTiming;
            m_maxTiming = Mathf.Max(minimalTiming, m_maxTiming);
        }
    }
}