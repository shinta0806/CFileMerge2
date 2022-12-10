// ============================================================================
// 
// ���ݒ�E�B���h�E�̃R�[�h�r�n�C���h
// 
// ============================================================================

// ----------------------------------------------------------------------------
// �E�B���h�E�ł� MVVM ������ł���Ǝv����̂ŁA�E�B���h�E�ւ̑���̓y�[�W�̃r���[���f���ōs��
// ----------------------------------------------------------------------------

using System.Diagnostics;

using CFileMerge2.Models.SharedMisc;

using Microsoft.UI.Windowing;

using PInvoke;

using Serilog;
using Serilog.Events;

using Shinta;
using Shinta.WinUi3;

namespace CFileMerge2.Views.Cfm2SettingsWindows;

public sealed partial class Cfm2SettingsWindow : WindowEx3
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
        Title = "Cfm2SettingsWindow_Title".ToLocalized();
        Content = new Cfm2SettingsPage(this);
        _newSubclassProc = new WindowsApi.SubclassProc(SubclassProc);
        WinUi3Common.EnableContextHelp(this, _newSubclassProc);
#if false
        var a = PresenterKind;
        var b = (OverlappedPresenter)Presenter;
        b.IsModal = true;
#endif
    }

    // ====================================================================
    // private �ϐ�
    // ====================================================================

    /// <summary>
    /// �E�B���h�E�v���V�[�W���[
    /// </summary>
    private readonly WindowsApi.SubclassProc _newSubclassProc;

    // ====================================================================
    // private �֐�
    // ====================================================================

    /// <summary>
    /// �E�B���h�E���b�Z�[�W����
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="msg"></param>
    /// <param name="wPalam"></param>
    /// <param name="lParam"></param>
    /// <param name="_1"></param>
    /// <param name="_2"></param>
    /// <returns></returns>
    private IntPtr SubclassProc(IntPtr hwnd, User32.WindowMessage msg, IntPtr wPalam, IntPtr lParam, IntPtr _1, IntPtr _2)
    {
        Debug.WriteLine("SubclassProc()" + Environment.TickCount.ToString("#,0"));
        switch (msg)
        {
            case User32.WindowMessage.WM_SYSCOMMAND:
                if ((User32.SysCommands)wPalam == User32.SysCommands.SC_CONTEXTHELP)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Cfm2Common.ShowHelpAsync(this, "Kankyousettei");
                        }
                        catch (Exception ex)
                        {
                            await ShowLogMessageDialogAsync(LogEventLevel.Error, "�w���v�\�����G���[�F\n" + ex.Message);
                            Log.Information("�X�^�b�N�g���[�X�F\n" + ex.StackTrace);
                        }
                    });
                    return IntPtr.Zero;
                }
                return WindowsApi.DefSubclassProc(hwnd, msg, wPalam, lParam);
            default:
                return WindowsApi.DefSubclassProc(hwnd, msg, wPalam, lParam);
        }
    }
}

