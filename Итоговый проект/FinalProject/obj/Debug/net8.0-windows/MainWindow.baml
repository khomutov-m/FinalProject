   M S B A M L   `   `   `  ����G  CFinalProject, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:FinalProjectFinalProject  5   	       FinalProject.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:FinalProject     5   	      AppWindowStyle#��[  5   	     � 
ResizeMode$  NoResize=�5	   	   $	��550��5
   E   $	��800��6R    � Title 6	   ��  �� ElementName$ 	MainFrame�� �� Path$ Content.Title�.��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      .+�5   
   �� -   5        $��MainPageButton��5      $��Главный экран��6'   $	��100��6?   $
��Left=�6K   5      �� 5   
   -   5        $��FullNameTextBlock��5      $��Right=�6-   $��5q�6I   �� 5   
   $	��0.5q�6   
���   �$��1��6;    � 5   
   -   5        $��	MainFrame��5        � Source$" Pages/AuthorizationPage.xaml?�6!     � NavigationUIVisibility$ Hidden=�6G   $��2��6g   5      5      