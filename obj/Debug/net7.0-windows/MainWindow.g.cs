﻿#pragma checksum "..\..\..\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "71E5B2EC77A4D86BEAF178EEC37CBBCFEE7D8365"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Options.File.Checker.WPF;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Options.File.Checker.WPF {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox LicenseFileLocationTextBox;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button LicenseFileBrowseButton;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label LicenseFileLocationLabel;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox OptionsFileLocationTextBox;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button OptionsFileBrowseButton;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock OutputTextBlock;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label OptionsFileLocationLabel;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AnalyzerButton;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SaveOutputButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.13.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Options.File.Checker.WPF;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.13.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.LicenseFileLocationTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 10 "..\..\..\MainWindow.xaml"
            this.LicenseFileLocationTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.LicenseFileLocationTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.LicenseFileBrowseButton = ((System.Windows.Controls.Button)(target));
            
            #line 11 "..\..\..\MainWindow.xaml"
            this.LicenseFileBrowseButton.Click += new System.Windows.RoutedEventHandler(this.LicenseFileBrowseButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.LicenseFileLocationLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.OptionsFileLocationTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 13 "..\..\..\MainWindow.xaml"
            this.OptionsFileLocationTextBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.OptionsFileLocationTextBox_TextChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.OptionsFileBrowseButton = ((System.Windows.Controls.Button)(target));
            
            #line 14 "..\..\..\MainWindow.xaml"
            this.OptionsFileBrowseButton.Click += new System.Windows.RoutedEventHandler(this.OptionsFileBrowseButton_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.OutputTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 7:
            this.OptionsFileLocationLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 8:
            this.AnalyzerButton = ((System.Windows.Controls.Button)(target));
            
            #line 17 "..\..\..\MainWindow.xaml"
            this.AnalyzerButton.Click += new System.Windows.RoutedEventHandler(this.AnalyzerButton_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.SaveOutputButton = ((System.Windows.Controls.Button)(target));
            
            #line 18 "..\..\..\MainWindow.xaml"
            this.SaveOutputButton.Click += new System.Windows.RoutedEventHandler(this.SaveOutputButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

