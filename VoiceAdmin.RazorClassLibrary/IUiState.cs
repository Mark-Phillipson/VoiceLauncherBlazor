using System.ComponentModel;

namespace RazorClassLibrary
{
	public interface IUiState : INotifyPropertyChanged, INotifyPropertyChanging
	{
		event Func<Task>? ActivateApplicationClose;
		void CloseApplication();
	}
}