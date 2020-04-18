using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerDownHandler {
    
    #region Serialized fields
    
    [Header("References")]
    [SerializeField] private Animator m_animator;
    
    [SerializeField] private TargetController m_targetController;

    [SerializeField] private BarController m_barController;

    [Header("Animation settings")]
        
    [SerializeField] private string m_stateTransitionParameter;
    
    [Header("Difficulty settings")]
        
    [SerializeField] private DifficultySettings m_difficultySettings;
        
    #endregion
    
    #region PrivateFields
            
    private int m_stateTransitionParameterHash;

    private bool m_busy = false;
    
    private ButtonState m_state = ButtonState.Start;

    private Coroutine m_indicatorMoveRoutine = null;
    
    float barProgress = 0f;
    
    #endregion
    
    #region PublicMethods

    public void OnPointerDown(PointerEventData eventData) {
        ButtonPressed();
    }

    #endregion
    
    #region PrivateMethods
    private void ButtonPressed() {
        if (m_busy) { return; }

        if (m_state == ButtonState.Start) {
            m_animator.SetTrigger(m_stateTransitionParameterHash);
            m_state = ButtonState.Shoot;
            if (m_indicatorMoveRoutine != null) {
                StopCoroutine(MoveBarIndicatorRoutine());
            }
            m_indicatorMoveRoutine = StartCoroutine(MoveBarIndicatorRoutine());
            
        } else {
            m_animator.SetTrigger(m_stateTransitionParameterHash);
            m_busy = true;
            StopCoroutine(m_indicatorMoveRoutine);
            m_targetController.ShootArrow(m_difficultySettings.GetTargetArea(barProgress),
                () => {
                    m_busy = false;
                    m_animator.SetTrigger(m_stateTransitionParameterHash);
                    m_state = ButtonState.Start;
                });
        }
    }

    private IEnumerator MoveBarIndicatorRoutine() {
        barProgress = 0f;
        bool directionRight = true;
        while (true) {
            //Debug.Log($"Bar progress is {barProgress}");
            barProgress += (directionRight? 1f : -1f) * m_difficultySettings.BarIndicatorSpeed * Time.deltaTime;
            barProgress = Mathf.Clamp01(barProgress);
            m_barController.SetIndicatorProgress(barProgress);

            if (Mathf.Abs(barProgress - 0.5f) >= 0.5f) {
                directionRight = !directionRight;
            }

            yield return null;
        }
    }

    #endregion
    
    #region UnityCallbacks
    
    private void Awake() { 
        m_stateTransitionParameterHash = Animator.StringToHash(m_stateTransitionParameter);
    }
    
    
#if UNITY_EDITOR
    private void OnValidate() {

        //Validate animator
        if (!m_animator) { m_animator = GetComponent<Animator>(); }
        
        //Validate animator parameters
        if (!m_animator.parameters.Any(parameter => parameter.name.Equals(m_stateTransitionParameter))) { m_stateTransitionParameter = null; }
        
        if (m_difficultySettings == null) {
            m_difficultySettings = DifficultySettings.Load();
        }
    }
#endif
    #endregion
    
    private enum ButtonState {
        Start,
        Shoot,
    }
}
