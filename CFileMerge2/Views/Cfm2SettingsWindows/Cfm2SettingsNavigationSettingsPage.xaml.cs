// ============================================================================
// 
// �ݒ�y�[�W�̃R�[�h�r�n�C���h
// 
// ============================================================================

// ----------------------------------------------------------------------------
//  
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CFileMerge2.ViewModels;
using CFileMerge2.ViewModels.Cfm2SettingsWindows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace CFileMerge2.Views.Cfm2SettingsWindows;

public sealed partial class Cfm2SettingsNavigationSettingsPage : Page
{
    // ====================================================================
    // �R���X�g���N�^�[
    // ====================================================================

    /// <summary>
    /// ���C���R���X�g���N�^�[
    /// </summary>
    public Cfm2SettingsNavigationSettingsPage()
    {
        ViewModel = new Cfm2SettingsNavigationSettingsPageViewModel();
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
