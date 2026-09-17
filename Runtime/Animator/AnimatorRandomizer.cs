using System;
using UnityEngine;
using UnityAnimator = UnityEngine.Animator;

namespace Mane.Unity.Animator
{
    /// <summary>
    /// On state enter, sets a selector int to a random variant for the first matching condition.
    /// </summary>
    [ManeStyle]
    public class AnimatorRandomizer : StateMachineBehaviour
    {
        [Header("Leave condition empty for always true behaviour.")]
        [SerializeField] private SwitchCondition[] _conditions;

        /// <summary>
        /// Applies the first matching <see cref="SwitchCondition"/>, then continues the state enter.
        /// </summary>
        public override void OnStateEnter(UnityAnimator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (SwitchCondition condition in _conditions)
            {
                int? parameter = condition.ConditionParameter;
                if (parameter == null || animator.GetBool(parameter.Value))
                {
                    animator.SetInteger(condition.SelectorParameter, 
                                        UnityEngine.Random.Range(0, condition.TotalVariants));
                    break;
                }
            }
            
            base.OnStateEnter(animator, stateInfo, layerIndex);
        }

        
        /// <summary>
        /// Optional bool gate and the int parameter that receives a random variant index.
        /// </summary>
        [Serializable]
        public class SwitchCondition
        {
            [SerializeField] private string _conditionParameter;
            [SerializeField] private string _selectorParameter;
            [SerializeField] private int _totalVariants;
            
            private int? _selectorParameterHash;
            private int? _conditionParameterHash;
            
            /// <summary>
            /// Hashed int parameter that stores the chosen variant.
            /// </summary>
            public int SelectorParameter
            {
                get
                {
                    _selectorParameterHash ??= UnityAnimator.StringToHash(_selectorParameter);

                    return _selectorParameterHash.Value;
                }
            }
            
            /// <summary>
            /// Hashed bool that must be true, or null to always match.
            /// </summary>
            public int? ConditionParameter
            {
                get
                {
                    if (string.IsNullOrEmpty(_conditionParameter)) return null;

                    _conditionParameterHash ??= UnityAnimator.StringToHash(_conditionParameter);

                    return _conditionParameterHash.Value;
                }
            }
            
            /// <summary>
            /// Exclusive upper bound for the random variant index.
            /// </summary>
            public int TotalVariants => _totalVariants;
        }
    }
}
