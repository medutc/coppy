using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Tesseract;
using Microsoft.Win32;
using System;

public void EnableStartup()
{
    // Opens the current user's startup registry key
    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
    {
        // Registers the exact path of your .exe to launch on boot
        if (Environment.ProcessPath != null)
        {
            key?.SetValue("CoppyApp", Environment.ProcessPath);
        }
    }
}
namespace coppy
{
    public class TrayContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private string firstRunFile = "setup_complete.txt";

        public TrayContext()
        {
            if (!File.Exists(firstRunFile))
            {
                MessageBox.Show(
                    "Welcome to coppy!\n\n" +
                    "To use the app:\n" +
                    "1. Click the icon in your system tray (bottom right).\n" +
                    "2. Your screen will darken.\n" +
                    "3. Click and drag over any text on your screen.\n" +
                    "4. Release the mouse. The text will automatically be copied to your clipboard!",
                    "First Use Tutorial", MessageBoxButtons.OK, MessageBoxIcon.Information);

                File.Create(firstRunFile).Close();
            }

            trayIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Application,
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true,
                Text = "coppy - Click to capture text"
            };

            trayIcon.ContextMenuStrip.Items.Add("Exit", null, ExitApp);
            trayIcon.MouseClick += TrayIcon_MouseClick;
            trayIcon.Icon = new Icon("coppyIcon.ico");
        }

        private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                using (OverlayForm overlay = new OverlayForm())
                {
                    if (overlay.ShowDialog() == DialogResult.OK)
                    {
                        CaptureAndExtractText(overlay.SelectedBounds);
                    }
                }
            }
        }

        private void CaptureAndExtractText(Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            try
            {
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                    }

                    using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                    {
                        // FIX: Convert the standard C# Bitmap into Tesseract's Pix format
                        using (var pix = PixConverter.ToPix(bitmap))
                        {
                            // Pass the newly created 'pix' to the engine instead of 'bitmap'
                            using (var page = engine.Process(pix))
                            {
                                string extractedText = page.GetText().Trim();

                                if (!string.IsNullOrEmpty(extractedText))
                                {
                                    Clipboard.SetText(extractedText);
                                    trayIcon.ShowBalloonTip(2000, "Success", "Text copied to clipboard!", ToolTipIcon.Info);
                                }
                                else
                                {
                                    trayIcon.ShowBalloonTip(2000, "No Text Found", "Could not detect any text in that area.", ToolTipIcon.Warning);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during OCR extraction: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExitApp(object? sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
