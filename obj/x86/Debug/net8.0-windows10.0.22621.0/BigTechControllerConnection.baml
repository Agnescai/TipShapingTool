   M S B A M L   `   `   `  ����E  ATipShaping, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:TipShaping
TipShaping  5   	   ,    &TipShaping.BigTechControllerConnection   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:TipShaping     5   	     � Title$!  BigTechControllerConnection��5   	   $	��120��6-   $	��500��6:   .��5	      � .+�5
   
   �� $	��Top=�6    �� Orientation$ 
Horizontal=�6/   $��5,10,5,0q�6H   $��20��6Z   .+�5      	� $�� ��6   $��Center=�6%   $	��0,0q�6@   �� 5      -   5         ��Name$ PortTextBlock��5      $��Center=�60   $
��Wrap=�6K   $��Port:��6_   $	��0,0q�6m   �� 5      -   5         ��Name$ PortComboBox��5       �� IsDropDownOpen$ False��60    J� 	AllowDrop$
 True��6G   $��Center=�6Y   $	��100��6{   $	��5,0q�6�   6�   �� 5      -   5        $ BaudRateTextBlock��5      $
��Wrap=�65   $��
Baud Rate:��6I   $��Center=�6[   $��20,0,0,0q�6v   �� 5      -   5        $��BaudRateComboBox��5      $	��120��60   $ False��6<   $
 True��6S   $��Center=�6d   $	��5,0q�6�   6�   �� 5      -   5         ��Name$ ConnectButton��5      $��Connect��6,   $��Center=�6\   $
��20,0q�6~   $��65��6�   5      �� 5   
   $	��Top=�6   $ 
Horizontal=�6-   $��
5, 45, 5,0q�6F   $��20��6Z   .+�5      �� -   5        $ CommandTextBlock��5      $
��Wrap=�63   $��Command:��6G   $��Center=�6W   $��64��6r    �� TextAlignment$ Center=�6~   $	��0,0q�6�   �� 5      -   5         ��Name$ CommandInput��5      	 �� TextWrapping$
	 Wrap=�6-   $��Center=�6A   $	��255��6\   $��Center=�6j   $	��5,0q�6�   �� 5      -   5        $ 
SendButton��5      $
��Send��6)   $��Center=�69   $��64��6m   $��Center=�6y   $
��20,0q�6�   5      5      5      