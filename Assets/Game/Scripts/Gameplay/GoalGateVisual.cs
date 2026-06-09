using UnityEngine;
using PeopleFlow.Data;
using TMPro;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Handles the visual representation of a GoalGate, including color tinting and count label.
    /// </summary>
    public class GoalGateVisual : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Renderer[] tintRenderers;
        [SerializeField] private TextMeshPro countLabel;

        private MaterialPropertyBlock _mpb;
        private GoalGate _goalGate;
        private ColorPalette _palette;

        public void Bind(GoalGate goalGate, ColorPalette palette)
        {
            _goalGate = goalGate;
            _palette = palette;
            
            if (_mpb == null)
                _mpb = new MaterialPropertyBlock();

            UpdateVisuals();

            _goalGate.OnCountChanged += HandleCountChanged;
        }

        private void HandleCountChanged(GoalGate goalGate)
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_goalGate == null || _palette == null) return;

            Color colorToApply = _palette.GetColor(_goalGate.GoalColor);
            
            if (countLabel != null)
            {
                countLabel.gameObject.SetActive(_goalGate.IsActive && !_goalGate.IsCompleted);
                countLabel.text = _goalGate.RemainingCount.ToString();
                countLabel.color = colorToApply;
            }

            if (tintRenderers != null)
            {
                foreach (var renderer in tintRenderers)
                {
                    if (renderer == null) continue;
                    ApplyTint(renderer, colorToApply);
                }
            }
        }

        private void ApplyTint(Renderer renderer, Color color)
        {
            renderer.GetPropertyBlock(_mpb);
            
            if (renderer.sharedMaterial != null)
            {
                if (renderer.sharedMaterial.HasProperty(BaseColorId))
                    _mpb.SetColor(BaseColorId, color);
                
                if (renderer.sharedMaterial.HasProperty(LegacyColorId))
                    _mpb.SetColor(LegacyColorId, color);
            }
            
            renderer.SetPropertyBlock(_mpb);
        }

        private void OnDestroy()
        {
            if (_goalGate != null)
                _goalGate.OnCountChanged -= HandleCountChanged;
        }
    }
}
