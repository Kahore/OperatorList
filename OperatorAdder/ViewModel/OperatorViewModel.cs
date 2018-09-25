using System;
using System.Collections.Generic;
/*------------------------------------------------*/
using Newtonsoft.Json;
using OperatorAdder.Model;

namespace OperatorAdder.ViewModel
{
	public class OperatorViewModel : ViewModelBase
	{
		OperatorModel _operatorModel;
		
		private List<OperatorViewModel> list;

		public OperatorViewModel(OperatorModel operatorModel) => _operatorModel = operatorModel;

		public OperatorViewModel(OperatorViewModel vmOperatorModel) => _operatorModel = new OperatorModel { Id = vmOperatorModel.Id, UserName = vmOperatorModel.UserName };

		public OperatorViewModel(List<OperatorViewModel> list) => this.list = list;

		[JsonConstructor]
		public OperatorViewModel(Guid id, string userName) => _operatorModel = new OperatorModel { Id = id, UserName = userName};

		public string UserName { get => _operatorModel.UserName; set { _operatorModel.UserName = value; NotifyPropertyChanged("UserName"); } }

		public Guid Id { get => _operatorModel.Id; set => _operatorModel.Id = value; }
	}
}
