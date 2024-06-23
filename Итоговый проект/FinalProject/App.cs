using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FinalProject
{
    public partial class App : Application
    {
        public static Frame CurrentFrame { get; set; }
        public static TextBlock FullNameTextBlock { get; set; }
        public static User CurrentUser { get; set; }
    }
}
