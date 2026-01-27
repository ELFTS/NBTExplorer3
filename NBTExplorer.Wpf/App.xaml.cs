using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using NBTExplorer.Wpf.Views;
using NBTExplorer.Wpf.Services;
using NBTExplorer.Wpf.ViewModels;
using NBTModel.Interop;

namespace NBTExplorer.Wpf
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Setup exception handling
            AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Initialize clipboard controller
            NbtClipboardController.Initialize(new NbtClipboardControllerWpf());

            // Register WPF dialogs for NBTModel
            FormRegistry.EditString = (data) =>
            {
                var window = new StringEditorWindow(data);
                window.Owner = MainWindow;
                return window.ShowDialog() == true;
            };

            FormRegistry.RenameTag = (data) =>
            {
                var window = new StringEditorWindow(data);
                window.Title = "Rename Tag";
                window.Owner = MainWindow;
                return window.ShowDialog() == true;
            };

            FormRegistry.EditTagScalar = (data) =>
            {
                var window = new ValueEditorWindow(data);
                window.Owner = MainWindow;
                return window.ShowDialog() == true;
            };

            FormRegistry.CreateNode = (data) =>
            {
                var window = new CreateNodeWindow(data);
                window.Owner = MainWindow;
                return window.ShowDialog() == true;
            };

            FormRegistry.MessageBox = (message) =>
            {
                MessageBox.Show(message, "NBTExplorer", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            
            FormRegistry.EditByteArray = (data) =>
            {
                var window = new HexEditorWindow(data.ToString(), data.Data, data.BytesPerElement);
                window.Owner = MainWindow;
                bool? result = window.ShowDialog();
                if (result == true && window.Modified)
                {
                    data.Data = window.Data;
                    return true;
                }
                return false;
            };
            
            // Register exit event handler
            Exit += App_Exit;
        }
        
        private void App_Exit(object sender, ExitEventArgs e)
        {
            // Save settings when application exits
            if (MainWindow?.DataContext is MainViewModel vm)
            {
                // Use the existing SettingsService instance from MainViewModel
                vm.SettingsService.Save();
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ProcessException(e.Exception);
            e.Handled = true;
        }

        private void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ProcessException(ex);
            }
            else if (e.IsTerminating)
            {
                MessageBox.Show("NBTExplorer 3.0 encountered an unknown exception object: " + e.ExceptionObject.GetType().FullName,
                    "NBTExplorer 3.0 failed to run", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void ProcessException(Exception ex)
        {
            if (IsMissingSubstrate(ex))
            {
                MessageBox.Show("NBTExplorer 3.0 could not find required assembly \"Substrate.dll\".\n\nIf you obtained NBTExplorer 3.0 from a ZIP distribution, make sure you've extracted NBTExplorer 3.0 and all of its supporting files into another directory before running it.",
                    "NBTExplorer 3.0 failed to run", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            if (IsMissingNBTModel(ex))
            {
                MessageBox.Show("NBTExplorer 3.0 could not find required assembly \"NBTModel.dll\".\n\nIf you obtained NBTExplorer 3.0 from a ZIP distribution, make sure you've extracted NBTExplorer 3.0 and all of its supporting files into another directory before running it.",
                    "NBTExplorer 3.0 failed to run", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            StringBuilder errorText = new StringBuilder();
            errorText.AppendLine("NBTExplorer 3.0 encountered the following exception while trying to run: " + ex.GetType().Name);
            errorText.AppendLine("Message: " + ex.Message);

            Exception ix = ex;
            while (ix.InnerException != null)
            {
                ix = ix.InnerException;
                errorText.AppendLine();
                errorText.AppendLine("Caused by Inner Exception: " + ix.GetType().Name);
                errorText.AppendLine("Message: " + ix.Message);
            }

            try
            {
                string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NBTExplorer 3.0");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string logPath = Path.Combine(logDir, "error.log");
                using (var writer = new StreamWriter(logPath, true))
                {
                    writer.WriteLine("NBTExplorer 3.0 Error Report");
                    writer.WriteLine(DateTime.Now);
                    writer.WriteLine("-------");
                    writer.WriteLine(errorText);
                    writer.WriteLine("-------");

                    ix = ex;
                    while (ix != null)
                    {
                        writer.WriteLine(ex.StackTrace);
                        writer.WriteLine("-------");
                        ix = ix.InnerException;
                    }

                    writer.WriteLine();
                }

                errorText.AppendLine();
                errorText.AppendLine("Additional error detail has been written to:\n" + logPath);
            }
            catch { }

            MessageBox.Show(errorText.ToString(), "NBTExplorer 3.0 failed to run", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }

        private bool IsMissingSubstrate(Exception ex)
        {
            if (ex is TypeInitializationException && ex.InnerException != null)
                ex = ex.InnerException;
            if (ex is FileNotFoundException)
            {
                FileNotFoundException fileEx = ex as FileNotFoundException;
                if (fileEx.FileName.Contains("Substrate"))
                    return true;
            }

            return false;
        }

        private bool IsMissingNBTModel(Exception ex)
        {
            if (ex is TypeInitializationException && ex.InnerException != null)
                ex = ex.InnerException;
            if (ex is FileNotFoundException)
            {
                FileNotFoundException fileEx = ex as FileNotFoundException;
                if (fileEx.FileName.Contains("NBTModel"))
                    return true;
            }

            return false;
        }
    }
}
