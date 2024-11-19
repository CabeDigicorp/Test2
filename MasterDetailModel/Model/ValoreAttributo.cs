using CommonResources;
using Commons;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MasterDetailModel
{

    [ProtoContract]
    [ProtoInclude(1001, typeof(ValoreAttributoElenco))]
    [ProtoInclude(1002, typeof(ValoreAttributoColore))]
    [ProtoInclude(1003, typeof(ValoreAttributoFormatoNumero))]
    [ProtoInclude(1004, typeof(ValoreAttributoRiferimentoGuidCollection))]
    [ProtoInclude(1005, typeof(ValoreAttributoVariabili))]
    [ProtoInclude(1006, typeof(ValoreAttributoGuidCollection))]
    [ProtoInclude(1007, typeof(ValoreAttributoReale))]
    [ProtoInclude(1008, typeof(ValoreAttributoContabilita))]
    [ProtoInclude(1009, typeof(ValoreAttributoGuid))]
    [ProtoInclude(1010, typeof(ValoreAttributoTesto))]
    public interface ValoreAttributo
    {
        void UpdateOnNewValore(Valore val);
        void UpdatePlainText(Valore val);
        void UpdateId(Valore val);
        ValoreAttributo Clone();
        bool Equals(ValoreAttributo valAtt);
    }


    [ProtoContract]
    public class ValoreAttributoElenco : ValoreAttributo
    {
        [ProtoMember(1)]
        public List<ValoreAttributoElencoItem> Items { get; set; } = new List<ValoreAttributoElencoItem>();

        [ProtoMember(2)]
        public ValoreAttributoElencoType Type { get; set; } = ValoreAttributoElencoType.Default;

        [ProtoMember(3)]
        public bool IsMultiSelection { get; set; } = false;

        public ValoreAttributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            ValoreAttributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public void UpdateOnNewValore(Valore val)
        {
            ValoreElenco valElenco = val as ValoreElenco;
            ValoreAttributoElencoItem valElItem = Items.FirstOrDefault(item => item.Text == val.PlainText);
            if (valElItem == null)
            {
                ValoreAttributoElencoItem item1 = new ValoreAttributoElencoItem() { Id = this.NewId(), Text = val.PlainText };
                Items.Add(item1);
                valElenco.ValoreAttributoElencoId = item1.Id;
            }
            else
            {
                valElenco.ValoreAttributoElencoId = valElItem.Id;
            }
        }

        public void UpdatePlainText(Valore val)
        {
            ValoreElenco valElenco = val as ValoreElenco;
            if (valElenco == null)
                return;

            if (valElenco.ValoreAttributoElencoId < 0)
                return;

            ValoreAttributoElencoItem valElItem = Items.FirstOrDefault(item => item.Id == valElenco.ValoreAttributoElencoId);
            if (valElItem != null)
                valElenco.V = valElItem.Text;

        }

        public int NewId()
        {
            //HashSet<int> ids = Items.Select(item => item.Id).ToHashSet();
            //int i = -1;
            //while (i < 1000)
            //{
            //    i++;
            //    if (!ids.Contains(i))
            //        break;
            //}
            //return i;

            HashSet<int> ids = Items.Select(item => item.Id).ToHashSet();
            HashSet<int> idsBase = CreateListIndexFromSpecificBase(2, 100);
            int i = 0;

            foreach (var item in idsBase)
            {
                i = item;
                if (!ids.Contains(i))
                    break;
            }
            return i;
        }

        public int NewIdProgressive()
        {
            HashSet<int> ids = Items.Select(item => item.Id).ToHashSet();
            int i = -1;
            while (i < 1000)
            {
                i++;
                if (!ids.Contains(i))
                    break;
            }
            return i;
        }

        private HashSet<int> CreateListIndexFromSpecificBase(int Base, int Esponente)
        {
            HashSet<int> idsBase2 = new HashSet<int>();
            for (int i = 0; i < Esponente; i++)
            {
                idsBase2.Add((int)Math.Pow(Base, i));
            }
            return idsBase2;
        }

        public void UpdateId(Valore val)
        {
            ValoreElenco valElenco = val as ValoreElenco;

            ValoreAttributoElencoItem valElItem = Items.FirstOrDefault(item => item.Text == valElenco.V);
            if (valElItem != null)
                valElenco.ValoreAttributoElencoId = valElItem.Id;
        }

        public bool Equals(ValoreAttributo valAtt)
        {
            return true;
        }
    }

    public enum ValoreAttributoElencoType
    {
        Default = 0,
        Font = 1,
    }

    [ProtoContract]
    public class ValoreAttributoElencoItem
    {
        /// <summary>
        /// Obsoleto
        /// </summary>
        //[ProtoMember(1)]
        //public Guid Id { get; set; }

        [ProtoMember(2)]
        public string Text { get; set; }

        [ProtoMember(3)]
        public int Id { get; set; }
    }

    [ProtoContract]
    public class ValoreAttributoColore : ValoreAttributo
    {
        [ProtoMember(1)]
        public List<ValoreAttributoColoreItem> Items { get; set; } = new List<ValoreAttributoColoreItem>();

        public ValoreAttributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            ValoreAttributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public void UpdateOnNewValore(Valore val)
        {
            ValoreColore valColore = val as ValoreColore;

            ValoreAttributoColoreItem valColItem = Items.FirstOrDefault(item => item.HexValue == valColore.Hexadecimal);
            if (valColItem == null)
                Items.Add(new ValoreAttributoColoreItem() { Id = Guid.NewGuid(), Text = valColore.PlainText, HexValue = valColore.Hexadecimal });
            else
                valColore.Hexadecimal = valColItem.HexValue;


            //if (Items.FirstOrDefault(item => item.HexValue == valColore.Hexadecimal) == null)
            //    Items.Add(new ValoreAttributoColoreItem() { Id = Guid.NewGuid(), Text = valColore.PlainText, HexValue = valColore.Hexadecimal });
        }

        public void UpdatePlainText(Valore val)
        {
            ValoreColore valColore = val as ValoreColore;

            ValoreAttributoColoreItem valColItem = Items.FirstOrDefault(item => item.HexValue == valColore.Hexadecimal);
            if (valColItem != null)
                valColore.V = valColItem.Text;

        }

        public void UpdateId(Valore val)
        {
            ValoreColore valColore = val as ValoreColore;

            ValoreAttributoColoreItem valColItem = Items.FirstOrDefault(item => item.Text == valColore.V);
            if (valColItem != null)
                valColore.Hexadecimal = valColItem.HexValue;
        }

        //public void Load()
        //{
        //    List<ValoreAttributoColoreItem> lista = new List<ValoreAttributoColoreItem>();

        //    foreach (var item in ColorInfo.ColoriInstallatiInMacchina)
        //    {
        //        ValoreAttributoColoreItem attributo = new ValoreAttributoColoreItem();
        //        attributo.HexValue = item.HexValue;
        //        attributo.Text = item.Name;
        //        attributo.Color = item.Color;
        //        lista.Add(attributo);
        //    }

        //    Items = lista;
        //}

        public void Load()
        {
            List<ValoreAttributoColoreItem> lista = new List<ValoreAttributoColoreItem>();

            var coloriMacchina = ColorInfo.ColoriInstallatiInMacchina.ToDictionary(item => item.Name, item => item);

            foreach (string colName in ColorsHelper.OrderedColorsName)
            {
                ColorInfo colInfo = null;
                if (coloriMacchina.TryGetValue(colName, out colInfo))
                {
                    ValoreAttributoColoreItem item = new ValoreAttributoColoreItem();
                    item.HexValue = colInfo.HexValue;
                    item.Text = colInfo.Name;
                    item.Color = colInfo.Color;
                    lista.Add(item);
                }
            }
            Items = lista;
        }

        public bool Equals(ValoreAttributo valAtt)
        {
            return true;
        }
    }

    [ProtoContract]
    public class ValoreAttributoColoreItem
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }

        [ProtoMember(2)]
        public string Text { get; set; }

        public System.Windows.Media.Color Color { get; set; }
        public System.Windows.Media.SolidColorBrush SampleBrush
        {
            get { return new System.Windows.Media.SolidColorBrush(Color); }
        }
        private string _HexValue;
        [ProtoMember(3)]
        public string HexValue
        {
            get { return Color.ToString(); }
            set
            {
                _HexValue = value;
            }
        }
        public ValoreAttributoColoreItem() { }
        public ValoreAttributoColoreItem(string color_name, Color color)
        {
            Text = color_name;
            Color = color;
        }
    }

    [ProtoContract]
    public class ValoreAttributoFormatoNumero : ValoreAttributo
    {
        [ProtoMember(1)]
        public List<ValoreAttributoFormatoNumeroItem> Items { get; set; } = new List<ValoreAttributoFormatoNumeroItem>();

        public ValoreAttributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            ValoreAttributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public void UpdateOnNewValore(Valore val)
        {
            ValoreFormatoNumero valFormatoNumero = val as ValoreFormatoNumero;

            ValoreAttributoFormatoNumeroItem valFormatoItem = Items.FirstOrDefault(item => item.Format == val.PlainText);

            if (valFormatoItem == null)
            {
                ValoreAttributoFormatoNumeroItem item1 = new ValoreAttributoFormatoNumeroItem() { Id = Guid.NewGuid(), Format = val.PlainText };
                Items.Add(item1);
                valFormatoNumero.ValoreAttributoFormatoNumeroId = item1.Id;
            }
            else
                valFormatoNumero.ValoreAttributoFormatoNumeroId = valFormatoItem.Id;

            //if (Items.FirstOrDefault(item => item.Format == val.PlainText) == null)
            //{
            //    ValoreAttributoFormatoNumeroItem item1 = new ValoreAttributoFormatoNumeroItem() { Id = Guid.NewGuid(), Format = val.PlainText };
            //    Items.Add(item1);
            //    valFormatoNumero.ValoreAttributoFormatoNumeroId = item1.Id;
            //}
        }

        public void UpdatePlainText(Valore val)
        {
            ValoreFormatoNumero valFormatoNumero = val as ValoreFormatoNumero;

            ValoreAttributoFormatoNumeroItem valForNumItem = Items.FirstOrDefault(item => item.Id == valFormatoNumero.ValoreAttributoFormatoNumeroId);
            if (valForNumItem != null)
                valFormatoNumero.V = valForNumItem.Format;

        }

        public void UpdateId(Valore val)
        {
            ValoreFormatoNumero valFormato = val as ValoreFormatoNumero;

            ValoreAttributoFormatoNumeroItem valForNumItem = Items.FirstOrDefault(item => item.Format == valFormato.V);
            if (valForNumItem != null)
                valFormato.ValoreAttributoFormatoNumeroId = valForNumItem.Id;
        }

        public bool Equals(ValoreAttributo valAtt)
        {
            return true;
        }
    }

    [ProtoContract]
    public class ValoreAttributoFormatoNumeroItem
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }

        //[ProtoMember(2)]
        //public string Text { get; set; }

        [ProtoMember(3)]
        public string Format { get; set; }

    }

    [ProtoContract]
    public class ValoreAttributoRiferimentoGuidCollection : ValoreAttributo
    {
        [ProtoMember(1)]
        public ValoreOperationType Operation { get; set; } = ValoreOperationType.Nothing;

        public ValoreAttributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            ValoreAttributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public bool Equals(ValoreAttributo valAtt)
        {
            if (Operation != ValoreOperationType.Nothing)
                return false;

            return true;
        }

        public void UpdateId(Valore val)
        {
        }
        public void UpdateOnNewValore(Valore val)
        {
        }
        public void UpdatePlainText(Valore val)
        {
        }

    }

    [ProtoContract]
    public class ValoreAttributoVariabili : ValoreAttributo
    {
        [ProtoMember(1)]
        public string CodiceAttributo { get; set; } = String.Empty;

        public ValoreAttributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            ValoreAttributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public bool Equals(ValoreAttributo valAtt)
        {
            if (valAtt == null)
                return false;

            if (CodiceAttributo == (valAtt as ValoreAttributoVariabili).CodiceAttributo)
                return true;

            return false;
        }

        public void UpdateId(Valore val)
        {
        }

        public void UpdateOnNewValore(Valore val)
        {
        }

        public void UpdatePlainText(Valore val)
        {
        }
    }

    [ProtoContract]
    public class ValoreAttributoGuidCollection : ValoreAttributo
    {
        [ProtoMember(1)]
        public ItemsSelectionTypeEnum ItemsSelectionType { get; set; } = ItemsSelectionTypeEnum.Nothing;

        public ValoreAttributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            ValoreAttributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public bool Equals(ValoreAttributo valAtt)
        {
            if (valAtt == null)
                return false;

            if (ItemsSelectionType == (valAtt as ValoreAttributoGuidCollection).ItemsSelectionType)
                return true;

            return false;
        }

        public void UpdateId(Valore val)
        {
        }

        public void UpdateOnNewValore(Valore val)
        {
        }

        public void UpdatePlainText(Valore val)
        {
        }
    }

    [ProtoContract]
    public class ValoreAttributoReale : ValoreAttributo
    {
        [ProtoMember(1)]
        public bool UseSignificantDigitsByFormat { get; set; } = false;

        public ValoreAttributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            ValoreAttributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public bool Equals(ValoreAttributo valAtt)
        {
            if (valAtt == null)
                return false;

            if (UseSignificantDigitsByFormat == (valAtt as ValoreAttributoReale).UseSignificantDigitsByFormat)
                return true;

            return false;
        }

        public void UpdateId(Valore val)
        {
        }

        public void UpdateOnNewValore(Valore val)
        {
        }

        public void UpdatePlainText(Valore val)
        {
        }
    }

    [ProtoContract]
    public class ValoreAttributoContabilita : ValoreAttributo
    {
        [ProtoMember(1)]
        public bool UseSignificantDigitsByFormat { get; set; } = false;

        public ValoreAttributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            ValoreAttributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public bool Equals(ValoreAttributo valAtt)
        {
            if (valAtt == null)
                return false;

            if (UseSignificantDigitsByFormat == (valAtt as ValoreAttributoContabilita).UseSignificantDigitsByFormat)
                return true;

            return false;
        }

        public void UpdateId(Valore val)
        {
        }

        public void UpdateOnNewValore(Valore val)
        {
        }

        public void UpdatePlainText(Valore val)
        {
        }
    }


    [ProtoContract]
    public class ValoreAttributoGuid : ValoreAttributo
    {
        [ProtoMember(1)]   
        public ValoreAttributoGuidSummarizeItem SummarizeAttributo3 { get; set; } = new ValoreAttributoGuidSummarizeItem();

        [ProtoMember(2)]
        public ValoreAttributoGuidSummarizeItem SummarizeAttributo4 { get; set; } = new ValoreAttributoGuidSummarizeItem();

        [ProtoMember(3)]
        public string ItemPath { get; set; } = string.Empty;

        public ValoreAttributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            ValoreAttributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public bool Equals(ValoreAttributo valAtt)
        {
            if (valAtt == null)
                return false;

            if (!SummarizeAttributo3.Equals((valAtt as ValoreAttributoGuid).SummarizeAttributo3))
                return false;

            if (!SummarizeAttributo4.Equals((valAtt as ValoreAttributoGuid).SummarizeAttributo4))
                return false;

            if (!ItemPath.Equals((valAtt as ValoreAttributoGuid).ItemPath))
                return false;

            return true;
        }

        public void UpdateId(Valore val)  {}
        public void UpdateOnNewValore(Valore val){}
        public void UpdatePlainText(Valore val) { }
    }

    [ProtoContract]
    public class ValoreAttributoGuidSummarizeItem
    {
        [ProtoMember(1)] 
        public string CodiceAttributo { get; set; } = string.Empty;

        public bool Equals(ValoreAttributoGuidSummarizeItem item)
        {
            if (item == null)
                return false;

            if (CodiceAttributo != item.CodiceAttributo)
                return false;

            return true;
        }
    }

    [ProtoContract]
    public class ValoreAttributoTesto : ValoreAttributo
    {
        /// <summary>
        /// Nel caso di struttura ad albero usa (filtri raggruppamenti e ordinamento) e visualizza sempre il valore concatenato con i valori dei padri
        /// </summary>
        [ProtoMember(1)]
        public bool UseDeepValore { get; set; } = false;

        public ValoreAttributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            ValoreAttributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public bool Equals(ValoreAttributo valAtt)
        {
            if (valAtt == null)
                return false;

            if (UseDeepValore == (valAtt as ValoreAttributoTesto).UseDeepValore)
                return true;

            return false;
        }

        public void UpdateId(Valore val)
        {
        }

        public void UpdateOnNewValore(Valore val)
        {
        }

        public void UpdatePlainText(Valore val)
        {
        }
    }

    public enum ItemsSelectionTypeEnum
    {
        Nothing = 0,
        ByHand = 1,
        All = 2,
        ByFilter = 3,
    }
}
