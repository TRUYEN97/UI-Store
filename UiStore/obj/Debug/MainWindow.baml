   M S B A M L   `   `   `  ����B  >UiStore, Version=2.5.5.0, Culture=neutral, PublicKeyToken=null5clr-namespace:UiStore.BehaviorsUiStore.Behaviors  5   	   !clr-namespace:UiStoreUiStore  5   	       UiStore.MainWindow   5   	   -	behaviorsclr-namespace:UiStore.Behaviors  5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   'localclr-namespace:UiStore     5   	   
������$	��600��5
   !   $
��1024��6.     � Title  6	   �� *Title+.��5      � $��0,0,0,0q�6    � ColumnDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $��2��6   $��0��6/   $��1��6?   ���#EEE �� Padding$ 10q�6e   $��5q�6r    �� CornerRadius$ 5��6}      ShadowEffect J� Effect# [  6�   ._�5      �� .+�5      �� $��Information��6   $
��Bold"�6/   $��16(�6A   �� 5      $
��10 2q�6S   ��6   �� *	Product+ �� StringFormat$ Product: {0}��6S   �� 5       $
��10 2q�6T   ��6   �� *	Station+$ Station:  {0}��6T   �� 5!      $
��10 2q�6S   ��6   �� *PcName+$ Pc name:  {0}��6S   �� 5"      $
��Bold"�66   $��10 10q�6H   $	��100��6W   $��14(�6d   ��6   �� *Ips+6d   5#      5$      �� 5'   
   $��0��6   $��1��6   $��1��6/   $��3��6?   ���#F5F5F5$ 10q�6h   $��5q�6u   $ 5��6�   # [  6�   ._�5(      �� .+�5)      �� $��Scan MAC��6   $
��Bold"�6,   $��14(�6>   $��0 0 0 5q�6L   �� 5*      5+      5,      �� 5/   
   $��1��6   $��1��6   $��1��6/   $��3��6?   ���#F5F5F5$ 10q�6h   $��5q�6u   $ 5��6�   # [  6�   ._�50      �� ��6   �� *Applications+��51      �� 52      .R�53      � 54      ��56      �� 57      .R�58      �� $��10q�6!   $ 3q�6-   $ 8��69   # [  59      0  *UiStore.Behaviors.MouseDoubleClickBehavior   Command 5:      �� *LaunchCommand+��5;      �� 5<   "    �� 
TargetType" ��6(   .�5=   &   �� 	 �� Property	 � ��
 �� Value

 ���� 6C    �� Triggers 5>   &   P� 5?   *    P� Property �@J�IsMouseOver P� Value . . �5@   .   �� 	 � ��

 �����6K   �� 5A   .   	 �@�Cursor$

 Hand��6G   5B   +   5D   #   ._�5G      �� $ 6��6L   $ 3q�6]   ��6%   �� *BackgroundColor+._�5H   "   �� $ 6��6V   $ 3q�6g   ��6)   �� *StatusBackgroundColor+._�5I   &   �� $ 6��6[   $ 8q�6l   ��6-   �� *RunningBackgroundColor+._�5J   *   �� $	��100��65   .+�5K   .   �� $��64��64   $��64��6?   $��Uniform=�6i   $��0,0,0,10q�6{   ��6K   �� *
IconSource+6{   �� 5L   .    �� TextAlignment$ Center=�5M   *   $
��Wrap=�5N   *   $	��100��5O   *   $��CharacterEllipsis=�5P   *   ��5L   8   �� *Name+5P   *   � 5Q   .   $��5��6U    � Value 6:   �� *
Progress+6U   5R   +   5S   '   5T   #   5U       � ContextMenu 5W      �� 5X   "   .@�5Y   &   m� $
��Open��6/    m� Command 6=   �� *LaunchCommand+ m� Icon 5Z   *   �� 5[   .   $0��*/UiStore;component/Resources/LaunchApp.png��64   5]   '   m� 5^   &   $��Close��6/    6>   �� *CloseCommand+ 5_   *   �� 5`   .   $,��&/UiStore;component/Resources/Close.png��64   5b   '   m� 5c   &   $��Information��6/    6D   �� *ShowInfoCommand+ 5d   *   �� 5e   .   $1��+/UiStore;component/Resources/Properties.png��64   5g   '   m� 5h   &   $��Cancel update��6/    6F   �� *CancelAppUpdate+ 5i   *   �� 5j   .   $-��'/UiStore;component/Resources/Cancel.png��64   5l   '   m� 5m   &   $��Cancel Extrack��6/    6G   �� *CancelExtrack+ 5n   *   �� 5o   .   $-��'/UiStore;component/Resources/Cancel.png��64   5q   '   5r   #   5t      5u      5w      5x      �� 5{   
   $��2��6   $��1��6   $��0��6/   $��4��6@   ���#F5F5F5$ 10q�6j   $��5q�6w   $ 5��6�   # [  6�   ._�5|      �� .+�5}      �� $��80��6;   ��6   �� *
LogLines+6;   5~      5      5�      5�      