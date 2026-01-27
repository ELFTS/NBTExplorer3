using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NBTExplorer.Wpf.ViewModels;

namespace NBTExplorer.Wpf.Views
{
    public partial class ValueRuleWindow : Window
    {
        public ValueRuleWindow(List<string> operators)
        {
            InitializeComponent();
            DataContext = new ValueRuleViewModel(operators, this);
        }
        
        public ValueRuleViewModel ViewModel => DataContext as ValueRuleViewModel;
        
        public string TagName
        {
            get => ViewModel.TagName;
            set => ViewModel.TagName = value;
        }
        
        public string TagValue
        {
            get => ViewModel.TagValue;
            set => ViewModel.TagValue = value;
        }
        
        public string Operator
        {
            get => ViewModel.Operator;
            set => ViewModel.Operator = value;
        }
        
        public long TagValueAsLong => ViewModel.TagValueAsLong;
        public double TagValueAsDouble => ViewModel.TagValueAsDouble;
    }
    
    public class ValueRuleViewModel : ViewModelBase
    {
        private Window _window;
        private string _tagName;
        private string _tagValue;
        private string _operator;
        private List<string> _operators;
        
        public ValueRuleViewModel(List<string> operators, Window window)
        {
            _window = window;
            Operators = operators;
            
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }
        
        public string TagName
        {
            get => _tagName;
            set => SetProperty(ref _tagName, value);
        }
        
        public string TagValue
        {
            get => _tagValue;
            set => SetProperty(ref _tagValue, value);
        }
        
        public string Operator
        {
            get => _operator;
            set => SetProperty(ref _operator, value);
        }
        
        public List<string> Operators
        {
            get => _operators;
            set => SetProperty(ref _operators, value);
        }
        
        public long TagValueAsLong
        {
            get
            {
                if (long.TryParse(TagValue, out long result))
                    return result;
                return 0;
            }
        }
        
        public double TagValueAsDouble
        {
            get
            {
                if (double.TryParse(TagValue, out double result))
                    return result;
                return 0;
            }
        }
        
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
        
        private void Ok(object parameter)
        {
            _window.DialogResult = true;
            _window.Close();
        }
        
        private void Cancel(object parameter)
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}