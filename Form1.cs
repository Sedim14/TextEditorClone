using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TextEditorClone
{
    public partial class Form1 : Form
    {
        TabPage selectedTab = null;
        bool isSaved = false;
        Dictionary<TabPage, string> filePaths = new Dictionary<TabPage, string>();

        Dictionary<string, Image> emojis = new Dictionary<string, Image>
        {
            { ":D", Properties.Resources.smile },
            { ":|", Properties.Resources.confused },
            { ">:c", Properties.Resources.angry },
            { ":,c", Properties.Resources.sad },
            { ":3", Properties.Resources.cat_face }
        };

        Dictionary<Image, string> emojiToText = new Dictionary<Image, string>();

        public Form1()
        {
            InitializeComponent();
            selectedTab = fileTabConrol.TabPages[0]; //1st tab will always be selected everytime the forms load
            createTempFile(selectedTab);
            updateFileStatus(selectedTab);

            // Invertir el diccionario emojis para facilitar el reemplazo al guardar
            foreach (var entry in emojis)
            {
                emojiToText[entry.Value] = entry.Key;
            }
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
                    richTextBox1.Text.Trim();
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
                string key = entry.Key;  // El emoji en formato de texto (e.g., ":D")
                Image value = entry.Value;  // La imagen correspondiente

                int index = richTextBox1.Text.IndexOf(key);
                while (index != -1)
                {
                    // Selecciona el texto del emoji
                    richTextBox1.Select(index, key.Length);

                    // Redimensiona la imagen al tamaño del texto
                    Image resizedEmoji = ResizeImage(value, (int)richTextBox1.Font.Size);

                    // Reemplaza el texto por la imagen redimensionada
                    Clipboard.SetImage(resizedEmoji);  // Copia la imagen redimensionada al portapapeles
                    richTextBox1.Paste();  // Pega la imagen en la posición seleccionada

                    // Busca la siguiente ocurrencia
                    index = richTextBox1.Text.IndexOf(key, index + 1);
                }
            }
        }

        private Image ResizeImage(Image imgToResize, int size)
        {
            int newWidth = size+5; // Ancho igual al tamaño de la fuente
            int newHeight = size+5; // Alto igual al tamaño de la fuente

            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.DrawImage(imgToResize, 0, 0, newWidth, newHeight);
            }
            return resizedImage;
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
                richTextBox1.Text = string.Empty; // Clear all text in textbox
                // Look for the temp file of the current selected tab and put it in the textbox
                string tmpName = "/tmp" + selectedTab.Text + ".txt";
                string tmpPath = AppDomain.CurrentDomain.BaseDirectory + tmpName;
                if (File.Exists(tmpPath))
                {
                    richTextBox1.Text = File.ReadAllText(tmpPath);
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

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = selectedTab.Text;

            if (fileName.StartsWith("New"))
            {
                saveFileAs();
            }
            else
            {
                // Obtén la ruta asociada a la pestaña seleccionada
                string filePath;
                if (filePaths.TryGetValue(selectedTab, out filePath) && File.Exists(filePath))
                {
                    // Guarda el contenido del TextBox en el archivo existente
                    File.WriteAllText(filePath, richTextBox1.Text);

                    updateTempFile(selectedTab);
                    updateFileStatus(selectedTab);
                    MessageBox.Show("Archivo guardado exitosamente.");
                }
                else
                {
                    saveFileAs();
                }
            }
        }

        private void saveFileAs() //Funciona correctamente-completa
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter = "Archivo de texto|*.txt";

            DialogResult res = saveFileDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                string fileName = Path.GetFileName(saveFileDialog.FileName);
                selectedTab.Text = fileName;

                // Guardar la ruta del archivo en el diccionario
                filePaths[selectedTab] = saveFileDialog.FileName;

                File.WriteAllText(saveFileDialog.FileName, richTextBox1.Text);
                MessageBox.Show("Archivo guardado exitosamente como: " + fileName);

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
            File.WriteAllText(tmpPath, richTextBox1.Text);//Write new content on the textbox
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
                string filePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(filePath);

                // Carga el contenido del archivo en el TextBox
                richTextBox1.Text = File.ReadAllText(filePath);

                // Actualiza el nombre de la pestaña y la ruta del archivo en el diccionario
                selectedTab.Text = fileName;
                filePaths[selectedTab] = filePath; // Aquí se actualiza la ruta del archivo

                // Actualiza el estado del archivo en la interfaz
                updateFileStatus(selectedTab);
            }
            else
            {
                MessageBox.Show("Archivo invalido");
            }

        }

        

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Obtén la pestaña activa actual
            TabPage activeTab = fileTabConrol.SelectedTab;

            if (activeTab != null)
            {
                // Verifica si hay cambios no guardados
                if (!isSaved)
                {
                    // Pregunta al usuario si desea guardar los cambios antes de cerrar la pestaña
                    DialogResult result = MessageBox.Show("Hay cambios no guardados. ¿Desea guardarlos antes de cerrar?", "Confirmar", MessageBoxButtons.YesNoCancel);

                    if (result == DialogResult.Cancel)
                    {
                        // Si el usuario elige Cancelar, no cierra la pestaña
                        return;
                    }
                    else if (result == DialogResult.Yes)
                    {
                        // Si el usuario elige Sí, guarda los cambios
                        saveToolStripMenuItem_Click(sender, e);
                    }
                }

                // Elimina la pestaña activa
                fileTabConrol.TabPages.Remove(activeTab);

                // Opcional: Actualiza el estado de los archivos y pestañas después de cerrar
                if (fileTabConrol.TabPages.Count > 0)
                {
                    // Si hay otras pestañas, selecciona la primera
                    fileTabConrol.SelectedTab = fileTabConrol.TabPages[0];
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.createNewFileTab(sender, e);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.openToolStripMenuItem_Click(sender,e);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.saveToolStripMenuItem_Click((object)sender, e);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            this.clickSaveAs(sender, e);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            this.closeToolStripMenuItem_Click(sender,e);
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            this.closeAllToolStripMenuItem_Click(sender,e);
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.None)
            {
                if (e.KeyCode == Keys.Enter)
                    richTextBox1.Text = richTextBox1.Text.Trim();
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

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Trigger emoji replacement after every key press
            checkForEmojis();
            //Write current content to the tmp file
            updateTempFile(selectedTab);
            //compare the current tmp file to the original file
            updateFileStatus(selectedTab);
        }

        
    }
}
