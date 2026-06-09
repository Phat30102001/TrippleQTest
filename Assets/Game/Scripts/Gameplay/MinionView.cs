using UnityEngine;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Handles the visual representation of a minion, including color tinting and animations.
    /// </summary>
    public class MinionView : MonoBehaviour
    {
        private static readonly int BaseColorShaderPropertyId = Shader.PropertyToID("_BaseColor");
        private static readonly int SpeedParameterHash = Animator.StringToHash("Speed");
        private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
        private static readonly int DieTriggerHash = Animator.StringToHash("Die");

        [SerializeField] private Animator minionAnimator;
        [SerializeField] private Renderer[] colorableRenderers;

        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            if (minionAnimator == null) 
                minionAnimator = GetComponentInChildren<Animator>();
            
            if (colorableRenderers == null || colorableRenderers.Length == 0)
                colorableRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            
            _propertyBlock = new MaterialPropertyBlock();
        }

        public void SetColor(Color color)
        {
            if (_propertyBlock == null) 
                _propertyBlock = new MaterialPropertyBlock();
            
            foreach (var renderer in colorableRenderers)
            {
                if (renderer == null) continue;
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(BaseColorShaderPropertyId, color);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        public void SetRunning(bool isRunning)
        {
            if (minionAnimator != null) 
                minionAnimator.SetFloat(SpeedParameterHash, isRunning ? 1f : 0f);
        }

        public void PlayJump()
        {
            if (minionAnimator != null) 
                minionAnimator.SetTrigger(JumpTriggerHash);
        }

        public void PlayDie()
        {
            if (minionAnimator != null) 
                minionAnimator.SetTrigger(DieTriggerHash);
        }
    }
}
