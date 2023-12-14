using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RazorClassLibrary
{
	public class UiState : IUiState
	{
		public event Func<Task>? ActivateApplicationClose;
		public event PropertyChangedEventHandler? PropertyChanged;
		public event PropertyChangingEventHandler? PropertyChanging;

		public void CloseApplication()
		{
			// windows forms is an actual framework not a reference it will not work here!
			//System.Windows.Forms.Application.Exit();
		}
	}
}
