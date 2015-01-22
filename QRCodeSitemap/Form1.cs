using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Gma.QrCodeNet.Encoding;

namespace QRCodeSitemap
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            QrEncoder _objQRCodeEncoder = new QrEncoder(ErrorCorrectionLevel.M);
            QrCode _objQRCode;
            GraphicsRenderer _objQRCodeRenderer = new GraphicsRenderer(new FixedModuleSize(4, QuietZoneModules.Two), Brushes.Black, Brushes.White);

            // Affiche la boîte de dialogue d'enregistrement.
            saveFileDialog1.Filter = "Fichier HTML (*.html)|*.html";
            saveFileDialog1.FileName = "index.html";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Créé un répertoire qui contiendra les codes QR de chaque lien.
                string _strQRCodeRepertoire = String.Format("{0}\\images\\", Path.GetDirectoryName(saveFileDialog1.FileName));
                Directory.CreateDirectory(_strQRCodeRepertoire);

                // Charge le contenu du fichier XML.
                XDocument _objXmlDocument = XDocument.Load(textBox1.Text);
                XNamespace _objXmlNameSpace = _objXmlDocument.Root.GetDefaultNamespace();

                // Créé le fichier HTML.
                var _objXmlSettings = new XmlWriterSettings();
                _objXmlSettings.Indent = true;
                using (var _objXmlWriter = XmlWriter.Create(saveFileDialog1.FileName, _objXmlSettings))
                {
                    // Doctype.
                    _objXmlWriter.WriteDocType("html", "-//W3C//DTD XHTML 1.0 Transitional//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd", null);

                    // <html>
                    _objXmlWriter.WriteStartElement("html");

                    // <head>
                    _objXmlWriter.WriteStartElement("head");

                    // <title></title>
                    _objXmlWriter.WriteStartElement("title");
                    _objXmlWriter.WriteValue(String.Format("Index de \"{0}\"", textBox1.Text));
                    _objXmlWriter.WriteEndElement();

                    // </head>
                    _objXmlWriter.WriteEndElement();

                    // <body>
                    _objXmlWriter.WriteStartElement("body");

                    // <h1></h1>
                    _objXmlWriter.WriteStartElement("h1");
                    _objXmlWriter.WriteValue(textBox1.Text);
                    _objXmlWriter.WriteEndElement();

                    // <table>
                    _objXmlWriter.WriteStartElement("table");

                    // Génère un tableau de tous les liens.
                    int i = 0;
                    IEnumerable<XElement> _objXmlElements = from element in _objXmlDocument.Descendants(_objXmlNameSpace + "loc") select element;
                    foreach (XElement _objXmlElement in _objXmlElements)
                    {
                        string _strQRCode = String.Empty;

                        // Déclare un objet "MemoryStream" pour stocker le fichier PNG du code QR.
                        MemoryStream _objMemoryStream = new MemoryStream();

                        // Créé le code QR.
                        _objQRCodeEncoder.TryEncode(_objXmlElement.Value, out _objQRCode);

                        // Définit le chemin de destination du code QR.
                        _strQRCode = String.Format("{0}\\images\\qrcode_{1}.png", Path.GetDirectoryName(saveFileDialog1.FileName), i);
                        
                        // Enregistrement du code QR.
                        _objQRCodeRenderer.WriteToStream(_objQRCode.Matrix, ImageFormat.Png, _objMemoryStream);
                        using (FileStream _objFileStream = new FileStream(_strQRCode, FileMode.Create, FileAccess.Write)) {
                            _objMemoryStream.WriteTo(_objFileStream);
                        };

                        // <tr>
                        _objXmlWriter.WriteStartElement("tr");

                        // <td></td> : première colonne : QRCode.
                        _objXmlWriter.WriteStartElement("td");
                        _objXmlWriter.WriteStartElement("img");
                        _objXmlWriter.WriteStartAttribute("src");
                        _objXmlWriter.WriteValue(String.Format("images/qrcode_{0}.png", i));
                        _objXmlWriter.WriteEndAttribute();
                        _objXmlWriter.WriteEndElement();
                        _objXmlWriter.WriteEndElement();

                        // <td></td> : deuxième colonne : lien.
                        _objXmlWriter.WriteStartElement("td");
                        _objXmlWriter.WriteStartElement("a");
                        _objXmlWriter.WriteStartAttribute("href");
                        _objXmlWriter.WriteValue(_objXmlElement.Value);
                        _objXmlWriter.WriteEndAttribute();
                        _objXmlWriter.WriteValue(_objXmlElement.Value);
                        _objXmlWriter.WriteEndElement();
                        _objXmlWriter.WriteEndElement();

                        // </tr>
                        _objXmlWriter.WriteEndElement();

                        i++;
                    }

                    // </table>
                    _objXmlWriter.WriteEndElement();

                    // </body>
                    _objXmlWriter.WriteEndElement();

                    // </html>
                    _objXmlWriter.WriteEndElement();

                    // Fin du document.
                    _objXmlWriter.WriteEndDocument();
                }

                // Affiche l'index généré.
                Process p = new Process();
                p.StartInfo.FileName = saveFileDialog1.FileName;
                p.Start();
            }
        }
    }
}
