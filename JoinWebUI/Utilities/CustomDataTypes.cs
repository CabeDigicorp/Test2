using Syncfusion.Blazor.Charts;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ModelData.Model;

namespace JoinWebUI.Utilities
{
    public struct NullableBoolChoices
    {
        public int Value;
        public bool? BoolValue;
        public string Description;
    }


    public class PropertyItem
    {
        public string Name { get; set; }
        //private string? _value;
        public string? Value { get; set; }
        //{
        //    get
        //    {
        //        if (_value != null && IfcCategories.IfcCategoryFormattedNames.ContainsKey(_value))
        //            return IfcCategories.IfcCategoryFormattedNames[_value];

        //        return _value;
        //    }                    
        //    set => _value = value;
        //}
        public long? LinkKey { get; private set; }

        public PropertyItem()
            : this(string.Empty, string.Empty, null)
        {
        }

        public PropertyItem(string name, string? value)
            : this(name, value, null)
        {
        }

        public PropertyItem(string name, string? value, long? linkKey)
        {
            //NomeGruppo = groupName;
            Name = name;
            Value = value;
            LinkKey = linkKey;
            //LinkName = linkName;
        }
    }

    //public class MaterialLayerSetItem
    //{
    //    public string Name { get; private set; }
    //    public string Direction { get; private set; }
    //    public string Sense { get; private set; }
    //    public string Offset { get; private set; }
    //    public MaterialLayerItem[] ChildrenLayers { get; set; } = new MaterialLayerItem[0];
    //}

    public class MaterialLayerItem
    {
        public string Name { get; set; } = string.Empty;
        public PropertyItem[] ChildrenProperties { get; set; } = new PropertyItem[0];
    }

    public class QuantityItem : PropertyItem
    {
        //public string Name { get; private set; }
        public string IfcType { get; private set; }
        public string? Description { get; set; }
        public string? Unit { get; set; }
        
        public string ValueAndUnit { get => $"{Value} {Unit}"; }

        //public long? LinkKey { get; private set; }
        public string? LinkName { get; private set; }

        public QuantityItem(string name, string ifcType)
            : this(name, ifcType, null, null, null, null)
        {
        }

        public QuantityItem(string name, string ifcType, string? description, string? unit, string? value, long? linkKey)
            : base(name, value, linkKey)
        {
            IfcType = ifcType;
            Description = description;
            Unit = unit;
        }
    }


    public class ModelNavTreeNode : INotifyPropertyChanged, IComparable<ModelNavTreeNode>
    {
        public ModelNavTreeNodeTypes NodeType { get; private set; }

        //public string? IfcModelKey { get; protected set; }
        //public long ExpressID { get; protected set; }
        public ObjectKey ID { get; private set; }
        public string Name { get; protected set; }
        public string? GlobalId { get; protected set; }
        public ModelNavTreeNode? Parent { get; set; } = null;

        public bool HasChildren { get => Children.Count > 0; } //{ get; set; }
        public List<ModelNavTreeNode> Children { get; set; } = new List<ModelNavTreeNode>();
        public string ObjectType { get; protected set; }
        //public string Info { get => ObjectType + ": " + Name; }
        public bool IsFragment { get; protected set; } = false;
        public bool IsIfcSpace { get; protected set; } = false;
        public bool HasProperties { get; protected set; } = false;
        public bool Checked { get; set; } = false;

        public ModelNavTreeNode(ModelNavTreeNodeTypes nodeType, ObjectKey id, string name, string? globalId, string type, bool isFragment, bool isIfcSpace, bool hasProperties, ModelNavTreeNode? parent)
        {
            NodeType = nodeType;
            ID = id;
            Name = name;
            GlobalId = globalId;
            ObjectType = type;
            IsFragment = isFragment;
            IsIfcSpace = isIfcSpace;
            HasProperties = hasProperties;
            Parent = parent;
        }

        //public ModelNavTreeNode(ModelNavTreeNodeTypes nodeType, long expressID, string name, string type, bool isFragment, bool isIfcSpace, bool hasProperties, ModelNavTreeNode? parent)
        //: this(nodeType, new ObjectKey(expressID), name, type, isFragment, isIfcSpace, hasProperties, parent)
        //{            
        //}


        public bool IsSelectable(bool ifcSpacesShown)
        {
            return (IsFragment || HasProperties) && (!IsIfcSpace || ifcSpacesShown);
        }


        public bool IsCheckable(bool ifcSpacesShown)
        {
            var result = IsSelectable(ifcSpacesShown);
            if (!result && HasChildren)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    result = (Children[i].IsCheckable(ifcSpacesShown));
                    if (result) break;

                }
            }
            return result;
        }

        //public bool HasCheckableChildren(bool ifcSpacesShown)
        //{
        //    if (HasChildren)
        //    {
        //        for (int i = 0; i < Children.Count; i++)
        //        {
        //            if (Children[i].IsSelectable(ifcSpacesShown) || Children[i].HasSelectableChildren(ifcSpacesShown)) return true;
        //        }
        //    }

        //    return false;
        //}

        //private bool _highlighted = false;
        //public bool Highlighted
        //{
        //    get => _highlighted;
        //    set
        //    {
        //        if (_highlighted != value)
        //        {
        //            _highlighted = value;
        //            NotifyPropertyChanged(nameof(Highlighted));
        //        }
        //    }
        //}


        private bool _expanded = false;
        public bool Expanded
        {
            get => _expanded;
            set
            {
                if (_expanded != value)
                {
                    _expanded = value;
                    NotifyPropertyChanged(nameof(Expanded));
                }
            }
        }

        //public void ExpandThisAndAncestors()
        //{
        //    Expanded = true;
        //    if (Parent != null)
        //    {
        //        Parent.ExpandThisAndAncestors();
        //    }

        //}

        public void ExpandSingle()
        {
            Expanded = true;
            if (Children.Count == 1)
            {
                Children[0].ExpandSingle();
            }
        }

        public void ExpandAll()
        {
            Expanded = true;

            foreach (var child in Children)
            {
                child.ExpandAll();
            }
        }

        public void CollapseAll()
        {
            Expanded = false;
            foreach (var child in Children)
            {
                child.CollapseAll();
            }
        }

        public int CompareTo(ModelNavTreeNode? other)
        {
            if (other == null) return 1;

            return string.Compare(this.Name, other.Name);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public enum ModelNavTreeNodeTypes
    {
        SpatialRelation,
        Type,
        Element,
        Group,
        GroupByType
    }




    public class SpatialRelationBaseInfo
    {
        public ObjectKey ID { get; set; } = new ObjectKey(string.Empty, -1);
        public string Name { get; set; } = string.Empty;
        public string? GlobalId { get; set; } = null;
        public string TypeName { get; set; } = string.Empty;
        public bool IsFragment { get; set; } = false;
        public bool IsIfcSpace { get; set; } = false;
        public bool HasProperties { get; set; } = false;
        public SpatialRelationBaseInfo[] Children { get; set; } = new SpatialRelationBaseInfo[0];
    }

    public class TypeBaseInfo
    {
        public ObjectKey ID { get; set; } = new ObjectKey(string.Empty, -1);
        public string Name { get; set; } = string.Empty;
        public string? GlobalId { get; set; } = null;
        public long SuperTypeExpressID { get; set; } = -1;
        public string SuperTypeName { get; set; } = string.Empty;
        public bool IsIfcSpace { get; set; } = false;
        public bool HasProperties { get; set; } = false;
        public ElementBaseInfo[] ChildrenElements { get; set; } = new ElementBaseInfo[0];
        public TypeBaseInfo[] ChildrenTypes { get; set; } = new TypeBaseInfo[0];
    }

    public class GroupBaseInfo
    {
        public ObjectKey ID { get; set; } = new ObjectKey(string.Empty, -1);
        public string Name { get; set; } = string.Empty;
        public string? GlobalId { get; set; } = null;
        public string TypeName { get; set; } = string.Empty;
        public bool IsFragment { get; set; } = false;
        public bool IsIfcSpace { get; set; } = false;
        public bool HasProperties { get; set; } = false;
        public ElementBaseInfo[] ChildrenElements { get; set; } = new ElementBaseInfo[0];
        public ElementBaseInfo[] Buildings { get; set; } = new ElementBaseInfo[0];
    }

    public class ElementBaseInfo
    {
        public ObjectKey ID { get; set; } = new ObjectKey(string.Empty, -1);
        public string Name { get; set; } = string.Empty;
        public string? GlobalId { get; set; } = null;
        public long TypeExpressID { get; set; } = -1;
        public string TypeName { get; set; } = string.Empty;
        public bool IsFragment { get; set; } = false;
        public bool IsIfcSpace { get; set; } = false;
        public bool HasProperties { get; set; } = false;
    }

    public class ObjectKey
    {
        public ObjectKey(string? ifcModelKey, long expressID)
        {
            IfcModelKey = ifcModelKey;
            ExpressID = expressID;
        }

        public ObjectKey(long expressID)
        {
            IfcModelKey = null;
            ExpressID = expressID;
        }

        public ObjectKey()
        {
            IfcModelKey = null;
            ExpressID = -1;
        }

        public string? IfcModelKey { get; set; }
        public long ExpressID { get; set; }

        public bool Equals(ObjectKey? obj)
        {
            if (obj == null)
                return this == null;
            return (this.IfcModelKey == obj.IfcModelKey && this.ExpressID == obj.ExpressID);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return this == null;
            if (!(obj is ObjectKey))
                return false;
            return Equals(obj as ObjectKey);
        }

        public override int GetHashCode()
        {
            long code = IfcModelKey != null ? IfcModelKey.GetHashCode() : 0;
            code += ExpressID.GetHashCode();
            return code.GetHashCode();
        }
    }

    public struct ModelIdPair
    {
        public string IfcModelKey { get; set; }
        public string ModelGlobalID { get; set; }

        public ModelIdPair(string ifcModelKey, string modelGlobalID)
        {
            IfcModelKey = ifcModelKey;
            ModelGlobalID = modelGlobalID;
        }
    }
}
