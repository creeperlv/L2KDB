using L2KDB.Server.Core;
using L2KDB.Server.Diagnostic;
using LiteDatabase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace L2KDB.Server.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private NotifyIcon notifyIcon;
        public MainWindow()
        {
            InitializeComponent();
            //this.notifyIcon = new NotifyIcon();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
            //var u = new Uri("L2KDBIcon.ico");
            ////FileStream fileStream=new FileStream()
            ////"L2KDBIcon.ico"
            ////notifyIcon.Icon = new System.Drawing.Icon();
            ////notifyIcon.Icon = new System.Drawing.Icon();
            //notifyIcon.Visible = true;
            //notifyIcon.show
            Hardcodet.Wpf.TaskbarNotification.TaskbarIcon taskbarIcon = new Hardcodet.Wpf.TaskbarNotification.TaskbarIcon();
            try
            {

                taskbarIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/L2KDBIcon.ico"));
                taskbarIcon.ToolTipText = "L2KDB Server";
                taskbarIcon.TrayMouseDoubleClick += (a, b) =>
                {
                    if (this.Visibility != Visibility.Visible)
                    {
                        this.Show();
                    }
                    else
                    {
                        this.Hide();
                    }
                };
            }
            catch (Exception)
            {

            }
            this.Hide();
            Diagnotor.CurrentDiagnotor = new AdvancedDiagnotor(Output, Dispatcher);
            Diagnotor.CurrentDiagnotor.LogError("==========================================");
            Diagnotor.CurrentDiagnotor.LogError("=Enter 'Stop' to stop and exit the server=");
            Diagnotor.CurrentDiagnotor.LogError("==========================================");
            Task.Run(() =>
            {
                try
                {
                    core = new ServerCore(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Databases\\",
                    Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Server-Config\\");
                    core.Start();
                }
                catch (Exception e)
                {
                    Diagnotor.CurrentDiagnotor.LogError(e.Message);
                }
            });
        }
        ServerCore core;
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                string cmd = tb.Text;
                tb.Text = "";
                if (cmd.ToUpper() == "STOP")
                {
                    core.StopServer();
                    Environment.Exit(0);
                }
                else if (cmd.ToUpper().StartsWith("SET-ADMIN"))
                {
                    try
                    {

                        var combine = cmd.Substring("Set-Admin".Length).Trim();
                        var auth = combine.Split(' ');
                        core.SetAdmin(auth[0], auth[1]);
                        Diagnotor.CurrentDiagnotor.LogSuccess($"Set {auth[0]} to administrator.");

                    }
                    catch (Exception exc)
                    {
                        Diagnotor.CurrentDiagnotor.LogError($"Unable to set admin.\r\bException:{exc.Message}");
                    }
                }
                else if (cmd.ToUpper().StartsWith("SET-PORT"))
                {
                    try
                    {

                        var combine = cmd.Substring("Set-Port".Length).Trim();
                        core.SetPort(int.Parse(combine));
                        Diagnotor.CurrentDiagnotor.LogSuccess($"Set port to: {combine}.");
                        Diagnotor.CurrentDiagnotor.LogWarning($"It requires a restart to take effect.");

                    }
                    catch (Exception exc)
                    {
                        Diagnotor.CurrentDiagnotor.LogError($"Unable to set port.\r\bException:{exc.Message}");
                    }
                }
                else if (cmd.ToUpper().StartsWith("SET-IP"))
                {
                    try
                    {

                        var combine = cmd.Substring("Set-IP".Length).Trim();
                        core.SetIP((combine));
                        Diagnotor.CurrentDiagnotor.LogSuccess($"Set IP to: {combine}.");
                        Diagnotor.CurrentDiagnotor.LogWarning($"It requires a restart to take effect.");

                    }
                    catch (Exception exc)
                    {
                        Diagnotor.CurrentDiagnotor.LogError($"Unable to set port.\r\bException:{exc.Message}");
                    }
                }
                else if (cmd.ToUpper().StartsWith("REMOVE-ADMIN"))
                {
                    try
                    {

                        var combine = cmd.Substring("Remove-Admin".Length);
                        var auth = combine.Split(' ');
                        core.RemoveAdmin(auth[0], auth[1]);
                        Diagnotor.CurrentDiagnotor.LogSuccess($"Removed administrator permission of {auth[0]}.");

                    }
                    catch (Exception exc)
                    {
                        Diagnotor.CurrentDiagnotor.LogError($"Unable to set admin.\r\bException:{exc.Message}");
                    }
                }
                else if (cmd.ToUpper() == "VERSION")
                {
                    Dispatcher.Invoke(() =>
                    {
                        Output.Inlines.Add(new Run("====Version Info===="+Environment.NewLine) { Foreground = new SolidColorBrush(Colors.White) });
                        Output.Inlines.Add(new Run("Server:") { Foreground = new SolidColorBrush(Colors.White) });
                        Output.Inlines.Add(new Run(core.CoreVersion + Environment.NewLine) { Foreground = new SolidColorBrush(Colors.LimeGreen) });
                        Output.Inlines.Add(new Run("L2KDB:") { Foreground = new SolidColorBrush(Colors.White) });
                        Output.Inlines.Add(new Run(Database.DatabaseVersion + Environment.NewLine) { Foreground = new SolidColorBrush(Colors.LimeGreen) });
                        Output.Inlines.Add(new Run("Shell:") { Foreground = new SolidColorBrush(Colors.White) });
                        Output.Inlines.Add(new Run("1.0.0.0" + Environment.NewLine) { Foreground = new SolidColorBrush(Colors.LimeGreen) });
                    });
                }
                else
                {
                    if (cmd.Trim() == "")
                    {

                    }
                    else
                    {
                        Diagnotor.CurrentDiagnotor.LogError($"Command: {cmd} not found!");
                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
    public class AdvancedDiagnotor : IDiagnotor
    {
        TextBlock block;
        Dispatcher Dispatcher;
        public AdvancedDiagnotor(TextBlock textBlock, Dispatcher d)
        {
            Dispatcher = d;
            block = textBlock;
        }
        public void Log(string str)
        {
            Dispatcher.Invoke(() =>
            {
                block.Inlines.Add(new Run(str + Environment.NewLine) { Foreground = new SolidColorBrush(Colors.White) });
            });
        }

        public void LogError(string str)
        {
            Dispatcher.Invoke(() =>
            {
                block.Inlines.Add(new Run(str + Environment.NewLine) { Foreground = new SolidColorBrush(Colors.Red) });
            });
        }

        public void LogSuccess(string str)
        {
            Dispatcher.Invoke(() =>
            {
                block.Inlines.Add(new Run(str + Environment.NewLine) { Foreground = new SolidColorBrush(Colors.LimeGreen) });

            });
        }

        public void LogWarning(string str)
        {
            Dispatcher.Invoke(() =>
            {
                block.Inlines.Add(new Run(str + Environment.NewLine) { Foreground = new SolidColorBrush(Colors.White) });
            });
        }
    }
}
