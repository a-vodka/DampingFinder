   M S B A M L   `   `   `  ����H  DDampingFinder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:DampingFinderDampingFinder  5      !    DampingFinder.CustomSpinner   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:DampingFinder     5      $��30��5   6   $	��100��6B   $��30��6N   $	��100��6]   .��5	      �   � ColumnDefinitions  5
   
   �� 5      $��1* �6   �� 5      $��30 �6    � RowDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6   .+�5   
   �� 
����������	#FFE6E6E6$��1q�6<    �� CornerRadius$ 2��6P   $��2��6a   $��2��6r   �� 5   
   -   5         ��Name$ numericText��5      $��2��6'    �� SelectionBrush �	#FF60ACF7$��0q�6�   $
��Bold"�6�   $��Right=�6�   $��Center=�6�   ���	#FF767676$��0,0,4,0q�62  ��68   Z� ��6N   Z� ��6D  �� *Value+ �� RelativeSource �� *FindAncestor+ �� AncestorType!   DampingFinder.CustomSpinner"  �� 5   
   -   5         ��Name$ btnInc��5       � Cursor$
 Hand��6!   $��1��6D   ��5      �� 5      .R�5      � .+�5      �� ���	#FF4FC0EA$��0��65   �� 5      
�������$��0��61   $��Center=�6E   $��Center=�6b   $��6��6}   $��2��6�   �� 5      
�������$��0��61   $��Center=�6E   $��Center=�6b   $��2��6}   $��6��6�   5      5      5!      �� 5#   
   -   5        $ btnDec��5#      $
 Hand��6!   $��1��6/   $��1��6?   ��5$      �� 5%      .R�5&      � .+�5'      �� ���	#FF4FC0EA$��0��65   �� 5(      
�������$��0��61   $��Center=�6E   $��Center=�6b   $��6��6}   $��2��6�   5)      5*      5,      5.      5/      