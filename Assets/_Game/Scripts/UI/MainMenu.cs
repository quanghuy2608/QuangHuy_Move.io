using UnityEngine;

public class MainMenu : UICanvas
{
    public void PlayButton()
    {
        LevelManager.Instance.OnStartGame();
        Close();
    }

    
    public void SkinSelectButton()
    {
        
        UIManager.Instance.OpenUI<SkinSelectionUI>();
        Close();
    }

    public override void Setup()
    {
        base.Setup();
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }
    public void SettingButton()
    {
        UIManager.Instance.OpenUI<Setting>();
    }
}