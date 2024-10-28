namespace GBSharp
{
	public partial class ControlsForm : Form
	{
		private KeyMapping? _keyMapping = null;

		public ControlsForm(KeyMapping keyMapping)
		{
			InitializeComponent();

			// Fill out the drop downs with all possible keys, then select the current key mapping.
			_keyMapping = keyMapping;
			string[] keys = Enum.GetNames(typeof(Keys));
			up1ComboBox.Items.AddRange(keys);
			up1ComboBox.SelectedIndex = up1ComboBox.FindStringExact(_keyMapping.Up1Key.ToString());
			left1ComboBox.Items.AddRange(keys);
			left1ComboBox.SelectedIndex = left1ComboBox.FindStringExact(_keyMapping.Left1Key.ToString());
			down1ComboBox.Items.AddRange(keys);
			down1ComboBox.SelectedIndex = down1ComboBox.FindStringExact(_keyMapping.Down1Key.ToString());
			right1ComboBox.Items.AddRange(keys);
			right1ComboBox.SelectedIndex = right1ComboBox.FindStringExact(_keyMapping.Right1Key.ToString());
			a1ComboBox.Items.AddRange(keys);
			a1ComboBox.SelectedIndex = a1ComboBox.FindStringExact(_keyMapping.A1Key.ToString());
			b1ComboBox.Items.AddRange(keys);
			b1ComboBox.SelectedIndex = b1ComboBox.FindStringExact(_keyMapping.B1Key.ToString());
			start1ComboBox.Items.AddRange(keys);
			start1ComboBox.SelectedIndex = start1ComboBox.FindStringExact(_keyMapping.Start1Key.ToString());
			select1ComboBox.Items.AddRange(keys);
			select1ComboBox.SelectedIndex = select1ComboBox.FindStringExact(_keyMapping.Select1Key.ToString());
			up2ComboBox.Items.AddRange(keys);
			up2ComboBox.SelectedIndex = up2ComboBox.FindStringExact(_keyMapping.Up2Key.ToString());
			left2ComboBox.Items.AddRange(keys);
			left2ComboBox.SelectedIndex = left2ComboBox.FindStringExact(_keyMapping.Left2Key.ToString());
			down2ComboBox.Items.AddRange(keys);
			down2ComboBox.SelectedIndex = down2ComboBox.FindStringExact(_keyMapping.Down2Key.ToString());
			right2ComboBox.Items.AddRange(keys);
			right2ComboBox.SelectedIndex = right2ComboBox.FindStringExact(_keyMapping.Right2Key.ToString());
			a2ComboBox.Items.AddRange(keys);
			a2ComboBox.SelectedIndex = a2ComboBox.FindStringExact(_keyMapping.A2Key.ToString());
			b2ComboBox.Items.AddRange(keys);
			b2ComboBox.SelectedIndex = b2ComboBox.FindStringExact(_keyMapping.B2Key.ToString());
			start2ComboBox.Items.AddRange(keys);
			start2ComboBox.SelectedIndex = start2ComboBox.FindStringExact(_keyMapping.Start2Key.ToString());
			select2ComboBox.Items.AddRange(keys);
			select2ComboBox.SelectedIndex = select2ComboBox.FindStringExact(_keyMapping.Select2Key.ToString());

			// NOTE: Handle selection changes after the initial values have been assigned.
			up1ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			left1ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			down1ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			right1ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			a1ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			b1ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			start1ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			select1ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			up2ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			left2ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			down2ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			right2ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			a2ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			b2ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			start2ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
			select2ComboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
		}

		private void ComboBoxSelectedIndexChanged(object? sender, EventArgs e)
		{
			if (_keyMapping != null)
			{
				Keys key;
				_ = Enum.TryParse(up1ComboBox.SelectedItem as string, out key);
				_keyMapping.Up1Key = key;
				_ = Keys.TryParse(left1ComboBox.SelectedItem as string, out key);
				_keyMapping.Left1Key = key;
				_ = Keys.TryParse(down1ComboBox.SelectedItem as string, out key);
				_keyMapping.Down1Key = key;
				_ = Keys.TryParse(right1ComboBox.SelectedItem as string, out key);
				_keyMapping.Right1Key = key;
				_ = Keys.TryParse(a1ComboBox.SelectedItem as string, out key);
				_keyMapping.A1Key = key;
				_ = Keys.TryParse(b1ComboBox.SelectedItem as string, out key);
				_keyMapping.B1Key = key;
				_ = Keys.TryParse(start1ComboBox.SelectedItem as string, out key);
				_keyMapping.Start1Key = key;
				_ = Keys.TryParse(select1ComboBox.SelectedItem as string, out key);
				_keyMapping.Select1Key = key;
				_ = Keys.TryParse(up2ComboBox.SelectedItem as string, out key);
				_keyMapping.Up2Key = key;
				_ = Keys.TryParse(left2ComboBox.SelectedItem as string, out key);
				_keyMapping.Left2Key = key;
				_ = Keys.TryParse(down2ComboBox.SelectedItem as string, out key);
				_keyMapping.Down2Key = key;
				_ = Keys.TryParse(right2ComboBox.SelectedItem as string, out key);
				_keyMapping.Right2Key = key;
				_ = Keys.TryParse(a2ComboBox.SelectedItem as string, out key);
				_keyMapping.A2Key = key;
				_ = Keys.TryParse(b2ComboBox.SelectedItem as string, out key);
				_keyMapping.B2Key = key;
				_ = Keys.TryParse(start2ComboBox.SelectedItem as string, out key);
				_keyMapping.Start2Key = key;
				_ = Keys.TryParse(select2ComboBox.SelectedItem as string, out key);
				_keyMapping.Select2Key = key;
			}
		}
	}
}
