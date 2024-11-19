

using Commons;
using MasterDetailModel;
using MasterDetailView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Globalization;
using Model;
using System.Windows.Media;
using CommonResources;

namespace MasterDetailView
{
    /// <summary>
    /// Rappresenta la vista di un entità nella lista master
    /// </summary>
    public class EntityView : NotificationBase<Entity>
    {
        protected EntitiesListMasterDetailView _master = null;
        public EntitiesListMasterDetailView Master { get => _master; }

        Dictionary<string, DetailAttributoView> _detailAttributiView = new Dictionary<string, DetailAttributoView>();

       

        public EntityView(EntitiesListMasterDetailView master, Entity ent = null) : base(ent)
        {
            _master = master;

            SetDetailAttributiView();

        }

        public Entity Entity { get => this.This; }
        //{
        //    return this.This;
        //}

        public Guid Id { get { return This.EntityId; } }


        public virtual string Attributo1
        {
            get
            {
                if (This.EntityType.AttributiMasterCodes.Any())
                {
                    return GetAttributoValoreResult(0);
                    //return This.Attributi[This.EntityType.AttributiMasterCodes[0]].Valore.ToPlainText();
                }

                return string.Empty;
            }
        }

        public virtual string Attributo2
        {
            get
            {
                if (This.EntityType.AttributiMasterCodes.Count >= 2)
                {
                    string attCodice = Master.EntityType.AttributiMasterCodes[1];
                    if (This.Attributi.ContainsKey(This.EntityType.AttributiMasterCodes[1]))
                    {
                        return GetAttributoValoreResult(1);
                        //return This.Attributi[This.EntityType.AttributiMasterCodes[1]].Valore.ToPlainText();
                    }


                }
                return string.Empty;
            }
        }

        public virtual string Attributo3
        {
            get
            {
                if (This.EntityType.AttributiMasterCodes.Count >= 3)
                {
                    if (This.Attributi.ContainsKey(This.EntityType.AttributiMasterCodes[2]))
                    {
                        return GetAttributoValoreResult(2);
                        //return This.Attributi[This.EntityType.AttributiMasterCodes[2]].Valore.ToPlainText();
                    }
                }

                return "";
            }
        }

        public virtual string Attributo4
        {
            get
            {
                if (This.EntityType.AttributiMasterCodes.Count >= 4)
                {
                    if (This.Attributi.ContainsKey(This.EntityType.AttributiMasterCodes[3]))
                    {
                        return GetAttributoValoreResult(3);
                    }
                }
                return "";
            }
        }

        //protected string GetAttributoValoreResult(int attMasterIndex)
        //{
        //    AttributoFormatHelper attFormatHelper = new AttributoFormatHelper(Master.DataService);
        //    EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);

        //    EntityAttributo entAtt = This.Attributi[This.EntityType.AttributiMasterCodes[attMasterIndex]];
        //    string format = attFormatHelper.GetValorePaddedFormat(entAtt);


        //    if (entAtt.Valore is ValoreContabilita)
        //    {
        //        return (entAtt.Valore as ValoreContabilita).FormatRealResult(format);
        //    }
        //    else if (entAtt.Valore is ValoreReale)
        //    {
        //        return (entAtt.Valore as ValoreReale).FormatRealResult(format);
        //    }
        //    else if (entAtt.Valore is ValoreTesto)
        //    {
        //        return (entAtt.Valore as ValoreTesto).Result;
        //    }
        //    else
        //    {
        //        return entAtt.Valore.ToPlainText();
        //    }
        //}

        protected string GetAttributoValoreResult(int attMasterIndex)
        {
            AttributoFormatHelper attFormatHelper = new AttributoFormatHelper(Master.DataService);
            EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);

            string codiceAtt = This.EntityType.AttributiMasterCodes[attMasterIndex];
            string format = attFormatHelper.GetValorePaddedFormat(This, codiceAtt);

            Valore val = entsHelper.GetValoreAttributo(This, codiceAtt, false, false);

            if (val is ValoreContabilita)
            {
                return (val as ValoreContabilita).FormatRealResult(format);
            }
            else if (val is ValoreReale)
            {
                return (val as ValoreReale).FormatRealResult(format);
            }
            else if (val is ValoreTesto)
            {
                return (val as ValoreTesto).Result;
            }
            else if (val != null)
            {
                return val.ToPlainText();
            }
            else
                return string.Empty;
        }

        static public string GetMasterMappingNameByIndex(int i) { return AttributoMappingBaseName + i.ToString("D2"); }
        static public string GetMasterValueMappingNameByIndex(int i) { return ValueAttributoMappingBaseName + i.ToString("D2"); }

        //static public int GetAttributiMasterCodesIndexByMappingName(string mappingName)//Data.EntityAttributoView00
        //{
        //    string constant = AttributoMappingBaseName;

        //    if (mappingName.StartsWith(constant))
        //    {
        //        string strIndex = mappingName.Substring(constant.Length);
        //        return Convert.ToInt32(strIndex);
        //    }

        //    return -1;
        //}

        public string GetAttributiMasterCodeByMappingName(string mappingName)//Data.EntityAttributoView00
        {
            return Master.MasterMappingNames.GetCodiceByMappingName(mappingName);
        }

        protected virtual object GetAttributoValore(string propertyName, bool display = true)//display: valore formattato, value: valore in edit mode
        {
            try
            {

                string codiceAtt = null;
                if (display)
                {
                    codiceAtt = Master.MasterMappingNames.GetCodiceByMappingName("Data."+propertyName);
                }
                else
                {
                    codiceAtt = Master.MasterMappingNames.GetCodiceByValueMappingName("Data."+propertyName);
                }

                Attributo sourceAtt = Master.EntitiesHelper.GetSourceAttributo(Master.EntityType.GetKey(), codiceAtt);

                if (sourceAtt.Etichetta == "Ordine")
                { }


                Valore val = null;
                if (sourceAtt.ValoreDefault is ValoreTestoRtf)
                {
                    EntityAttributo sourceEntAtt = Master.EntitiesHelper.GetSourceEntityAttributo(This, codiceAtt);

                    if (sourceEntAtt != null)
                        val = new ValoreTesto() { V = Master.EntitiesHelper.GetValorePlainText(sourceEntAtt.Entity, sourceAtt.Codice, true, true) };
                }
                else
                {
                    bool deep = false;
                    if (sourceAtt.ValoreAttributo is ValoreAttributoTesto valAttTesto)
                        deep = valAttTesto.UseDeepValore;

                    val = Master.EntitiesHelper.GetValoreAttributo(This, codiceAtt, deep, true);
                }



                if (display)
                {

                    if (val is ValoreData)
                        return (val as ValoreData).V;
                    if (val is ValoreTesto)
                    {
                        return (val as ValoreTesto).Result;
                        //return (val as ValoreTesto).V;
                    }
                    if (val is ValoreContabilita)
                    {
                        //string format = Master.AttributoFormatHelper.GetValorePaddedFormat(sourceEntAtt);
                        string format = Master.AttributoFormatHelper.GetValorePaddedFormat(This, codiceAtt);
                        object res = (val as ValoreContabilita).FormatRealResult(format);
                        return res;
                    }
                    if (val is ValoreReale)
                    {
                        //string format = Master.AttributoFormatHelper.GetValorePaddedFormat(sourceEntAtt);
                        string format = Master.AttributoFormatHelper.GetValorePaddedFormat(This, codiceAtt);
                        object res = (val as ValoreReale).FormatRealResult(format);
                        return res;
                    }
                    if (val is ValoreTestoRtf)
                        return (val as ValoreTestoRtf).BriefPlainText;
                    if (val is ValoreElenco)
                        return (val as ValoreElenco).V;
                    if (val is ValoreColore)
                        return (val as ValoreColore).V;
                    if (val is ValoreBooleano)
                        return (val as ValoreBooleano).V;
                    if (val is ValoreFormatoNumero)
                        return (val as ValoreFormatoNumero).V;
                }
                else //value
                {
                    if (val is ValoreData)
                        return (val as ValoreData).V;
                    if (val is ValoreTesto)
                        return (val as ValoreTesto).V;
                    if (val is ValoreContabilita)
                        return (val as ValoreContabilita).V;
                    if (val is ValoreReale)
                        return (val as ValoreReale).V;
                    if (val is ValoreTestoRtf)
                        return (val as ValoreTestoRtf).BriefPlainText;
                    if (val is ValoreElenco)
                        return (val as ValoreElenco).V;
                    if (val is ValoreColore)
                        return (val as ValoreColore).V;
                    if (val is ValoreBooleano)
                        return (val as ValoreBooleano).V;
                    if (val is ValoreFormatoNumero)
                        return (val as ValoreFormatoNumero).V;
                }

            }
            catch (Exception e)
            {

            }
            return null;
        }

        private void SetAttributoValore(string propertyName, object value, bool display = true)
        {

            
            //int index = -1;
            //if (display)
            //    index = Convert.ToInt32(propertyName.Remove(0, AttributoBaseName.Length));
            //else
            //    index = Convert.ToInt32(propertyName.Remove(0, ValueAttributoBaseName.Length));


            //DetailAttributoView detailAttView = DetailAttributiView[Master.EntityType.AttributiMasterCodes[index]];

            //Valore valPrec = detailAttView.EntityAttributo.Valore;


            //Valore val = null;
            //if (valPrec is ValoreData)
            //    val = new ValoreData() { V = value as DateTime? };
            //if (valPrec is ValoreTesto)
            //    val = new ValoreTesto() { V = value as string };
            //if (valPrec is ValoreContabilita)
            //    val = new ValoreContabilita() { V = value.ToString() };
            //if (valPrec is ValoreReale)
            //    val = new ValoreReale() { V = value.ToString() };



            //if (value != null && !val.ResultEquals(valPrec))
            //{
            //    //Master.UpdateDetail = true;
            //    Master.AttributiEntities.SetValoreAttributo(Master.EntityType.AttributiMasterCodes[index], val);
            //    Master.UpdateCache(true);
            //}

        }

        //oss 00,01,02,... è esattamente l'indice dell'attributo nella lista AttributiMasterCodes
        //Display
        static public string AttributoMappingBaseName { get => "Data." + AttributoBaseName; }
        static public string AttributoBaseName { get => "EntityAttributoView"; }
        public object EntityAttributoView00 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView00)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView00), value); }
        public object EntityAttributoView01 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView01)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView01), value); }
        public object EntityAttributoView02 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView02)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView02), value); }
        public object EntityAttributoView03 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView03)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView03), value); }
        public object EntityAttributoView04 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView04)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView04), value); }
        public object EntityAttributoView05 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView05)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView05), value); }
        public object EntityAttributoView06 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView06)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView06), value); }
        public object EntityAttributoView07 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView07)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView07), value); }
        public object EntityAttributoView08 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView08)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView08), value); }
        public object EntityAttributoView09 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView09)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView09), value); }
        public object EntityAttributoView10 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView10)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView10), value); }
        public object EntityAttributoView11 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView11)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView11), value); }
        public object EntityAttributoView12 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView12)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView12), value); }
        public object EntityAttributoView13 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView13)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView13), value); }
        public object EntityAttributoView14 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView14)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView14), value); }
        public object EntityAttributoView15 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView15)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView15), value); }
        public object EntityAttributoView16 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView16)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView16), value); }
        public object EntityAttributoView17 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView17)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView17), value); }
        public object EntityAttributoView18 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView18)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView18), value); }
        public object EntityAttributoView19 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView19)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView19), value); }
        public object EntityAttributoView20 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView20)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView20), value); }
        public object EntityAttributoView21 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView21)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView21), value); }
        public object EntityAttributoView22 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView22)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView22), value); }
        public object EntityAttributoView23 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView23)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView23), value); }
        public object EntityAttributoView24 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView24)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView24), value); }
        public object EntityAttributoView25 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView25)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView25), value); }
        public object EntityAttributoView26 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView26)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView26), value); }
        public object EntityAttributoView27 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView27)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView27), value); }
        public object EntityAttributoView28 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView28)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView28), value); }
        public object EntityAttributoView29 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView29)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView29), value); }
        public object EntityAttributoView30 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView30)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView30), value); }
        public object EntityAttributoView31 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView31)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView31), value); }
        public object EntityAttributoView32 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView32)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView32), value); }
        public object EntityAttributoView33 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView33)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView33), value); }
        public object EntityAttributoView34 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView34)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView34), value); }
        public object EntityAttributoView35 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView35)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView35), value); }
        public object EntityAttributoView36 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView36)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView36), value); }
        public object EntityAttributoView37 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView37)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView37), value); }
        public object EntityAttributoView38 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView38)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView38), value); }
        public object EntityAttributoView39 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView39)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView39), value); }
        public object EntityAttributoView40 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView40)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView40), value); }
        public object EntityAttributoView41 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView41)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView41), value); }
        public object EntityAttributoView42 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView42)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView42), value); }
        public object EntityAttributoView43 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView43)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView43), value); }
        public object EntityAttributoView44 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView44)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView44), value); }
        public object EntityAttributoView45 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView45)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView45), value); }
        public object EntityAttributoView46 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView46)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView46), value); }
        public object EntityAttributoView47 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView47)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView47), value); }
        public object EntityAttributoView48 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView48)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView48), value); }
        public object EntityAttributoView49 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView49)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView49), value); }
        public object EntityAttributoView50 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView50)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView50), value); }
        public object EntityAttributoView51 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView51)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView51), value); }
        public object EntityAttributoView52 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView52)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView52), value); }
        public object EntityAttributoView53 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView53)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView53), value); }
        public object EntityAttributoView54 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView54)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView54), value); }
        public object EntityAttributoView55 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView55)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView55), value); }
        public object EntityAttributoView56 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView56)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView56), value); }
        public object EntityAttributoView57 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView57)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView57), value); }
        public object EntityAttributoView58 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView58)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView58), value); }
        public object EntityAttributoView59 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView59)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView59), value); }
        public object EntityAttributoView60 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView60)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView60), value); }
        public object EntityAttributoView61 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView61)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView61), value); }
        public object EntityAttributoView62 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView62)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView62), value); }
        public object EntityAttributoView63 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView63)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView63), value); }
        public object EntityAttributoView64 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView64)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView64), value); }
        public object EntityAttributoView65 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView65)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView65), value); }
        public object EntityAttributoView66 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView66)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView66), value); }
        public object EntityAttributoView67 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView67)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView67), value); }
        public object EntityAttributoView68 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView68)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView68), value); }
        public object EntityAttributoView69 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView69)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView69), value); }
        public object EntityAttributoView70 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView70)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView70), value); }
        public object EntityAttributoView71 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView71)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView71), value); }
        public object EntityAttributoView72 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView72)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView72), value); }
        public object EntityAttributoView73 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView73)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView73), value); }
        public object EntityAttributoView74 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView74)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView74), value); }
        public object EntityAttributoView75 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView75)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView75), value); }
        public object EntityAttributoView76 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView76)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView76), value); }
        public object EntityAttributoView77 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView77)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView77), value); }
        public object EntityAttributoView78 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView78)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView78), value); }
        public object EntityAttributoView79 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView79)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView79), value); }
        public object EntityAttributoView80 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView80)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView80), value); }
        public object EntityAttributoView81 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView81)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView81), value); }
        public object EntityAttributoView82 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView82)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView82), value); }
        public object EntityAttributoView83 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView83)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView83), value); }
        public object EntityAttributoView84 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView84)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView84), value); }
        public object EntityAttributoView85 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView85)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView85), value); }
        public object EntityAttributoView86 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView86)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView86), value); }
        public object EntityAttributoView87 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView87)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView87), value); }
        public object EntityAttributoView88 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView88)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView88), value); }
        public object EntityAttributoView89 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView89)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView89), value); }
        public object EntityAttributoView90 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView90)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView90), value); }
        public object EntityAttributoView91 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView91)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView91), value); }
        public object EntityAttributoView92 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView92)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView92), value); }
        public object EntityAttributoView93 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView93)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView93), value); }
        public object EntityAttributoView94 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView94)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView94), value); }
        public object EntityAttributoView95 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView95)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView95), value); }
        public object EntityAttributoView96 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView96)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView96), value); }
        public object EntityAttributoView97 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView97)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView97), value); }
        public object EntityAttributoView98 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView98)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView98), value); }
        public object EntityAttributoView99 { get => GetAttributoValore(GetPropertyName(() => EntityAttributoView99)); set => SetAttributoValore(GetPropertyName(() => EntityAttributoView99), value); }


        //Value (edit mode)
        static public string ValueAttributoMappingBaseName { get => "Data." + ValueAttributoBaseName; }
        static public string ValueAttributoBaseName { get => "ValueEntityAttributoView"; }
        public object ValueEntityAttributoView00 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView00), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView00), value, false); }
        public object ValueEntityAttributoView01 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView01), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView01), value, false); }
        public object ValueEntityAttributoView02 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView02), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView02), value, false); }
        public object ValueEntityAttributoView03 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView03), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView03), value, false); }
        public object ValueEntityAttributoView04 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView04), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView04), value, false); }
        public object ValueEntityAttributoView05 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView05), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView05), value, false); }
        public object ValueEntityAttributoView06 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView06), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView06), value, false); }
        public object ValueEntityAttributoView07 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView07), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView07), value, false); }
        public object ValueEntityAttributoView08 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView08), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView08), value, false); }
        public object ValueEntityAttributoView09 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView09), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView09), value, false); }
        public object ValueEntityAttributoView10 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView10), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView10), value, false); }
        public object ValueEntityAttributoView11 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView11), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView11), value, false); }
        public object ValueEntityAttributoView12 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView12), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView12), value, false); }
        public object ValueEntityAttributoView13 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView13), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView13), value, false); }
        public object ValueEntityAttributoView14 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView14), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView14), value, false); }
        public object ValueEntityAttributoView15 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView15), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView15), value, false); }
        public object ValueEntityAttributoView16 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView16), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView16), value, false); }
        public object ValueEntityAttributoView17 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView17), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView17), value, false); }
        public object ValueEntityAttributoView18 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView18), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView18), value, false); }
        public object ValueEntityAttributoView19 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView19), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView19), value, false); }
        public object ValueEntityAttributoView20 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView20), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView20), value, false); }
        public object ValueEntityAttributoView21 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView21), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView21), value, false); }
        public object ValueEntityAttributoView22 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView22), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView22), value, false); }
        public object ValueEntityAttributoView23 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView23), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView23), value, false); }
        public object ValueEntityAttributoView24 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView24), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView24), value, false); }
        public object ValueEntityAttributoView25 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView25), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView25), value, false); }
        public object ValueEntityAttributoView26 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView26), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView26), value, false); }
        public object ValueEntityAttributoView27 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView27), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView27), value, false); }
        public object ValueEntityAttributoView28 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView28), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView28), value, false); }
        public object ValueEntityAttributoView29 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView29), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView29), value, false); }
        public object ValueEntityAttributoView30 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView30), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView30), value, false); }
        public object ValueEntityAttributoView31 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView31), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView31), value, false); }
        public object ValueEntityAttributoView32 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView32), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView32), value, false); }
        public object ValueEntityAttributoView33 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView33), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView33), value, false); }
        public object ValueEntityAttributoView34 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView34), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView34), value, false); }
        public object ValueEntityAttributoView35 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView35), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView35), value, false); }
        public object ValueEntityAttributoView36 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView36), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView36), value, false); }
        public object ValueEntityAttributoView37 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView37), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView37), value, false); }
        public object ValueEntityAttributoView38 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView38), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView38), value, false); }
        public object ValueEntityAttributoView39 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView39), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView39), value, false); }
        public object ValueEntityAttributoView40 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView40), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView40), value, false); }
        public object ValueEntityAttributoView41 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView41), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView41), value, false); }
        public object ValueEntityAttributoView42 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView42), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView42), value, false); }
        public object ValueEntityAttributoView43 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView43), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView43), value, false); }
        public object ValueEntityAttributoView44 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView44), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView44), value, false); }
        public object ValueEntityAttributoView45 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView45), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView45), value, false); }
        public object ValueEntityAttributoView46 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView46), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView46), value, false); }
        public object ValueEntityAttributoView47 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView47), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView47), value, false); }
        public object ValueEntityAttributoView48 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView48), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView48), value, false); }
        public object ValueEntityAttributoView49 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView49), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView49), value, false); }
        public object ValueEntityAttributoView50 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView50), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView50), value, false); }
        public object ValueEntityAttributoView51 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView51), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView51), value, false); }
        public object ValueEntityAttributoView52 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView52), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView52), value, false); }
        public object ValueEntityAttributoView53 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView53), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView53), value, false); }
        public object ValueEntityAttributoView54 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView54), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView54), value, false); }
        public object ValueEntityAttributoView55 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView55), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView55), value, false); }
        public object ValueEntityAttributoView56 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView56), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView56), value, false); }
        public object ValueEntityAttributoView57 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView57), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView57), value, false); }
        public object ValueEntityAttributoView58 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView58), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView58), value, false); }
        public object ValueEntityAttributoView59 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView59), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView59), value, false); }
        public object ValueEntityAttributoView60 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView60), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView60), value, false); }
        public object ValueEntityAttributoView61 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView61), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView61), value, false); }
        public object ValueEntityAttributoView62 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView62), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView62), value, false); }
        public object ValueEntityAttributoView63 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView63), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView63), value, false); }
        public object ValueEntityAttributoView64 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView64), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView64), value, false); }
        public object ValueEntityAttributoView65 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView65), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView65), value, false); }
        public object ValueEntityAttributoView66 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView66), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView66), value, false); }
        public object ValueEntityAttributoView67 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView67), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView67), value, false); }
        public object ValueEntityAttributoView68 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView68), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView68), value, false); }
        public object ValueEntityAttributoView69 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView69), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView69), value, false); }
        public object ValueEntityAttributoView70 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView70), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView70), value, false); }
        public object ValueEntityAttributoView71 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView71), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView71), value, false); }
        public object ValueEntityAttributoView72 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView72), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView72), value, false); }
        public object ValueEntityAttributoView73 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView73), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView73), value, false); }
        public object ValueEntityAttributoView74 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView74), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView74), value, false); }
        public object ValueEntityAttributoView75 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView75), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView75), value, false); }
        public object ValueEntityAttributoView76 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView76), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView76), value, false); }
        public object ValueEntityAttributoView77 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView77), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView77), value, false); }
        public object ValueEntityAttributoView78 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView78), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView78), value, false); }
        public object ValueEntityAttributoView79 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView79), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView79), value, false); }
        public object ValueEntityAttributoView80 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView80), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView80), value, false); }
        public object ValueEntityAttributoView81 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView81), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView81), value, false); }
        public object ValueEntityAttributoView82 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView82), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView82), value, false); }
        public object ValueEntityAttributoView83 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView83), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView83), value, false); }
        public object ValueEntityAttributoView84 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView84), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView84), value, false); }
        public object ValueEntityAttributoView85 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView85), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView85), value, false); }
        public object ValueEntityAttributoView86 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView86), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView86), value, false); }
        public object ValueEntityAttributoView87 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView87), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView87), value, false); }
        public object ValueEntityAttributoView88 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView88), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView88), value, false); }
        public object ValueEntityAttributoView89 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView89), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView89), value, false); }
        public object ValueEntityAttributoView90 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView90), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView90), value, false); }
        public object ValueEntityAttributoView91 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView91), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView91), value, false); }
        public object ValueEntityAttributoView92 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView92), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView92), value, false); }
        public object ValueEntityAttributoView93 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView93), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView93), value, false); }
        public object ValueEntityAttributoView94 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView94), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView94), value, false); }
        public object ValueEntityAttributoView95 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView95), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView95), value, false); }
        public object ValueEntityAttributoView96 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView96), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView96), value, false); }
        public object ValueEntityAttributoView97 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView97), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView97), value, false); }
        public object ValueEntityAttributoView98 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView98), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView98), value, false); }
        public object ValueEntityAttributoView99 { get => GetAttributoValore(GetPropertyName(() => ValueEntityAttributoView99), false); set => SetAttributoValore(GetPropertyName(() => ValueEntityAttributoView99), value, false); }




        public Dictionary<string, DetailAttributoView> DetailAttributiView
        {
            get
            {
                return _detailAttributiView;

            }
            set
            {
                SetProperty(ref _detailAttributiView, value);
            }

        }


        //protected virtual void SetDetailAttributiView_Old()
        //{
        //    Entity entity = Entity;

            

        //    foreach (EntityAttributo entAtt in This.Attributi.Values)
        //    {

        //        if (entAtt.Attributo == null)
        //            continue;

        //        Valore valore = null;

        //        if (!entAtt.Attributo.IsVisible)
        //            continue;

        //        if (entAtt.Attributo is AttributoRiferimento)
        //        {
        //            AttributoRiferimento attRif = entAtt.Attributo as AttributoRiferimento;

        //            if (Master.EntitiesHelper.IsAttributoRiferimentoGuidCollection(attRif))
        //            {
        //                Valore val = Master.EntitiesHelper.GetValoreAttributo(entAtt.Entity, attRif.Codice, false, false);
        //                if (val != null)
        //                    valore = val.Clone();
        //                else
        //                    valore = new ValoreTesto();
        //            }
        //            else
        //            {
        //                ValoreGuid valGuid = entity.Attributi[attRif.ReferenceCodiceGuid].Valore as ValoreGuid;
        //                Entity ent = Master.EntitiesHelper.GetDataServiceEntityById(attRif.ReferenceEntityTypeKey, valGuid.V);

        //                if (ent != null)
        //                {
        //                    Valore val = Master.EntitiesHelper.GetValoreAttributo(ent, attRif.ReferenceCodiceAttributo, false, false);

        //                    if (val != null)
        //                    {
        //                        valore = val.Clone();
        //                    }
        //                    else
        //                        valore = new ValoreTesto();

        //                }
        //                else
        //                {
        //                    valore = new ValoreTesto();
        //                }
        //            }


        //            DetailAttributoView entAttView = new DetailAttributoView(Master, valore, /*clone*/entAtt) { TabIndex = entAtt.Attributo.DetailViewOrder };
        //            _detailAttributiView.Add(entAttView.Attributo.Codice, entAttView);
        //        }
        //        else
        //        {
        //            DetailAttributoView entAttView = new DetailAttributoView(Master, entAtt.Valore,/*clone*/entAtt) { TabIndex = entAtt.Attributo.DetailViewOrder };
        //            _detailAttributiView.Add(entAtt.Attributo.Codice, entAttView);
        //        }



        //    }

        //}

        protected virtual void SetDetailAttributiView()
        {
            Entity entity = Entity;

            EntityType entType = Master.DataService.GetEntityType(entity.EntityTypeCodice);

            foreach (Attributo Attributo in entType.Attributi.Values)
            {

                if (Attributo == null)
                    continue;

                Valore valore = null;

                if (!Attributo.IsVisible)
                    continue;

                if (Attributo is AttributoRiferimento)
                {
                    AttributoRiferimento attRif = Attributo as AttributoRiferimento;

                    if (Master.EntitiesHelper.IsAttributoRiferimentoGuidCollection(attRif))
                    {
                        Valore val = Master.EntitiesHelper.GetValoreAttributo(Entity, attRif.Codice, false, false);
                        if (val != null)
                            valore = val.Clone();
                        else
                            valore = new ValoreTesto();
                    }
                    else
                    {
                        //ValoreGuid valGuid = entity.Attributi[attRif.ReferenceCodiceGuid].Valore as ValoreGuid;
                        ValoreGuid valGuid = Master.EntitiesHelper.GetValoreAttributo(entity, attRif.ReferenceCodiceGuid, false, false) as ValoreGuid;
                        if (valGuid == null)
                            continue;

                        
                        Entity ent = Master.EntitiesHelper.GetDataServiceEntityById(attRif.ReferenceEntityTypeKey, valGuid.V);

                        if (ent != null)
                        {
                            Valore val = Master.EntitiesHelper.GetValoreAttributo(ent, attRif.ReferenceCodiceAttributo, false, false);

                            if (val != null)
                            {
                                valore = val.Clone();
                            }
                            else
                                valore = new ValoreTesto();

                        }
                        else
                        {
                            valore = new ValoreTesto();
                        }
                    }


                    DetailAttributoView entAttView = new DetailAttributoView(Master, valore, Attributo) { TabIndex = Attributo.DetailViewOrder };
                    _detailAttributiView.Add(entAttView.Attributo.Codice, entAttView);
                }
                else
                {

                    valore = Master.EntitiesHelper.GetValoreAttributo(entity, Attributo.Codice, false, false);

                    DetailAttributoView entAttView = new DetailAttributoView(Master, valore, Attributo) { TabIndex = Attributo.DetailViewOrder };
                    _detailAttributiView.Add(Attributo.Codice, entAttView);
                }



            }

        }
        protected bool _isChecked = false;
        public virtual bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (SetProperty(ref _isChecked, value))
                {
                    //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<TreeEntityView>();
                    //log.Trace("IsChecked");

                    if (_isChecked)
                        Master.CheckedEntitiesId.Add(this.Id);
                    else
                        Master.CheckedEntitiesId.Remove(this.Id);


                    Master.OnCheckedEntity();

                    //RaisePropertyChanged(GetPropertyName(() => BackgroundColor));

                }
            }
        }

        public void SetChecked(bool ischecked)
        {
            _isChecked = ischecked;
        }

        //public ICommand CheckedCommand
        //{
        //    get { return new CommandHandler(() => this.Checked()); }
        //}

        //void Checked()
        //{

        //}



        public ICommand ShiftCheckedCommand
        {
            get { return new CommandHandler(() => this.ShiftChecked()); }
        }

        void ShiftChecked()
        {
            Master.IsMultipleModify = false;
            Master.CheckedEntitiesId.Clear();
            Master.OnShiftChecked(this.Id);
            Master.SelectEntityById(this.Id, false);
            
        }

        public ICommand ShiftCtrlCheckedCommand
        {
            get { return new CommandHandler(() => this.ShiftCtrlChecked()); }
        }

        void ShiftCtrlChecked()
        {
            Master.OnShiftChecked(this.Id);
            Master.SelectEntityById(this.Id, false);

        }

        public ICommand CtrlCheckedCommand
        {
            get { return new CommandHandler(() => this.CtrlChecked()); }
        }

        void CtrlChecked()
        {
            Master.IsMultipleModify = false;
            bool check = Master.CheckedEntitiesId.Contains(this.Id);
            Master.CheckEntityById(this.Id, !check);
            Master.OnCheckedEntity();
            Master.SelectEntityById(this.Id, false);
            
        }

        public bool IsSearched
        {
            get
            {
                if (Master.RightPanesView.FilterView.IsSearchApplied())
                {
                    if (Master.RightPanesView.FilterView.FoundEntitiesId.Contains(Id))
                        return true;
                }

                return false;
            }
        }


        //public bool IsSelected
        //{
        //    get
        //    {
        //        return Id == Master.SelectedEntityId;
        //    }
        //}

        //public string BorderBrush
        //{
        //    get
        //    {
        //        if (IsSelected)
        //            return "SelectedEntityBorderColor";
        //        else
        //            return "TransparentColor";
        //    }
        //}


        //public string BackgroundColor
        //{
        //    get
        //    {
        //        if (IsChecked)
        //            return "CheckedEntityColor";// { ThemeResource SystemControlHighlightListAccentLowBrush}";
        //        else
        //            return "TransparentColor";
        //    }
        //}

        //public ColorConverter.ColorsEnum Background
        //{
        //    get
        //    {
        //        if (Master.CheckedEntitiesId.Contains(Id))
        //            return ColorConverter.ColorsEnum.EntitySelectionColor;
        //        else
        //            return ColorConverter.ColorsEnum.Transparent;


        //    }
        //}

        public SolidColorBrush Background
        {
            get
            {
                if (Master.CheckedEntitiesId.Contains(Id))
                    return ColorsHelper.Convert(MyColorsEnum.EntitySelectionColor);
                else
                    return ColorsHelper.Convert(MyColorsEnum.Transparent);
            }
        }

        public SolidColorBrush HighlighterColor
        {
            get => ColorsHelper.Convert(Entity.HighlighterColorName);
        }

        public bool IsCopied
        {
            get => Master.ReadyToModifyEntitiesId.Contains(Id) && Master.ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Copy;
        }

        public bool IsReadyToMove
        {
            get => Master.ReadyToModifyEntitiesId.Contains(Id) && Master.ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Move;
        }

        public bool IsReadyToMultiModify
        {
            get => Master.ReadyToModifyEntitiesId.Contains(Id) && Master.ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.MultiModify;
        }


        public ICommand RightTappedCommand
        {
            get
            {
                return new CommandHandler(() => this.RightTapped());
            }
        }

        protected virtual void RightTapped()
        {
            Master.SelectIndex(Master.FilteredIndexOf(Id));
            //Master.SelectedIndex = Master.FilteredIndexOf(Id);
        }

#region EntityView Copy&Paste

        public ICommand CopyToClipboardCommand
        {
            get
            {
                return new CommandHandler(() => this.CopyToClipboard());
            }
        }

        public void CopyToClipboard()
        {
            Master.CopyEntities();
        }

        public bool IsCopyToClipboardEnabled
        {
            get
            {
                return true;
                //return IsChecked;
            }
        }

        public ICommand CopyTextToClipboardCommand { get { return new CommandHandler(() => this.CopyTextToClipboard()); } }
        public void CopyTextToClipboard()
        {
            Master.CopyTextEntities();
        }


        public ICommand PasteClipboardCommand
        {
            get
            {
                return new CommandHandler(() => this.PasteClipboard());
            }
        }

        public void PasteClipboard()
        {
            Master.PasteClipboardEntities();
        }

        public bool IsPasteClipboardEnabled
        {
            get { return Master.IsAnyReadyToPaste; }
        }

        //public bool IsPasteClipboardEnabled
        //{
        //    get { return /*!MasterDetailViewManager.IsClipboardEmpty &&*/ Master.SelectedEntityId == Id; }
        //}

        //public ICommand CopyWithHeadersToClipboardCommand
        //{
        //    get
        //    {
        //        return new CommandHandler(() => this.CopyToClipboard(true));
        //    }

        //}

#endregion EntityView Copy&Paste


        /// <summary>
        /// Aggiorna l'entità nella lista master
        /// </summary>
        public virtual void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Attributo1));
            RaisePropertyChanged(GetPropertyName(() => Attributo2));
            RaisePropertyChanged(GetPropertyName(() => Attributo3));
            RaisePropertyChanged(GetPropertyName(() => Attributo4));
            RaisePropertyChanged(GetPropertyName(() => IsChecked));
            RaisePropertyChanged(GetPropertyName(() => IsSearched));
            //RaisePropertyChanged(GetPropertyName(() => IsSelected));
            //RaisePropertyChanged(GetPropertyName(() => Foreground));
            RaisePropertyChanged(GetPropertyName(() => Background));
            RaisePropertyChanged(GetPropertyName(() => IsReadyToMove));
            RaisePropertyChanged(GetPropertyName(() => IsCopied));
            RaisePropertyChanged(GetPropertyName(() => IsReadyToMultiModify));
            RaisePropertyChanged(GetPropertyName(() => HighlighterColor));


            RaisePropertyChanged(GetPropertyName(() => DetailAttributiView));
            foreach (DetailAttributoView entAttView in DetailAttributiView.Values)
                entAttView.UpdateUI();
        }

        //public void Calculate(Valore val)
        //{
        //    foreach (DetailAttributoView attView in DetailAttributiView.Values)
        //    {
        //        attView.Calculate(val);
        //    }
        //}

        public Guid GetAttributoGuidId(string codiceAttributoGuid)
        {
            return Entity.GetAttributoGuidId(codiceAttributoGuid);
        }

        public virtual bool IsValoreAttributoReadOnly(string attCode)
        {
            if (Master.EntityType.Attributi.ContainsKey(attCode))
            {
                Attributo att = Master.EntityType.Attributi[attCode];
                if (att.IsValoreReadOnly || att.IsValoreLockedByDefault ||
                    att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento ||
                    att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile)
                    return true;
            }
            return false;
        }

    }

    //public class EntityViewSelectionConverter : IValueConverter
    //{

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        EntityView data = (EntityView)value;
    //        if (data != null)
    //        {
    //            bool isChecked = (data as EntityView).IsChecked;
    //            if (isChecked)
    //                return true;
    //        }

    //        return false;
    //    }


    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}
}