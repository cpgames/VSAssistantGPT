using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace cpGames.VSA.ViewModel
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        #region Fields
        private readonly TaskModel _taskModel;
        #endregion

        #region Properties
        public Action? RemoveAction { get; set; }
        public string Name
        {
            get => _taskModel.name;
            set
            {
                if (_taskModel.name != value)
                {
                    _taskModel.name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _taskModel.description;
            set
            {
                if (_taskModel.description != value)
                {
                    _taskModel.description = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FailureReason
        {
            get => _taskModel.failureReason;
            set
            {
                if (_taskModel.failureReason != value)
                {
                    _taskModel.failureReason = value;
                    OnPropertyChanged();
                }
            }
        }

        public TaskStatus Status
        {
            get => _taskModel.status;
            set
            {
                if (_taskModel.status != value)
                {
                    _taskModel.status = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AssignedTo
        {
            get => _taskModel.assignedTo;
            set
            {
                if (_taskModel.assignedTo != value)
                {
                    _taskModel.assignedTo = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AssignedBy
        {
            get => _taskModel.assignedBy;
            set
            {
                if (_taskModel.assignedBy != value)
                {
                    _taskModel.assignedBy = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Order
        {
            get => _taskModel.order;
            set
            {
                if (_taskModel.order != value)
                {
                    _taskModel.order = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Recurring
        {
            get => _taskModel.recurring;
            set
            {
                if (_taskModel.recurring != value)
                {
                    _taskModel.recurring = value;
                    OnPropertyChanged();
                }
            }
        }

        public float RecurringInterval
        {
            get => _taskModel.recurringInterval;
            set
            {
                if (_taskModel.recurringInterval != value)
                {
                    _taskModel.recurringInterval = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Constructors
        public TaskViewModel(TaskModel taskModel)
        {
            _taskModel = taskModel;
        }
        #endregion

        #region Events
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region Methods
        public void RemoveTask()
        {
            RemoveAction?.Invoke();
        }
        #endregion
    }
}