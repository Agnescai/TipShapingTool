﻿#pragma checksum "..\..\..\BigTechControllerConnection.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "421829BCF33E2DF43E6ED5C922DBB4C00AD79E27"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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
using TipShaping;


namespace TipShaping {
    
    
    /// <summary>
    /// BigTechControllerConnection
    /// </summary>
    public partial class BigTechControllerConnection : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 13 "..\..\..\BigTechControllerConnection.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock PortTextBlock;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\BigTechControllerConnection.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox PortComboBox;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\BigTechControllerConnection.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock BaudRateTextBlock;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\BigTechControllerConnection.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox BaudRateComboBox;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\BigTechControllerConnection.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ConnectButton;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\BigTechControllerConnection.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock CommandTextBlock;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\BigTechControllerConnection.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox CommandInput;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\BigTechControllerConnection.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SendButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.3.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TipShaping;component/bigtechcontrollerconnection.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\BigTechControllerConnection.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.3.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 9 "..\..\..\BigTechControllerConnection.xaml"
            ((TipShaping.BigTechControllerConnection)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.BigTechControllerConnection_Closing);
            
            #line default
            #line hidden
            
            #line 9 "..\..\..\BigTechControllerConnection.xaml"
            ((TipShaping.BigTechControllerConnection)(target)).Loaded += new System.Windows.RoutedEventHandler(this.BigTechControllerConnection_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.PortTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.PortComboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.BaudRateTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.BaudRateComboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 6:
            this.ConnectButton = ((System.Windows.Controls.Button)(target));
            
            #line 17 "..\..\..\BigTechControllerConnection.xaml"
            this.ConnectButton.Click += new System.Windows.RoutedEventHandler(this.ConnectButton_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.CommandTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 8:
            this.CommandInput = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.SendButton = ((System.Windows.Controls.Button)(target));
            
            #line 23 "..\..\..\BigTechControllerConnection.xaml"
            this.SendButton.Click += new System.Windows.RoutedEventHandler(this.SendButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

