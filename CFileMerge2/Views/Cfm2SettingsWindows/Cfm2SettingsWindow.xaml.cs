// ============================================================================
// 
// ���ݒ�E�B���h�E�̃R�[�h�r�n�C���h
// 
// ============================================================================

// ----------------------------------------------------------------------------
// �E�B���h�E�ł� MVVM ������ł���Ǝv����̂ŁA�E�B���h�E�ւ̑���̓y�[�W�̃r���[���f���ōs��
// ----------------------------------------------------------------------------

using CFileMerge2.Models.SharedMisc;

namespace CFileMerge2.Views.Cfm2SettingsWindows;

public sealed partial class Cfm2SettingsWindow : WindowEx2
{
    // ====================================================================
    // �R���X�g���N�^�[
    // ====================================================================

    /// <summary>
    /// ���C���R���X�g���N�^�[
    /// </summary>
    public Cfm2SettingsWindow()
    {
        InitializeComponent();

        // ������
        SizeToContent = SizeToContent.Height;
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, Cfm2Constants.CONTENT_PATH_ICON));
        Content = new Cfm2SettingsPage(this);
    }
}
