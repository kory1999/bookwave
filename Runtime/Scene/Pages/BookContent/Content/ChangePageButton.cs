using BW.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Content
{
    public class ChangePageButton : PageButton
    {
        [SerializeField] private Image _vipLock;

        public override void ToggleVIPLock(bool on)
        {
            base.ToggleVIPLock(on);

            if (_vipLock != null)
            {
                _vipLock.ChangeAlpha(on ? 1f : 0f);
                _rightButton.image.ChangeAlpha(on ? 0f : 1f);
            }
        }
    }
}