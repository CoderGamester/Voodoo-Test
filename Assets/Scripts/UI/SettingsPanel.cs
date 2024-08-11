using Game.Data;
using Game.Services;
using Game.Logic;
using GameLovers.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour 
{
    public Image m_VibrationButton;
	public Image m_GoogleIcon;
	public Image m_FacebookIcon;
	public Image m_AppleIcon;
	public Image m_EmailIcon;
	public Sprite m_VibrationOnSprite;
    public Sprite m_VibrationOffSprite;
	public Animator m_BarAnim;

    // Cache
    private MobileHapticManager m_Haptic;
	private IGameServices m_GamesServices;
	private IGameDataProvider m_GameDataProvider;

	// Buffer
	private bool m_PanelVisible;

	private readonly Color c_ButtonColorOn = new Color(1, 1, 1, 0.5f);

	private void Awake()
	{
		m_GamesServices = MainInstaller.Resolve<IGameServices>();
		m_GameDataProvider = MainInstaller.Resolve<IGameDataProvider>();
		m_Haptic = MobileHapticManager.Instance;
        m_PanelVisible = false;

        m_BarAnim.SetBool("Visible", m_PanelVisible);
    }

	private void SetButtonStates()
	{
		var serverInfo = m_GamesServices.AuthenticationService.GetServerInfo();

		m_VibrationButton.sprite = m_GameDataProvider.AppDataProvider.IsHapticOn ? m_VibrationOnSprite : m_VibrationOffSprite;
		m_GoogleIcon.color = serverInfo.MockGoogleLogin ? c_ButtonColorOn : Color.white;
		m_FacebookIcon.color = serverInfo.MockFacebookLogin ? c_ButtonColorOn : Color.white;
		m_AppleIcon.color = serverInfo.MockAppleLogin ? c_ButtonColorOn : Color.white;
		m_EmailIcon.color = serverInfo.MockEmailLogin ? c_ButtonColorOn : Color.white;
	}

    public void ClickVibrateButton()
    {
		var vibration = !m_GameDataProvider.AppDataProvider.IsHapticOn;

		m_GameDataProvider.AppDataProvider.IsHapticOn = vibration;
		m_VibrationButton.sprite = vibration ? m_VibrationOnSprite : m_VibrationOffSprite;

		if (vibration)
            m_Haptic.Vibrate(MobileHapticManager.E_FeedBackType.ImpactHeavy);
	}

	public void ClickSettingsButton()
	{
		m_PanelVisible = !m_PanelVisible;
		m_BarAnim.SetBool("Visible", m_PanelVisible);
	}

	public void ClickAppleButton()
	{
		m_GamesServices.AuthenticationService.LoginWithApple(RestartScene, null);
	}

	public void ClickFacebookButton()
	{
		m_GamesServices.AuthenticationService.LoginWithApple(RestartScene, null);
	}

	public void ClickGoogleButton()
	{
		m_GamesServices.AuthenticationService.LoginWithApple(RestartScene, null);
	}

	public void ClickEmailButton()
	{
		m_GamesServices.AuthenticationService.LoginWithApple(RestartScene, null);
	}

	public void RestartScene(PlayerData data)
    {
        // This is necessary until all objects in the menu follow an obeservable pattern listener from logic
		SceneManager.LoadScene("Game");
	}
}
