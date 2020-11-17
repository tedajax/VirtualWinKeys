using System;
using System.Drawing;
using System.Windows.Forms;

namespace VirtualWinKeys
{

	public class VirtualWinKeysApp : Form
	{
		static VirtualWinKeysApp app;

		[STAThread]
		public static void Main()
		{
			app = new VirtualWinKeysApp();
			Application.Run();
		}

		private NotifyIcon trayIcon;
		private ContextMenu trayMenu;

		ShortcutManager shortcuts = new ShortcutManager();

		public VirtualWinKeysApp()
		{
			shortcuts.RegisterShortcut(VirtualWinKeys.ModifierKeys.Alt | VirtualWinKeys.ModifierKeys.Win, Keys.Right, OnMoveActiveWindowRight);
			shortcuts.RegisterShortcut(VirtualWinKeys.ModifierKeys.Alt | VirtualWinKeys.ModifierKeys.Win, Keys.Left, OnMoveActiveWindowLeft);

			trayMenu = new ContextMenu();
			trayMenu.MenuItems.Add("Exit", OnRequestExit);

			trayIcon = new NotifyIcon();
			trayIcon.Text = "VirtualWinKeys";
			trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);

			trayIcon.ContextMenu = trayMenu;
			trayIcon.Visible = true;
		}

		private void OnMoveActiveWindowRight()
		{
			var destination = VirtualDesktop.Desktop.Current.Right;
			if (destination != null)
			{
				destination.MoveActiveWindow();
				destination.MakeVisible();
			}
		}

		private void OnMoveActiveWindowLeft()
		{
			var destination = VirtualDesktop.Desktop.Current.Left;
			if (destination != null)
			{
				destination.MoveActiveWindow();
				destination.MakeVisible();
			}
		}

		private void OnRequestExit(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void Alert(string message)
		{
			MessageBox.Show(message, "alert", MessageBoxButtons.OK);
		}

		private bool OnKeyDown(Keys key)
		{
			Alert(key.ToString());
			return false;
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				trayIcon.Dispose();
			}

			base.Dispose(isDisposing);
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// VirtualWinKeysApp
			// 
			this.ClientSize = new System.Drawing.Size(823, 555);
			this.Name = "VirtualWinKeysApp";
			this.Opacity = 0D;
			this.ResumeLayout(false);

		}
	}
}