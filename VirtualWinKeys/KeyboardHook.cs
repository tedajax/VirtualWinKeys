using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// Just the example given here given a bit of polish to fit into the broader application a bit better.
// https://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp

namespace VirtualWinKeys
{
	struct ModifierShortcut : IEquatable<ModifierShortcut>
	{
		public ModifierKeys modifier;
		public Keys key;

		public override int GetHashCode()
		{
			return ((int)key & 0xFF) | ((int)modifier << 16);
		}

		public override bool Equals(object obj)
		{
			return Equals((ModifierShortcut)obj);
		}

		public bool Equals(ModifierShortcut other)
		{
			return modifier == other.modifier && key == other.key;
		}

		public static bool operator==(ModifierShortcut a, ModifierShortcut b)
		{
			return a.modifier == b.modifier && a.key == b.key;
		}

		public static bool operator!=(ModifierShortcut a, ModifierShortcut b)
		{
			return !(a == b);
		}
	}

	public class ShortcutManager
	{
		private Dictionary<ModifierShortcut, Action> shortcutCallbacks = new Dictionary<ModifierShortcut, Action>();

		private KeyboardHook hook = new KeyboardHook();

		public ShortcutManager()
		{
			hook.KeyPressed += OnHookKeyPressed;
		}

		private void OnHookKeyPressed(object sender, KeyPressedEventArgs e)
		{
			ModifierShortcut shortcut = new ModifierShortcut()
			{
				modifier = e.Modifier,
				key = e.Key,
			};

			if (shortcutCallbacks.ContainsKey(shortcut))
			{
				shortcutCallbacks[shortcut]();
			}
		}

		public void RegisterShortcut(ModifierKeys modifier, Keys key, Action callback)
		{
			ModifierShortcut shortcut = new ModifierShortcut()
			{
				modifier = modifier,
				key = key,
			};

			if (shortcutCallbacks.ContainsKey(shortcut))
			{
				// no duplicate handling right now
				return;
			}

			hook.RegisterHotKey(modifier, key);
			shortcutCallbacks.Add(shortcut, callback);
		}
	}

	public sealed class KeyboardHook : IDisposable
	{
		// Registers a hot key with Windows.
		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		// Unregisters the hot key with Windows.
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		/// <summary>
		/// Represents the window that is used internally to get the messages.
		/// </summary>
		private class Window : NativeWindow, IDisposable
		{
			private static int WM_HOTKEY = 0x0312;

			public Window()
			{
				// create the handle for the window.
				this.CreateHandle(new CreateParams());
			}

			/// <summary>
			/// Overridden to get the notifications.
			/// </summary>
			/// <param name="m"></param>
			protected override void WndProc(ref Message m)
			{
				base.WndProc(ref m);

				// check if we got a hot key pressed.
				if (m.Msg == WM_HOTKEY)
				{
					// get the keys.
					Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
					ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

					// invoke the event to notify the parent.
					if (KeyPressed != null)
						KeyPressed(this, new KeyPressedEventArgs(modifier, key));
				}
			}

			public event EventHandler<KeyPressedEventArgs> KeyPressed;

			#region IDisposable Members

			public void Dispose()
			{
				this.DestroyHandle();
			}

			#endregion
		}

		private Window _window = new Window();
		private int _currentId;

		public KeyboardHook()
		{
			// register the event of the inner native window.
			_window.KeyPressed += delegate (object sender, KeyPressedEventArgs args)
			{
				if (KeyPressed != null)
					KeyPressed(this, args);
			};
		}

		/// <summary>
		/// Registers a hot key in the system.
		/// </summary>
		/// <param name="modifier">The modifiers that are associated with the hot key.</param>
		/// <param name="key">The key itself that is associated with the hot key.</param>
		public void RegisterHotKey(ModifierKeys modifier, Keys key)
		{
			// increment the counter.
			_currentId = _currentId + 1;

			// register the hot key.
			if (!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
				throw new InvalidOperationException("Couldn’t register the hot key.");
		}

		/// <summary>
		/// A hot key has been pressed.
		/// </summary>
		public event EventHandler<KeyPressedEventArgs> KeyPressed;

		#region IDisposable Members

		public void Dispose()
		{
			// unregister all the registered hot keys.
			for (int i = _currentId; i > 0; i--)
			{
				UnregisterHotKey(_window.Handle, i);
			}

			// dispose the inner native window.
			_window.Dispose();
		}

		#endregion
	}

	/// <summary>
	/// Event Args for the event that is fired after the hot key has been pressed.
	/// </summary>
	public class KeyPressedEventArgs : EventArgs
	{
		public ModifierKeys Modifier { get; private set; }
		public Keys Key { get; private set; }

		internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
		{
			Modifier = modifier;
			Key = key;
		}
	}

	/// <summary>
	/// The enumeration of possible modifiers.
	/// </summary>
	[Flags]
	public enum ModifierKeys : uint
	{
		None = 0,
		Alt = 1,
		Control = 2,
		Shift = 4,
		Win = 8
	}
}