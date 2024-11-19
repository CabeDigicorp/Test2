using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Commons.View
{
    public class StampeListView : NotificationBase
    {
        public ObservableCollection<WorkItem> _ItemColumn;
        public ObservableCollection<WorkItem> ItemColumn 
        {
            get
            {
                return _ItemColumn;
            }
            set
            {
                if (SetProperty(ref _ItemColumn, value))
                {
                    _ItemColumn = value;
                }
            }
        }
        public Int32 IdList { get; set; }
        private string _HeaderContent;
        public string HeaderContent
        {
            get
            {
                return _HeaderContent;
            }
            set
            {
                if (SetProperty(ref _HeaderContent, value))
                {
                    _HeaderContent = value;
                }
            }
        }
        private string _SizeColumn;
        public string SizeColumn
        {
            get
            {
                return _SizeColumn;
            }
            set
            {
                value = value + " cm";
                if (SetProperty(ref _SizeColumn, value))
                {
                    _SizeColumn = value;
                }
            }
        }

        public event EventHandler CollectionChanged;
        public event EventHandler<DraAndDropEventArgs> DragEvent;
        public event EventHandler<DraAndDropEventArgs> DropEvent;
        public StampeListView(Int32 Id)
        {
            IdList = Id;
            SizeColumn = "..." + SizeColumn;
        }

        public void UpdateSizeColumn()
        {
            EventHandler CollectionChangedEvent = CollectionChanged;
            CollectionChangedEvent?.Invoke(this, new EventArgs());
        }

        public void Init()
        {
           
        }

        public void RaiseDragEvent(WorkItem workItem, string operation)
        {
            EventHandler<DraAndDropEventArgs> HandlerDragEvent = DragEvent;
            DraAndDropEventArgs eventArgs = new DraAndDropEventArgs(workItem, IdList, operation);
            HandlerDragEvent?.Invoke(this, eventArgs);
        }
        public void RaiseDropEvent()
        {
            EventHandler<DraAndDropEventArgs> HandlerDropEvent = DropEvent;
            DraAndDropEventArgs eventArgs = new DraAndDropEventArgs(new WorkItem(), IdList, "Drop");
            HandlerDropEvent?.Invoke(this, eventArgs);
        }
    }

    public class WorkItem : NotificationBase
    {
        private string _Title;
        public string Title 
        {
            get
            {
                return _Title;
            }
            set
            {
                if (SetProperty(ref _Title, value))
                {
                    _Title = value;
                }
            }
        }

    
        private string _PropertyType;
        public string PropertyType
        {
            get
            {
                return _PropertyType;
            }
            set
            {
                if (SetProperty(ref _PropertyType, value))
                {
                    _PropertyType = value;
                }
            }
        }

        public WorkItem(string title,decimal indent)
        {


        }
        public WorkItem(string title, string propertyType)
        {
            this.Title = title;
            this.PropertyType = propertyType;

        }
        public WorkItem()
        {

        }
    }
    public class DraAndDropEventArgs : EventArgs
    {
        public WorkItem WorkItem { get; set; }
        public Int32 IdList { get; set; }
        public string Operation { get; set; }
        public DraAndDropEventArgs(WorkItem workItem, Int32 idList , string operation)
        {
            WorkItem = workItem;
            IdList = idList;
            Operation = operation;
        }

        public DraAndDropEventArgs()
        {

        }
    }
}
