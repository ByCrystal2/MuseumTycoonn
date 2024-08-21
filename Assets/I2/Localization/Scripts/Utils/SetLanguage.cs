using UnityEngine;

namespace I2.Loc
{
	[AddComponentMenu("I2/Localization/SetLanguage Button")]
	public class SetLanguage : MonoBehaviour 
	{		
		public string _Language;

#if UNITY_EDITOR
		public LanguageSource mSource;
#endif
		
		public void OnClick()
		{
			if (!MainMenu.instance.CanSetNewLanguage) return;
			ApplyLanguage();
        }

		public void ApplyLanguage()
		{
			if( LocalizationManager.HasLanguage(_Language))
			{
				LocalizationManager.CurrentLanguage = _Language;
				GameManager.instance.SetGameLanguage(_Language);
			}
		}
    }
}