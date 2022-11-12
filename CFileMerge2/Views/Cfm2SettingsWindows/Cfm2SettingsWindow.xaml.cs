// ============================================================================
// 
// ���ݒ�E�B���h�E�̃R�[�h�r�n�C���h
// 
// ============================================================================

// ----------------------------------------------------------------------------
// �E�B���h�E�ł� MVVM ������ł���Ǝv����̂ŁA�E�B���h�E�ւ̑���̓y�[�W�̃r���[���f���ōs��
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using CFileMerge2.Models.SharedMisc;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CFileMerge2.Views.Cfm2SettingsWindows;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Cfm2SettingsWindow : WindowEx
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
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, Cfm2Constants.CONTENT_PATH_ICON));
        Content = new Cfm2SettingsPage(this);

        // �Ȃ��� Cfm2SettingsWindow.xaml �� Width, Height ���w�肵�Ă������Ȃ��̂ŁA�����Ŏw�肷��
        // ToDo: �����悤�ɂȂ�΂��̃R�[�h�͕s�v
        Width = 600;

        // Height �͌�� Cfm2SettingsPageViewModel �ɂ��w�肳���͂��Ȃ̂ŁA�����ł͉��w��
        // �������Ɩ{���̍����𑪒�ł��Ȃ����߁A�����傫�߂Ɏw�肵�Ă���
        // ToDo: SizeToContent �����������΂��̃R�[�h�͕s�v
        Height = 400;

    }
}
