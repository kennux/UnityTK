using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityTK
{
	/// <summary>
	/// Tooltip opener implementation for <see cref="TextTooltipViewModel"/>.
	/// Will open a tooltip with the specified text on the gameobject this is attached to or the mouse position.
	/// </summary>
	public class TextTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public string text;
		public bool anchorToMouse = false;
		public bool followMouse = false;

		[Header("Only for RectTransforms")]
		public Vector2 tooltipPivotInRectTransform = new Vector2(.5f,.5f);
        private bool pointerInside;
        private string currentText;

        public void OnDisable()
        {
            if (pointerInside)
                Tooltip.Close();
            pointerInside = false;
        }

        public void OnEnable()
        {
            var rectTransform = this.transform as RectTransform;
            if (rectTransform == null)
                return;

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null))
                OnPointerEnter(null); // Open tooltip
        }

        public void Update()
        {
            if (!pointerInside)
                return;

            if (currentText != text)
            {
                OnPointerExit(null);
                OnPointerEnter(null);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
		{
            if (string.IsNullOrEmpty(text))
            {
                Tooltip.Close();
                return;
            }
            
			var model = TextTooltipViewModel.instance;
			model.text = text;
            currentText = text;

			TooltipAnchorTarget target;
			if (this.anchorToMouse)
				target = TooltipAnchorTarget.ForMouse(this.followMouse);
			else if (this.transform is RectTransform)
				target = TooltipAnchorTarget.ForUIObject(this.transform as RectTransform, this.tooltipPivotInRectTransform);
			else
				target = TooltipAnchorTarget.ForWorldObject(this.transform);

			Tooltip.Open(model, target);
            pointerInside = true;
        }

		public void OnPointerExit(PointerEventData eventData)
		{
			Tooltip.Close();
            pointerInside = false;
        }
	}
}
