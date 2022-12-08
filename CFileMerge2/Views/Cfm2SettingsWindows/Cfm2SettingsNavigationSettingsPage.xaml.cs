// ============================================================================
// 
// �ݒ�y�[�W�̃R�[�h�r�n�C���h
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using CFileMerge2.ViewModels.Cfm2SettingsWindows;

using Microsoft.UI.Xaml.Controls;

namespace CFileMerge2.Views.Cfm2SettingsWindows;

public sealed partial class Cfm2SettingsNavigationSettingsPage : Page
{
    // ====================================================================
    // �R���X�g���N�^�[
    // ====================================================================

    /// <summary>
    /// ���C���R���X�g���N�^�[
    /// </summary>
    public Cfm2SettingsNavigationSettingsPage(WindowEx3 window, Cfm2SettingsPageViewModel cfm2SettingsPageViewModel)
    {
        ViewModel = new Cfm2SettingsNavigationSettingsPageViewModel(window, cfm2SettingsPageViewModel);
        InitializeComponent();
    }

    // ====================================================================
    // public �v���p�e�B�[
    // ====================================================================

    /// <summary>
    /// �r���[���f��
    /// </summary>
    public Cfm2SettingsNavigationSettingsPageViewModel ViewModel
    {
        get;
    }
}
