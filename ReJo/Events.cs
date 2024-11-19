using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using CommonResources;

namespace ReJo
{
    public class SaveAsEventHandler
    {
        public void Initialize(UIApplication uiapp)
        {
            // Ottieni il comando SaveAs
            RevitCommandId saveAsCommandId = RevitCommandId.LookupPostableCommandId(PostableCommand.SaveAsProject);

            // Crea un binding per l'evento BeforeExecuted
            uiapp.CreateAddInCommandBinding(saveAsCommandId).BeforeExecuted += new EventHandler<BeforeExecutedEventArgs>(OnBeforeSaveAsExecuted);
        }

        private void OnBeforeSaveAsExecuted(object sender, BeforeExecutedEventArgs e)
        {
            string docPathName = e.ActiveDocument.PathName;
            if (docPathName.StartsWith(JoinService.This.AppSettingsPath))
            {

                e.Cancel = true;

                if (!CmdInit.IsInitialized)
                    return;

                SaveFileDialog saveFileDialog = new SaveFileDialog();

                // Imposta le proprietà del dialogo
                saveFileDialog.Filter = "Revit files (*.rvt)|*.rvt|All files (*.*)|*.*";
                saveFileDialog.Title = "Save a Revit File";
                saveFileDialog.DefaultExt = "rvt";
                saveFileDialog.AddExtension = true;
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); 

                // Mostra il dialogo e controlla se l'utente ha cliccato su "Salva"
                if (saveFileDialog.ShowDialog() == true)
                {
                    // Ottieni il percorso completo del file selezionato
                    string docPathNameNew = saveFileDialog.FileName;

                    if (docPathNameNew.StartsWith(JoinService.This.AppSettingsPath))
                    {
                        MessageBox.Show(LocalizationProvider.GetString("ImpossibileSalvareNellaCartellaTemporanea"));
                    }
                    else
                    {
                        CmdInit.This.UIApplication.ActiveUIDocument.Document.SaveAs(docPathNameNew);
                    }
                    
                }
            }
        }
    }

    public class SaveEventHandler
    {
        public void Initialize(UIApplication uiapp)
        {
            // Ottieni il comando Save
            RevitCommandId saveCommandId = RevitCommandId.LookupPostableCommandId(PostableCommand.Save);

            // Crea un binding per l'evento BeforeExecuted
            uiapp.CreateAddInCommandBinding(saveCommandId).BeforeExecuted += new EventHandler<BeforeExecutedEventArgs>(OnBeforeSaveExecuted);
        }

        private void OnBeforeSaveExecuted(object sender, BeforeExecutedEventArgs e)
        {
            string docPathName = e.ActiveDocument.PathName;
            if (docPathName.StartsWith(JoinService.This.AppSettingsPath))
            {
                e.Cancel = true;
                MessageBox.Show(LocalizationProvider.GetString("ImpossibileSalvareNellaCartellaTemporanea"));
            }
        }
    }
}
