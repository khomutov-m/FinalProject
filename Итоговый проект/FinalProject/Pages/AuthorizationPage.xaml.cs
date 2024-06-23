using FinalProject.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FinalProject
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationPage.xaml
    /// </summary>
    public partial class AuthorizationPage : Page
    {
        public AuthorizationPage()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));
        }

        private void EnterAsGuestButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentFrame.Navigate(new ClientPage());
            App.CurrentUser = new() {UserName="Гость" };
            App.FullNameTextBlock.Text = App.CurrentUser.UserFullName;
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.CurrentUser = DataAccessLayer.GetUser(LoginTextBox.Text);
                if(App.CurrentUser!=null)
                {
                    if (App.CurrentUser.UserPassword == PasswordBox.Password)
                    {
                        App.FullNameTextBlock.Text = App.CurrentUser.UserFullName;
                        switch (App.CurrentUser.UserRole)
                        {
                            case 2: 
                                App.CurrentFrame.Navigate(new ManagerPage());
                                break;
                            case 3:
                                App.CurrentFrame.Navigate(new AdminPage());
                                break;
                            default:
                                App.CurrentFrame.Navigate(new ClientPage());
                                break;
                        }
                    }
                    else
                        MessageBox.Show("Неверный пароль", "Некорректные данные", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                    MessageBox.Show("Неверный логин", "Некорректные данные", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch
            {
                MessageBox.Show("Не удалось получить данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
