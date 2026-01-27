using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NBTExplorer.Model.Search;
using NBTExplorer.Wpf.ViewModels;

namespace NBTExplorer.Wpf.Views
{
    public partial class StringRuleWindow : Window
    {
        private StringTagRule _rule;
        
        public StringRuleWindow(StringTagRule rule)
        {
            InitializeComponent();
            _rule = rule;
            
            DataContext = new StringRuleViewModel(rule, this);
        }
    }
    
    public class StringRuleViewModel : ViewModelBase
    {
        private StringTagRule _rule;
        private Window _window;
        private string _tagName;
        private string _tagValue;
        private string _operator;
        private List<string> _operators;
        
        public StringRuleViewModel(StringTagRule rule, Window window)
        {
            _rule = rule;
            _window = window;
            
            TagName = rule.Name;
            TagValue = rule.Value;
            Operator = SearchRule.StringOpStrings[rule.Operator];
            Operators = new List<string>(SearchRule.StringOpStrings.Values);
            
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
        
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
        
        private void Ok(object parameter)
        {
            _rule.Name = TagName;
            _rule.Value = TagValue;
            _rule.Operator = SearchRule.StringOpStrings.First(x => x.Value == Operator).Key;
            
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