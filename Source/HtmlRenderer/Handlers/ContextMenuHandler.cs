// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they bagin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using HtmlRenderer.Dom;
using HtmlRenderer.Entities;
using HtmlRenderer.Utils;

namespace HtmlRenderer.Handlers
{
    /// <summary>
    /// Handle context menu.
    /// </summary>
    internal sealed class ContextMenuHandler : IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// select all text
        /// </summary>
        private static readonly string _selectAll;

        /// <summary>
        /// copy selected text
        /// </summary>
        private static readonly string _copy;

        /// <summary>
        /// copy the link source
        /// </summary>
        private static readonly string _copyLink;

        /// <summary>
        /// open link (as left mouse click)
        /// </summary>
        private static readonly string _openLink;

        /// <summary>
        /// copy the source of the image
        /// </summary>
        private static readonly string _copyImageLink;

        /// <summary>
        /// copy image to clipboard
        /// </summary>
        private static readonly string _copyImage;

        /// <summary>
        /// save image to disk
        /// </summary>
        private static readonly string _saveImage;

        /// <summary>
        /// open video in browser
        /// </summary>
        private static readonly string _openVideo;

        /// <summary>
        /// copy video url to browser
        /// </summary>
        private static readonly string _copyVideoUrl;

        /// <summary>
        /// the selection handler linked to the context menu handler
        /// </summary>
        private readonly SelectionHandler _selectionHandler;

        /// <summary>
        /// the html container the handler is on
        /// </summary>
        private readonly HtmlContainer _htmlContainer;

        /// <summary>
        /// the last conext menu shown
        /// </summary>
        private ContextMenuStrip _contextMenu;

        /// <summary>
        /// the control that the context menu was shown on
        /// </summary>
        private Control _parentControl;

        /// <summary>
        /// the css rect that context menu shown on
        /// </summary>
        private CssRect _currentRect;
        
        /// <summary>
        /// the css link box that context menu shown on
        /// </summary>
        private CssBox _currentLink;

        #endregion


        /// <summary>
        /// Init context menu items strings.
        /// </summary>
        static ContextMenuHandler()
        {
            if(CultureInfo.CurrentUICulture.Name.StartsWith("fr",StringComparison.InvariantCultureIgnoreCase))
            {
                _selectAll = "Tout sélectionner";
                _copy = "Copier";
                _copyLink = "Copier l'adresse du lien";
                _openLink = "Ouvrir le lien";
                _copyImageLink = "Copier l'URL de l'image";
                _copyImage = "Copier l'image";
                _saveImage = "Enregistrer l'image sous...";
                _openVideo = "Ouvrir la vidéo";
                _copyVideoUrl = "Copier l'URL de l'vidéo";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("de", StringComparison.InvariantCultureIgnoreCase))
            {
                _selectAll = "Alle auswählen";
                _copy = "Kopieren";
                _copyLink = "Link-Adresse kopieren";
                _openLink = "Link öffnen";
                _copyImageLink = "Bild-URL kopieren";
                _copyImage = "Bild kopieren";
                _saveImage = "Bild speichern unter...";
                _openVideo = "Video öffnen";
                _copyVideoUrl = "Video-URL kopieren";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("it", StringComparison.InvariantCultureIgnoreCase))
            {
                _selectAll = "Seleziona tutto";
                _copy = "Copia";
                _copyLink = "Copia indirizzo del link";
                _openLink = "Apri link";
                _copyImageLink = "Copia URL immagine";
                _copyImage = "Copia immagine";
                _saveImage = "Salva immagine con nome...";
                _openVideo = "Apri il video";
                _copyVideoUrl = "Copia URL video";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("es", StringComparison.InvariantCultureIgnoreCase))
            {
                _selectAll = "Seleccionar todo";
                _copy = "Copiar";
                _copyLink = "Copiar dirección de enlace";
                _openLink = "Abrir enlace";
                _copyImageLink = "Copiar URL de la imagen";
                _copyImage = "Copiar imagen";
                _saveImage = "Guardar imagen como...";
                _openVideo = "Abrir video";
                _copyVideoUrl = "Copiar URL de la video";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("ru", StringComparison.InvariantCultureIgnoreCase))
            {
                _selectAll = "Выбрать все";
                _copy = "Копировать";
                _copyLink = "Копировать адрес ссылки";
                _openLink = "Перейти по ссылке";
                _copyImageLink = "Копировать адрес изображения";
                _copyImage = "Копировать изображение";
                _saveImage = "Сохранить изображение как...";
                _openVideo = "Открыть видео";
                _copyVideoUrl = "Копировать адрес видео";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("sv", StringComparison.InvariantCultureIgnoreCase))
            {
                _selectAll = "Välj allt";
                _copy = "Kopiera";
                _copyLink = "Kopiera länkadress";
                _openLink = "Öppna länk";
                _copyImageLink = "Kopiera bildens URL";
                _copyImage = "Kopiera bild";
                _saveImage = "Spara bild som...";
                _openVideo = "Öppna video";
                _copyVideoUrl = "Kopiera video URL";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("hu", StringComparison.InvariantCultureIgnoreCase))
            {
                _selectAll = "Összes kiválasztása";
                _copy = "Másolás";
                _copyLink = "Hivatkozás címének másolása";
                _openLink = "Hivatkozás megnyitása";
                _copyImageLink = "Kép URL másolása";
                _copyImage = "Kép másolása";
                _saveImage = "Kép mentése másként...";
                _openVideo = "Videó megnyitása";
                _copyVideoUrl = "Videó URL másolása";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("cs", StringComparison.InvariantCultureIgnoreCase))
            {
                _selectAll = "Vybrat vše";
                _copy = "Kopírovat";
                _copyLink = "Kopírovat adresu odkazu";
                _openLink = "Otevřít odkaz";
                _copyImageLink = "Kopírovat URL snímku";
                _copyImage = "Kopírovat snímek";
                _saveImage = "Uložit snímek jako...";
                _openVideo = "Otevřít video";
                _copyVideoUrl = "Kopírovat URL video";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("da", StringComparison.InvariantCultureIgnoreCase))
            {
                _selectAll = "Vælg alt";
                _copy = "Kopiér";
                _copyLink = "Kopier link-adresse";
                _openLink = "Åbn link";
                _copyImageLink = "Kopier billede-URL";
                _copyImage = "Kopier billede";
                _saveImage = "Gem billede som...";
                _openVideo = "Åbn video";
                _copyVideoUrl = "Kopier video-URL";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("nl", StringComparison.InvariantCultureIgnoreCase))
            {
                _selectAll = "Alles selecteren";
                _copy = "Kopiëren";
                _copyLink = "Link adres kopiëren";
                _openLink = "Link openen";
                _copyImageLink = "URL Afbeelding kopiëren";
                _copyImage = "Afbeelding kopiëren";
                _saveImage = "Bewaar afbeelding als...";
                _openVideo = "Video openen";
                _copyVideoUrl = "URL video kopiëren";
            }
            else if (CultureInfo.CurrentUICulture.Name.StartsWith("fi", StringComparison.InvariantCultureIgnoreCase))
            {
                _selectAll = "Valitse kaikki";
                _copy = "Kopioi";
                _copyLink = "Kopioi linkin osoite";
                _openLink = "Avaa linkki";
                _copyImageLink = "Kopioi kuvan URL";
                _copyImage = "Kopioi kuva";
                _saveImage = "Tallena kuva nimellä...";
                _openVideo = "Avaa video";
                _copyVideoUrl = "Kopioi video URL";
            }
            else
            {
                _selectAll = "Select all";
                _copy = "Copy";
                _copyLink = "Copy link address";
                _openLink = "Open link";
                _copyImageLink = "Copy image URL";
                _copyImage = "Copy image";
                _saveImage = "Save image as...";
                _openVideo = "Open video";
                _copyVideoUrl = "Copy video URL";
            }
        }

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="selectionHandler">the selection handler linked to the context menu handler</param>
        /// <param name="htmlContainer">the html container the handler is on</param>
        public ContextMenuHandler(SelectionHandler selectionHandler, HtmlContainer htmlContainer)
        {
            ArgChecker.AssertArgNotNull(selectionHandler, "selectionHandler");
            ArgChecker.AssertArgNotNull(htmlContainer, "htmlContainer");

            _selectionHandler = selectionHandler;
            _htmlContainer = htmlContainer;
        }

        /// <summary>
        /// Show context menu clicked on given rectangle.
        /// </summary>
        /// <param name="parent">the parent control to show the context menu on</param>
        /// <param name="rect">the rectangle that was clicked to show context menu</param>
        /// <param name="link">the link that was clicked to show context menu on</param>
        public void ShowContextMenu(Control parent, CssRect rect, CssBox link)
        {
            try
            {
                DisposeContextMenu();

                _parentControl = parent;
                _currentRect = rect;
                _currentLink = link;
                _contextMenu = new ContextMenuStrip();
                _contextMenu.ShowImageMargin = false;
            
                if(rect != null)
                {
                    bool isVideo = false;
                    if (link != null)
                    {
                        isVideo = link is CssBoxFrame && ((CssBoxFrame)link).IsVideo;
                        var openLink = _contextMenu.Items.Add(isVideo ? _openVideo : _openLink, null, OnOpenLinkClick);
                        if(_htmlContainer.IsSelectionEnabled)
                        {
                            var copyLink = _contextMenu.Items.Add(isVideo ? _copyVideoUrl : _copyLink, null, OnCopyLinkClick);
                            copyLink.Enabled = !string.IsNullOrEmpty(link.HrefLink);
                        }
                        openLink.Enabled = !string.IsNullOrEmpty(link.HrefLink);
                        _contextMenu.Items.Add("-");
                    }

                    if (rect.IsImage && !isVideo)
                    {
                        var saveImage = _contextMenu.Items.Add(_saveImage, null, OnSaveImageClick);
                        if(_htmlContainer.IsSelectionEnabled)
                        {
                            var copyImageUrl = _contextMenu.Items.Add(_copyImageLink, null, OnCopyImageLinkClick);
                            var copyImage = _contextMenu.Items.Add(_copyImage, null, OnCopyImageClick);
                            copyImageUrl.Enabled = !string.IsNullOrEmpty(_currentRect.OwnerBox.GetAttribute("src"));
                            copyImage.Enabled = rect.Image != null;
                        }
                        saveImage.Enabled = rect.Image != null;
                        _contextMenu.Items.Add("-");
                    }

                    if(_htmlContainer.IsSelectionEnabled)
                    {
                        var copy = _contextMenu.Items.Add(_copy, null, OnCopyClick);
                        copy.Enabled = rect.Selected;
                    }
                }

                if(_htmlContainer.IsSelectionEnabled)
                {
                    _contextMenu.Items.Add(_selectAll, null, OnSelectAllClick);
                }

                if(_contextMenu.Items.Count > 0)
                {
                    if(_contextMenu.Items[_contextMenu.Items.Count-1].Text == string.Empty)
                        _contextMenu.Items.RemoveAt(_contextMenu.Items.Count - 1);
                    _contextMenu.Show(parent, parent.PointToClient(Control.MousePosition));                    
                }
            }
            catch (Exception ex)
            {
                _htmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to show context menu", ex);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            DisposeContextMenu();
        }


        #region Private methods

        /// <summary>
        /// Dispose of the last used context menu.
        /// </summary>
        private void DisposeContextMenu()
        {
            try
            {
                if (_contextMenu != null)
                    _contextMenu.Dispose();
                _contextMenu = null;
                _parentControl = null;
                _currentRect = null;
                _currentLink = null;
            }
            catch
            {}
        }

        /// <summary>
        /// Handle link click.
        /// </summary>
        private void OnOpenLinkClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var mp = _parentControl.PointToClient(Control.MousePosition);
                _currentLink.HtmlContainer.HandleLinkClicked(_parentControl, new MouseEventArgs(MouseButtons.None, 0, mp.X, mp.Y, 0), _currentLink);
            }
            catch (HtmlLinkClickedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _htmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to open link", ex);
            }
            finally
            {
                DisposeContextMenu();                
            }
        }

        /// <summary>
        /// Copy the href of a link to clipboard.
        /// </summary>
        private void OnCopyLinkClick(object sender, EventArgs eventArgs)
        {
            try
            {
                Clipboard.SetText(_currentLink.HrefLink);
            }
            catch (Exception ex)
            {
                _htmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to copy link url to clipboard", ex);                
            }
            finally
            {
                DisposeContextMenu();
            }
        }

        /// <summary>
        /// Open save as dialog to save the image
        /// </summary>
        private void OnSaveImageClick(object sender, EventArgs eventArgs)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    var imageSrc = _currentRect.OwnerBox.GetAttribute("src");
                    saveDialog.DefaultExt = Path.GetExtension(imageSrc) ?? "png";
                    saveDialog.FileName = Path.GetFileName(imageSrc) ?? "image";
                    saveDialog.Filter = "Images|*.png;*.bmp;*.jpg";

                    if(saveDialog.ShowDialog(_parentControl) == DialogResult.OK)
                    {
                        _currentRect.Image.Save(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                _htmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to save image", ex);
            }
            finally
            {
                DisposeContextMenu();
            }
        }

        /// <summary>
        /// Copy the image source to clipboard.
        /// </summary>
        private void OnCopyImageLinkClick(object sender, EventArgs eventArgs)
        {
            try
            {
                Clipboard.SetText(_currentRect.OwnerBox.GetAttribute("src"));
            }
            catch (Exception ex)
            {
                _htmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to copy image url to clipboard", ex);                
            }
            finally
            {
                DisposeContextMenu();
            }
        }

        /// <summary>
        /// Copy image object to clipboard.
        /// </summary>
        private void OnCopyImageClick(object sender, EventArgs eventArgs)
        {
            try
            {
                Clipboard.SetImage(_currentRect.Image);
            }
            catch (Exception ex)
            {
                _htmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to copy image to clipboard", ex);                
            }
            finally
            {
                DisposeContextMenu();
            }
        }

        /// <summary>
        /// Copy selected text.
        /// </summary>
        private void OnCopyClick(object sender, EventArgs eventArgs)
        {
            try
            {
                _selectionHandler.CopySelectedHtml();
            }
            catch (Exception ex)
            {
                _htmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to copy text to clipboard", ex);                
            }
            finally
            {
                DisposeContextMenu();
            }
        }

        /// <summary>
        /// Select all text.
        /// </summary>
        private void OnSelectAllClick(object sender, EventArgs eventArgs)
        {
            try
            {
                _selectionHandler.SelectAll(_parentControl);
            }
            catch (Exception ex)
            {
                _htmlContainer.ReportError(HtmlRenderErrorType.ContextMenu, "Failed to select all text", ex);
            }
            finally
            {
                DisposeContextMenu();
            }
        }

        #endregion
    }
}
