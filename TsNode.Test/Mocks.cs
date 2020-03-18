using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using TsNode.Controls;
using TsNode.Interface;

namespace TsNode.Test
{
    public class TestNodeContext : INodeDataContext
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsSelected { get; set; }
        public bool TryConnect(ConnectInfo connectInfo)
        {
            return true;
        }

        public double X { get; set; }
        public double Y { get; set; }
        
        public IList<IPlugDataContext> InputPlugs { get; } = new List<IPlugDataContext>();
        public IList<IPlugDataContext> OutputPlugs { get; } = new List<IPlugDataContext>();

        public IEnumerable<IPlugDataContext> GetInputPlugs() => InputPlugs;


        public IEnumerable<IPlugDataContext> GetOutputPlugs() => OutputPlugs;
    }
    
    public class TestNodeControl : INodeControl , ISelectable
    {
        public double X 
        { 
            get => _dataContext.X; 
            set => _dataContext.X = value; 
        }
        public double Y 
        { 
            get => _dataContext.Y; 
            set => _dataContext.Y = value; 
        }

        public bool IsMouseOver { get; set; }
        public object DataContext => _dataContext;
        public double ActualWidth { get; set; }
        public double ActualHeight { get; set; }

        public event Action<object, UpdateNodePointArgs> UpdatePoints;
        
        public IList<IPlugControl> OutPutPlugs { get; } = new List<IPlugControl>();
        public IList<IPlugControl> InPutPlugs { get; } = new List<IPlugControl>();

        public IEnumerable<IPlugControl> GetOutputPlugs() => OutPutPlugs;

        public IEnumerable<IPlugControl> GetInputPlugs() => InPutPlugs;

        public bool IsSelected
        {
            get => _dataContext.IsSelected;
            set => _dataContext.IsSelected = value;
        }

        private readonly INodeDataContext _dataContext;

        public TestNodeControl(INodeDataContext nodeDataContext)
        {
            _dataContext = nodeDataContext;
        }
    }

    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _delegate;

        public DelegateCommand(Action<object> @delegate)
        {
            _delegate = @delegate;
        }
        
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _delegate(parameter);
        }

        public void RaiseCanExecute()
        {
            CanExecuteChanged?.Invoke(this,EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;
    }
    
    public static class Mock
    {
        public static TestNodeControl CreateNodeControl(double x, double y, bool isSelected = false)
        {
            return new TestNodeControl(new TestNodeContext()
            {
                X = x,
                Y = y,
                IsSelected = isSelected,
            });
        }

        public static ICommand Command<T>(Action<T> action)
        {
            return new DelegateCommand((x) => action((T) x));
        }
        
        public static ICommand Command(Action action)
        {
            return new DelegateCommand((x) => action());
        }
    }
}