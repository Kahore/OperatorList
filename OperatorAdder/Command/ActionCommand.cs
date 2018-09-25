using System;
using System.Windows.Input;

namespace OperatorAdder.Command
{
	public class ActionCommand : ICommand
	{
		private readonly Action<Object> action;
		private readonly Predicate<Object> predicate;

		public ActionCommand(Action<Object> action) : this(action, null)
		{
		}

		public ActionCommand(Action<Object> action, Predicate<Object> predicate)
		{
			this.action = action ?? throw new ArgumentNullException("action", "You must specify an Acion<T>");
			this.predicate = predicate;
		}

		#region Implementation of ICommand

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested += value; }
		}

		public bool CanExecute(object parameter)
		{
			if (predicate == null)
			{
				return true;
			}
			return predicate(parameter);
		}

		public void Execute(object parameter)
		{
			action(parameter);
		}

		public void Execute()
		{
			Execute(null);
		}

		#endregion Implementation of ICommand
	}
}