﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TextEditorClone
{
    public partial class Form1 : Form
    {
        TabPage selectedTab = null;
        bool isSaved = false;

        Dictionary<string, string> emojis = new Dictionary<string, string>
        {
            { ":D", "😄" },
            { ":|", "😠" },
            { ">:c", "👍" },
            { ":,c", "😢" },
            { ":3", "😺" }
        };              

        public Form1()
        {
            InitializeComponent();
            selectedTab = fileTabConrol.TabPages[0]; //1st tab will always be selected everytime the forms load
            createTempFile(selectedTab);
            updateFileStatus(selectedTab);
        }

  
        //Cuando se abre por 1era vez
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        
        //Windos forms methods
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.None)
            {
                if (e.KeyCode == Keys.Enter)
                    textBox1.Text.Trim();
                else
                {
                    //Check if any emoji has been written
                    checkForEmojis();
                    //Write current content to the tmp file
                    updateTempFile(selectedTab);
                    //compare the current tmp file to the og file
                    updateFileStatus(selectedTab);
                }
            } 
        }

        private void textBox1_KeyDown(object sender, KeyPressEventArgs e)
        {
            //Check if any emoji has been written
            checkForEmojis();
            //Write current content to the tmp file
            updateTempFile(selectedTab);
            //compare the current tmp file to the og file
            updateFileStatus(selectedTab);
        }

        private void checkForEmojis()
        {
            foreach (var entry in emojis)
            {
                string key = entry.Key;
                string value = entry.Value;
                string newText = textBox1.Text.Replace(key, value);
                textBox1.Text = newText;
            }
        }
        
        private void updateFileStatus(TabPage fileTab)
        {
            //Show name of the current file
            FileStatus.Items[0].Text = "File: " + fileTab.Text;
            //Check if it saved or not
            checkFileStatus(fileTab);

        }

        private void createNewFileTab(object sender, EventArgs e)
        {
            addNewTab();
        }

        private void addNewTab()
        {
            string newTabName = "New" + (fileTabConrol.TabPages.Count + 1);
            fileTabConrol.TabPages.Add(newTabName); //Add bew tab
            createTempFile(fileTabConrol.TabPages[fileTabConrol.TabPages.Count - 1]);//crea el archivo temporal del nuevo tab
        }

        private void updateSelectedTab(object sender, EventArgs e) //Solo se manda a llamar entre tabs existentes, no en uno que se acaba de crear
        {

            if(fileTabConrol.SelectedTab != null)
            {
                // Actualizamos la informacion en ek archivo tmp antes de movernos al tab del archivo seleccionado
                updateTempFile(selectedTab);
                //Accedemos al nuevo tab
                selectedTab = fileTabConrol.SelectedTab;
                textBox1.Text = string.Empty; // Clear all text in textbox
                // Look for the temp file of the current selected tab and put it in the textbox
                string tmpName = "/tmp" + selectedTab.Text + ".txt";
                string tmpPath = AppDomain.CurrentDomain.BaseDirectory + tmpName;
                if (File.Exists(tmpPath))
                {
                    textBox1.Text = File.ReadAllText(tmpPath);
                    updateFileStatus(selectedTab);
                }
            }


            /*
            TabPage previousTab = selectedTab;
            selectedTab = fileTabConrol.SelectedTab;

            if (selectedTab != null)
            {
                // Save the previous info of the previous tab to its temp file
                updateTempFile(previousTab);
                textBox1.Text = string.Empty; // Clear all text in textbox
                // Look for the temp file of the current selected tab and put it in the textbox
                string tmpName = "/tmp" + selectedTab.Text + ".txt";
                string tmpPath = AppDomain.CurrentDomain.BaseDirectory + tmpName;
                if (File.Exists(tmpPath))
                {
                    textBox1.Text = File.ReadAllText(tmpPath);
                }
            }*/

        }

        //File management
        private void checkFileStatus(TabPage fileTab) //Checa el estado actual del tab seleccionado
        {

            string savedFileRoute = FindFile(fileTab.Text); // Verifica si el archivo (no la versión temporal) existe

            if (savedFileRoute != null)
            {
                // Accede al contenido del archivo encontrado y su versión temporal y compáralos
                string tmpName = "/tmp" + fileTab.Text + ".txt";
                string tmpPath = AppDomain.CurrentDomain.BaseDirectory + tmpName;

                string fileContent = File.ReadAllText(savedFileRoute);
                string tmpFileContent = File.ReadAllText(tmpPath);

                if (fileContent.Equals(tmpFileContent))
                    FileStatus.Items[1].Text = "Status: Saved";
                else
                    FileStatus.Items[1].Text = "Status: Not Saved";
            }
            else
            {
                FileStatus.Items[1].Text = "Status: Not Saved";
            }
        }

        static string FindFile(string fileName) //Retorna un string donde representa la ruta en donde se encontro el archivo
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filesDirectory = "C:\\Users\\Dell\\Desktop\\Universidad\\5toSemestre\\InterfacesGráficasConAplicaciones\\Parcial1\\TextEditorClone\\Files\\";

            try
            {
                foreach (string file in Directory.GetFiles(filesDirectory, fileName, SearchOption.AllDirectories))
                {
                    return file;
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("No se tiene acceso al directorio: " + filesDirectory);
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("El directorio no se encontró: " + filesDirectory);
            }

            return null;
        }

        private void saveFileAs() //Funciona correctamente-completa
        {
            //Accedemos a la routa del archivo temporal
            string tmpName = "/tmp" + selectedTab.Text + ".txt";
            string tmpPath = AppDomain.CurrentDomain.BaseDirectory + tmpName;


            //Open save dile dialog and save
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter = "Archivo de texto|*.txt";
            DialogResult res = saveFileDialog.ShowDialog();
            if (res == DialogResult.OK) //If it was saved correctly
            {
                File.WriteAllText(saveFileDialog.FileName, textBox1.Text);
                // Get the file name and display it
                string fileName = Path.GetFileName(saveFileDialog.FileName);
                MessageBox.Show("File saved succesfully: " + fileName);
                selectedTab.Text = fileName;
                //Renombramos el archivo temporal al nombre que se guardo
                string newTmpName = "/tmp" + fileName + ".txt";
                string newTmpPath = AppDomain.CurrentDomain.BaseDirectory + newTmpName;
                File.Move(tmpPath, newTmpPath);
                //Muestra el estado del archivo en el forms
                updateFileStatus(selectedTab);
            }
                
        }


        //Temporary file management methods
        private void createTempFile(TabPage fileTab)//funciona correctamente-completa
        {
            //El nombre de los archivos temporales tendran la siguiente estructura tmpTabName,txt
            string tmpName = "/tmp" + fileTab.Text + ".txt";
            string tmpPath = AppDomain.CurrentDomain.BaseDirectory +tmpName;
            File.WriteAllText(tmpPath, string.Empty);
        }

        private void updateTempFile(TabPage fileTab)//Funciona correctamente-completa
        {
            string tmpName = "/tmp" + fileTab.Text + ".txt";
            string tmpPath = AppDomain.CurrentDomain.BaseDirectory + tmpName;
            File.WriteAllText(tmpPath, string.Empty);//Emptr current content of file
            File.WriteAllText(tmpPath, textBox1.Text);//Write new content on the textbox
        }

        private void deleteTempFiles()//Funciona correctamente-completa
        {
            string directoryPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePrefix = "tmp";

            DeleteFilesWithPrefix(directoryPath, filePrefix);
        }

        static void DeleteFilesWithPrefix(string directoryPath, string filePrefix)//Funciona correctamente-completa
        {
            try
            {
                // Get all files in the directory with the specified prefix
                string[] files = Directory.GetFiles(directoryPath, filePrefix + "*");

                foreach (string file in files)
                {
                    // Delete the file
                    File.Delete(file);
                    Console.WriteLine($"Deleted: {file}");
                }

                Console.WriteLine("All matching files have been deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            deleteTempFiles();
        }

        private void clickSaveAs(object sender, EventArgs e)
        {
            saveFileAs();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog.Title = "Seleccionar un archivo de texto";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {

                string fileContent = File.ReadAllText(openFileDialog.FileName);
                string fileName = Path.GetFileName(openFileDialog.FileName);
                textBox1.Text = fileContent;
                selectedTab.Text = fileName;
                updateFileStatus(selectedTab);
            }
            else
            {
                MessageBox.Show("Archivo invalido");
            }



        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
