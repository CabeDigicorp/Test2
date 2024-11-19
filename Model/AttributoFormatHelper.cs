using MasterDetailModel;

using System.Linq;

namespace Model
{
    public class AttributoFormatHelper
    {
        private string DefaultFormat { get; } = "#,##0.00";
        private IDataService _dataService = null;
        private EntitiesHelper EntitiesHelper { get; set; }

        public AttributoFormatHelper(IDataService dataService)
        {
            _dataService = dataService;
            EntitiesHelper = new EntitiesHelper(_dataService);
        }

        private string GetValorePaddedFormat(EntityAttributo entAtt)
        {
            EntityAttributo sourceEntAtt = EntitiesHelper.GetSourceEntityAttributo(entAtt.Entity, entAtt.AttributoCodice);

            string format = DefaultFormat;
            if (sourceEntAtt != null)
            {
                format = GetValoreFormatInternal(sourceEntAtt);

                if (format == null)
                    format = GetValoreFormatInternal(sourceEntAtt.Attributo);
            }
            else if (entAtt != null)
            {
                return GetValorePaddedFormat(entAtt.Attributo);
            }


            return GetPaddedFormat(format);
        }

        public string GetValorePaddedFormat(Entity ent, string codiceAttributo)
        {
            string format = DefaultFormat;

            if (ent == null)
                return GetPaddedFormat(format);

            if (!ent.Attributi.ContainsKey(codiceAttributo))
                return GetPaddedFormat(format);

            EntityAttributo sourceEntAtt = EntitiesHelper.GetSourceEntityAttributo(ent, codiceAttributo);


            if (sourceEntAtt != null)
            {
                format = GetValoreFormatInternal(sourceEntAtt);

                if (format == null)
                    format = GetValoreFormatInternal(sourceEntAtt.Attributo);
            }
            else
            {
                format = GetValoreFormat(ent.EntityTypeCodice, codiceAttributo);
            }


            return GetPaddedFormat(format);
        }

        public string GetValorePaddedFormat(Attributo att)
        {
            Attributo sourceAtt = EntitiesHelper.GetSourceAttributo(att);
            if (sourceAtt == null)
                return null;

            string format = GetValoreFormatInternal(sourceAtt);
            return GetPaddedFormat(format);
        }

        public string GetValoreFormat(string entityTypeKey, string codiceAttributo)
        {
            Attributo att = EntitiesHelper.GetSourceAttributo(entityTypeKey, codiceAttributo);
            return GetValoreFormatInternal(att);
        }

        public string GetValoreFormat(Entity ent, string codiceAttributo)
        {
            if (ent == null)
                return DefaultFormat;

            var sourceAtt = EntitiesHelper.GetSourceAttributo(ent.EntityTypeCodice, codiceAttributo);

            if (sourceAtt != null)
                return sourceAtt.ValoreFormat;

            return DefaultFormat;
        }

        public string GetValoreFormat(EntityAttributo entAtt)
        {
            EntityAttributo sourceEntAtt = EntitiesHelper.GetSourceEntityAttributo(entAtt.Entity, entAtt.AttributoCodice);

            string format = GetValoreFormatInternal(sourceEntAtt);

            if (format == null)
            {
                if (sourceEntAtt == null)
                    format = GetValoreFormatInternal(entAtt.Attributo);
                else
                    format = GetValoreFormatInternal(sourceEntAtt.Attributo);
            }

            return format;
        }


        //public string GetUnitaMisuraPaddedFormat(string um)
        //{
        //    string format = GetUnitaMisuraFormat(um);
        //    return GetPaddedFormat(format);
        //}

        private string GetPaddedFormat(string format)
        {
            return string.Format("{0}0:{1}{2}", "{", format, "}");
        }

        private string GetValoreFormatInternal(Attributo att)
        {
            if (att == null)
                return DefaultFormat;

            if (att.ValoreFormat != null && att.ValoreFormat.Any())
                return att.ValoreFormat;

            return DefaultFormat;
        }

        private string GetValoreFormatInternal(EntityAttributo entAtt)
        {
            if (entAtt == null)
                return null;

            ValoreReale valReale = entAtt.Valore as ValoreReale;
            if (valReale != null)
            {
                //prendo dal valore
                if (valReale.Format != null && valReale.Format.Any())
                    return valReale.Format;

                //prendo dall'attributo riferito (unità di misura)
                if (entAtt.Attributo != null) 
                {
                    if (entAtt.Attributo.Codice == BuiltInCodes.Attributo.Quantita || entAtt.Attributo.Codice == BuiltInCodes.Attributo.QuantitaTotale)
                    {
                        EntityAttributo sourceEntAtt = EntitiesHelper.GetSourceEntityAttributo(entAtt.Entity, BuiltInCodes.Attributo.PrezzarioItem_FormatoQuantita);

                        if (sourceEntAtt == null)
                            return DefaultFormat;


                        ValoreFormatoNumero valFormato = sourceEntAtt.Entity.GetValoreAttributo(BuiltInCodes.Attributo.FormatoQuantita, false, false) as  ValoreFormatoNumero;
                        if (valFormato != null)
                        {
                            string formato = valFormato.ToPlainText();
                            return formato;
                            //if (um != null && um.Any())
                            //    return GetUnitaMisuraFormat(um);
                        }
                    }
                }
            }

            ValoreContabilita valCont = entAtt.Valore as ValoreContabilita;
            if (valCont != null)
            {
                //prendo dal valore
                if (valCont.Format != null && valCont.Format.Any())
                    return valCont.Format;
            }


            return null;
        }



        //private string GetUnitaMisuraFormat(string um)
        //{

        //    string um1 = um.Trim();
        //    if (um1.StartsWith("{") && um1.EndsWith("}"))
        //    {
        //        //il valore dell'unità di misura è un formato
        //        string format = um1.Substring(1, um1.Length - 2);
        //        string res = TestFormat(format);
        //        if (res != null && res.Any())
        //        {
        //            //test riuscito!
        //            return format;
        //        }
        //    }
        //    else
        //    {
        //        //il valore dell'unità di misura NON è un formato
        //        //cerco nella tabella delle unità di misura di progetto
        //        bool found = true;
        //        if (found)
        //        {
        //            //...
        //            //codice provvisorio
        //            string format = string.Format("{0} {1}", DefaultFormat, um);
        //            return format;
        //        }
        //    }

        //    return null;
        //}

        private string TestFormat(string format)
        {
            string paddedFormat = GetPaddedFormat(format);
            //test formato
            string res = null;
            try
            {
                res = string.Format(paddedFormat, "1234,567");
            }
            catch
            {
            }

            return res;
        }


    }
}
