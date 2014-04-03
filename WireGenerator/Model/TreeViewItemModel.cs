using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace WireGenerator.Model
{
    public class TreeViewItemModel : INotifyPropertyChanged
    {
        #region Variables
        private string _value;
        private int _parentid;
        private bool _isSelected;

        public BindingList<TreeViewItemModel> Items { get; set; }
        #endregion

        #region Properties
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Item");
            }
        }

        public int ParentId
        {
            get { return _parentid; }
            set { _parentid = value; }
        }
        #endregion

        #region Constructor
        public TreeViewItemModel()
        {
            Items = new BindingList<TreeViewItemModel>();
        }

        public TreeViewItemModel(string value)
        {
            this._value = value;
            Items = new BindingList<TreeViewItemModel>();
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string value)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(value));
            }
        }
        #endregion
    }
}
