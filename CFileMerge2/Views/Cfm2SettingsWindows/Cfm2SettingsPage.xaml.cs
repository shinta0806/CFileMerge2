// ============================================================================
// 
// ���ݒ�y�[�W�̃R�[�h�r�n�C���h
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using CFileMerge2.ViewModels.Cfm2SettingsWindows;

using Microsoft.UI.Xaml.Controls;
using WinUIEx;

namespace CFileMerge2.Views.Cfm2SettingsWindows;

public sealed partial class Cfm2SettingsPage : PageEx3
{
    // ====================================================================
    // �R���X�g���N�^�[
    // ====================================================================

    /// <summary>
    /// ���C���R���X�g���N�^�[
    /// </summary>
    public Cfm2SettingsPage(WindowEx3 window)
            : base(window)
    {
        ViewModel = new Cfm2SettingsPageViewModel(window);
        InitializeComponent();
    }

    // ====================================================================
    // public �v���p�e�B�[
    // ====================================================================

    /// <summary>
    /// �r���[���f��
    /// </summary>
    public Cfm2SettingsPageViewModel ViewModel
    {
        get;
    }
}
