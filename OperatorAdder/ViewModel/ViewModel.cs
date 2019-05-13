using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;
/*================================================*/
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OperatorAdder.Command;

namespace OperatorAdder.ViewModel
{
	public class ViewModel:ViewModelBase
	{
		private readonly string pathToWatch;
		private readonly string logFile;
		private readonly string dest;
		private const string FileToWatch = "*.txt";
		private string fileText;
		private OperatorViewModel _operator;
		private ObservableCollection<OperatorViewModel> _operators;

		public string FileText
		{
			get => fileText;
			set
			{
				if (fileText == value) return;
				fileText = value;
				NotifyPropertyChanged("FileText");
			}
		}

		public ObservableCollection<OperatorViewModel> Operators { get => _operators; set { _operators = value; NotifyPropertyChanged("Operators"); } }

		public OperatorViewModel Operator { get => _operator; set { _operator = value; NotifyPropertyChanged("Operator"); } }

		private SelectedOperatorViewModel _selectedOperator;

		private ObservableCollection<SelectedOperatorViewModel> _selectedOperators;

		public ObservableCollection<SelectedOperatorViewModel> SelectedOperators { get => _selectedOperators; set { _selectedOperators = value; NotifyPropertyChanged("SelectedOperators"); } }

		public SelectedOperatorViewModel SelectedOperator { get => _selectedOperator; set { _selectedOperator = value; NotifyPropertyChanged("SelectedOperator"); } }

		public ViewModel()
		{
			Operators = new ObservableCollection<OperatorViewModel>();
			SelectedOperators = new ObservableCollection<SelectedOperatorViewModel>();

			CheckAllDir();

			RefreshAll();
			Operators.CollectionChanged += new NotifyCollectionChangedEventHandler(Operators_CollectionChanged);

			pathToWatch = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\Export\";
			logFile = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\log.txt";

			//pathToWatch = Properties.Settings.Default.PathToInstrumentFolder;
			dest = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\Export\done\";

            //dest = Properties.Settings.Default.PathToExport;
            RunWatch();
		}
        /// <summary>
        /// Create all environment folder
        /// </summary>
        private void CheckAllDir()
		{
			Directory.CreateDirectory(Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\");
			Directory.CreateDirectory(Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\Export\");
			Directory.CreateDirectory(Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\Export\done\");
		}
		/// <summary>
        /// Await the export file from instrument
        /// </summary>
	public void RunWatch()
		{
			var watcher = new FileSystemWatcher
			{
				NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime,

				Path = pathToWatch,
				Filter = FileToWatch
			};
			watcher.Created += OnChanged;
			watcher.EnableRaisingEvents = true;
		}

		private void OnChanged(object source, FileSystemEventArgs e)
		{

			try
			{
				if (e.ChangeType == WatcherChangeTypes.Created)
				{
					string replacement = String.Empty;

					StreamReader readerOperatorToAppend = new StreamReader(Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\SelectedOperator.txt");
					string OperatorToAppend = readerOperatorToAppend.ReadToEnd();
					readerOperatorToAppend.Close();
					readerOperatorToAppend.Dispose();
					
					OperatorToAppend = ";OPERATOR: " + OperatorToAppend + "\r\n";

					while (!IsFileReady(e.FullPath)) { }

					StreamReader myReader = new StreamReader(e.FullPath);

					replacement = Regex.Replace(myReader.ReadToEnd(), @"\t|\n|\r", "");
						
						replacement = replacement + OperatorToAppend;
						myReader.Close();
						myReader.Dispose();

						StreamWriter myWriter = new StreamWriter(e.FullPath);

						myWriter.Write(replacement);

					myWriter.Close();
					myWriter.Dispose();

                    if ( !File.Exists(dest + e.Name) )
                    {
                        File.Move( e.FullPath, dest + e.Name );
                    }
                    else {
                        File.Delete( e.FullPath );
                    }
                       
                    GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();
				}
			}
			catch (FileNotFoundException ex)
			{
				FileText = "File not found.";
				File.AppendAllText(logFile, FileText + ' ' + e.Name + ' ' + ex.ToString());

			}
			catch (FileLoadException ex)
			{
				FileText = "File Failed to load";
				File.AppendAllText(logFile, FileText + ' ' + e.Name + ' ' + ex.ToString());
			}
			catch (IOException ex)
			{
				FileText = "File I/O Error";
				File.AppendAllText(logFile, FileText + ' ' + e.Name + ' ' + ex.ToString());
			}
			catch (Exception err)
			{
				FileText = err.Message;
				File.AppendAllText(logFile, FileText + ' ' + e.Name + ' ' + err.ToString());
			}
			Thread.Sleep(1000);
		}

        /// <summary>
        ///  If the file can be opened for exclusive access it means that the file
        ///  is no longer locked by another process.
        /// </summary>
        /// <param name="filename">Curent filename that we check</param>
        /// <returns>bool value</returns>
		public static bool IsFileReady(string filename)
		{
			try
			{
				using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
					return inputStream.Length > 0;
			}
			catch (Exception)
			{
				return false;
			}
		}

		#region ICommand _addCommand;
		private ICommand _addCommand;
		public ICommand AddCommand
		{
			get
			{
				if (_addCommand == null)
				{
					_addCommand = new ActionCommand(param => Add(),
						null);
				}
				return _addCommand;
			}
		}

		private void Add()
		{
			Operator = new OperatorViewModel(Guid.NewGuid(), Operator.UserName);
			Operators.Add(Operator);

			var filePath = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\Operator_Tech.json";
			if (!File.Exists(filePath))
			{
				using (StreamWriter file = File.CreateText(filePath))
				{
					JsonSerializer serializer = new JsonSerializer();
					serializer.Serialize(file,  Operator);
				}
			}
			else
			{
				string json = File.ReadAllText(filePath);

				var list = JsonConvert.DeserializeObject<List<OperatorViewModel>>(json);

				bool IsListContainsUserName = list.Any(x => x.UserName == _operator.UserName);
				if (!IsListContainsUserName)
				{
					list.Add(new OperatorViewModel(_operator));
					var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);
					File.WriteAllText(filePath, convertedJson);
				}
			}
			RefreshAll();

		}
		#endregion

		#region ICommand _deleteCommand
		private ICommand _deleteCommand;
		public ICommand DeleteCommand
		{
			get
			{
				if (_deleteCommand == null)
				{
					_deleteCommand = new ActionCommand(param => Delete(Operator.Id,Operator.UserName),
						null);
				}
				return _deleteCommand;
			}
		}
		void Delete(Guid id, string username)
		{
			//var filePath = @"c:\movie.json";
			var filePath = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\Operator_Tech.json";
			string json = File.ReadAllText(filePath);
			
			JArray array = JArray.Parse(json);

			foreach (JProperty content in array.Children<JObject>().Properties())
			{
				if (content.Value.ToString() == id.ToString()) {
						content.Parent.Remove();
							var convertedJson = JsonConvert.SerializeObject(array, Formatting.Indented);
								File.WriteAllText(filePath, convertedJson);

					RefreshAll();
					break;
				}
			}
		}
		#endregion

		#region  ICommand _editCommand
		private ICommand _editCommand;
		public ICommand EditCommand { 
			get
			{
				if (_editCommand == null)
				{
					_editCommand = new ActionCommand(param => Edit(Operator.Id, Operator.UserName),
						null);
				}
				return _editCommand;
			}	
		}

		void Edit (Guid id, string username)
		{
			//var filePath = @"c:\movie.json";
			var filePath = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\Operator_Tech.json";
			string json = File.ReadAllText(filePath);

			JArray array = JArray.Parse(json);

			foreach (JProperty content in array.Children<JObject>().Properties())
			{
				if (content.Value.ToString() == id.ToString())
				{
					content.Parent.First.Remove();
						JProperty jTitle = new JProperty("UserName", username);
							content.Parent.AddFirst(jTitle);
					var convertedJson = JsonConvert.SerializeObject(array, Formatting.Indented);
						File.WriteAllText(filePath, convertedJson);

					RefreshAll();
					break;
				}
			}
		}

		#endregion

		#region  ICommand _selectUserCommand
		private ICommand _selectUserCommand;
		public ICommand SelectUserCommand
		{
			get
			{
				if (_selectUserCommand == null)
				{
					_selectUserCommand = new ActionCommand(param => SelectUserForExport(Operator.UserName),
						null);
				}
				return _selectUserCommand;
			}
		}
		void SelectUserForExport(string userNameForExport)
		{
			//var filePath = @"c:\SelectedOperator.txt";
			var filePath = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\SelectedOperator.txt";
			//EltraFormat
			File.WriteAllText(filePath, userNameForExport);

			RefreshAll();
		}
		#endregion

		void Operators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyPropertyChanged("Operators");
		}

		void RefreshAll()
		{
			Operators.Clear();
			SelectedOperators.Clear();

			var filePath = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\Operator_Tech.json";
			if (File.Exists(filePath))
			{
				using (StreamReader r = new StreamReader(filePath))
				{
					string jsonData = File.ReadAllText(filePath);

					var list = JsonConvert.DeserializeObject<List<OperatorViewModel>>(jsonData);
					var descListOb = list.OrderBy(x => x.UserName);
					descListOb.ToList().ForEach(Operators.Add);
				}
			}
			else
			{				
					var newTestOperator = "[{\"UserName\":\"TestUser\",\"Id\":\"" + Guid.NewGuid() + "\"}]";
					File.WriteAllText(filePath, newTestOperator);		
			}
			Operator = new OperatorViewModel(Guid.NewGuid(), "");
			
			var fileOperatorPath = Environment.GetEnvironmentVariable("UserProfile") + @"\LabOperatorSGS\SelectedOperator.txt";
			if (File.Exists(fileOperatorPath))
			{
				using (StreamReader reader = new StreamReader(fileOperatorPath))
				{
					string currentLine;
					currentLine = reader.ReadLine();
					SelectedOperator = new SelectedOperatorViewModel(currentLine);
						SelectedOperators.Add(new SelectedOperatorViewModel(currentLine));
				}
			}
			else
			{
				File.WriteAllText(fileOperatorPath,"");
			}
		
		}
	}
}
