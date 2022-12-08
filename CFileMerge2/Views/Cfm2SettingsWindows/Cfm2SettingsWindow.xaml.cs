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
using WinRT.Interop;
using WinUIEx;

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
        WinUi3Common.EnableContextHelp(this, SubclassProc);
#if false
        var a = PresenterKind;
        var b = (OverlappedPresenter)Presenter;
        b.IsModal = true;
#endif
    }

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
    private IntPtr SubclassProc(IntPtr hwnd, UInt32 msg, IntPtr wPalam, IntPtr lParam, IntPtr _1, IntPtr _2)
    {
        switch ((User32.WindowMessage)msg)
        {
            case User32.WindowMessage.WM_SYSCOMMAND:
                if ((User32.SysCommands)wPalam == User32.SysCommands.SC_CONTEXTHELP)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Cfm2Common.ShowHelpAsync(this);
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
