
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Commons
{

    public class NotificationBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // SetField (Name, value); // where there is a data member
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] String property
           = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(property);
            return true;
        }

        // SetField(()=> somewhere.Name = value; somewhere.Name, value) 
        // Advanced case where you rely on another property
        protected bool SetProperty<T>(T currentValue, T newValue, Action DoSet,
            [CallerMemberName] String property = null)
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) return false;
            DoSet.Invoke();
            RaisePropertyChanged(property);
            return true;
        }

        public void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        /// <summary>
        /// Restituiscce il nome della proprietà (by Ale)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        protected string GetPropertyName<T>(Expression<Func<T>> property)
        {
            Expression l = property.Body;
            MemberExpression me = property.Body as MemberExpression;
            return me.Member.Name;
        }
    }

    public class NotificationBase<T> : NotificationBase where T : class, new()
    {
        protected T This;

        public static implicit operator T(NotificationBase<T> thing) { return thing.This; }

        public NotificationBase(T thing = null)
        {
            This = (thing == null) ? new T() : thing;
        }
    }

    public class CommandHandler : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action _action;

        public CommandHandler(Action action)
        {
            this._action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this._action();
        }
    }

    public class CommandHandlerParam : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> _action;

        public CommandHandlerParam(Action<object> action)
        {
            this._action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this._action(parameter);
        }
        

    }

}