using System;
using UnityEngine;
using UnityAnimator = UnityEngine.Animator;

namespace Mane.Unity.Animator
{
    /// <summary>
    /// Resets an animator parameter to its default when this state is entered.
    /// </summary>
    [ManeStyle]
    public class AnimatorParameterResetter : StateMachineBehaviour
    {
        [SerializeField] private ParameterType _type;
        [SerializeField] private string _parameter;


        private int? _parameterHash;


        /// <summary>
        /// Hashed name of the parameter to reset.
        /// </summary>
        public int ParameterHash
        {
            get
            {
                _parameterHash ??= UnityAnimator.StringToHash(_parameter);
                return _parameterHash.Value;
            }
        }


        /// <summary>
        /// Resets the configured parameter, then continues the state enter.
        /// </summary>
        public override void OnStateEnter(UnityAnimator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            switch (_type)
            {
                case ParameterType.Bool:
                    animator.SetBool(ParameterHash, false);
                    break;
                case ParameterType.Int:
                    animator.SetInteger(ParameterHash, 0);
                    break;
                case ParameterType.Float:
                    animator.SetFloat(ParameterHash, 0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            base.OnStateEnter(animator, stateInfo, layerIndex);
        }


        /// <summary>
        /// Animator parameter type to reset.
        /// </summary>
        public enum ParameterType
        {
            /// <summary>
            /// Reset to false.
            /// </summary>
            Bool = 0,

            /// <summary>
            /// Reset to 0.
            /// </summary>
            Int = 1,

            /// <summary>
            /// Reset to 0.
            /// </summary>
            Float = 2,
        }
    }
}
