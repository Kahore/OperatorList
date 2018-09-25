using OperatorAdder.Model;
using System.Collections.Generic;

namespace OperatorAdder.ViewModel
{
   public class SelectedOperatorViewModel:ViewModelBase
    {
		SelectedOperatorModel _selectedOperatorModel;

		private List<SelectedOperatorViewModel> list;

		public SelectedOperatorViewModel(SelectedOperatorModel selectedOperatorModel) => _selectedOperatorModel = selectedOperatorModel;

		public SelectedOperatorViewModel(SelectedOperatorViewModel vmSelectedOperatorModel) => _selectedOperatorModel = new SelectedOperatorModel { SelectedUserName = vmSelectedOperatorModel.SelectedUserName };

		public SelectedOperatorViewModel(List<SelectedOperatorViewModel> list) => this.list = list;

		public SelectedOperatorViewModel(string selectedUserName) => _selectedOperatorModel = new SelectedOperatorModel { SelectedUserName = selectedUserName };

		public string SelectedUserName { get => _selectedOperatorModel.SelectedUserName; set { _selectedOperatorModel.SelectedUserName = value; NotifyPropertyChanged("SelectedUserName"); } }
	}
}
