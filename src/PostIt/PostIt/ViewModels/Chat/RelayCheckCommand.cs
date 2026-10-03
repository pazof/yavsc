using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PostIt.ViewModels.Chat
{
    internal class RelayCheckCommand : ICommand, IWithLabel
    {
        public string Label { get; }
        private Func<Task> value;

        public RelayCheckCommand(string v, Func<Task> value)
        {
            this.Label = v;
            this.value = value;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return value != null;
        }

        public void Execute(object? parameter)
        {
            if (value != null)
            {
                _ = value();
            }
        }
    }
}
