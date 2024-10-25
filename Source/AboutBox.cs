namespace GBSharp
{
	partial class AboutBox : Form
	{
		public AboutBox()
		{
			InitializeComponent();
		}

		private void LabelWebsiteLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.labelWebsite.LinkVisited = true;

			System.Diagnostics.Process linkProcess = new();
			linkProcess.StartInfo.FileName = this.labelWebsite.Text;
			linkProcess.StartInfo.UseShellExecute = true;
			linkProcess.Start();
		}
	}
}
