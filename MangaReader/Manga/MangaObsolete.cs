﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using MangaReader.Manga.Grouple;
using MangaReader.Properties;
using MangaReader.Services;

namespace MangaReader.Manga
{
  /// <summary>
  /// Манга.
  /// </summary>
  [Obsolete]
  public class Manga : INotifyPropertyChanged
  {
    #region Свойства

    /// <summary>
    /// Название манги.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Ссылка на мангу.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Статус манги.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Нужно ли обновлять мангу.
    /// </summary>
    public bool NeedUpdate
    {
      get { return _needUpdate; }
      set
      {
        _needUpdate = value;
        OnPropertyChanged("NeedUpdate");
      }
    }

    private bool _needUpdate = true;

    /// <summary>
    /// Обложка.
    /// </summary>
    public byte[] Cover { get; set; }

    /// <summary>
    /// Статус корректности манги.
    /// </summary>
    public bool IsValid
    {
      get { return !string.IsNullOrWhiteSpace(this.Name) && this.listOfChapters != null; }
    }

    /// <summary>
    /// Папка с мангой.
    /// </summary>
    public string Folder
    {
      get { return Page.MakeValidPath(Settings.DownloadFolder + "\\" + this.Name); }
    }

    /// <summary>
    /// Статус перевода.
    /// </summary>
    public string IsCompleted
    {
      get
      {
        var match = Regex.Match(this.Status, Strings.Manga_IsCompleted);
        return match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : null;
      }
    }

    /// <summary>
    /// Статус загрузки.
    /// </summary>
    public bool IsDownloaded
    {
      get { return false; }
    }

    /// <summary>
    /// Процент загрузки манги.
    /// </summary>
    public double Downloaded
    {
      get { return 0; }
      set { }
    }

    /// <summary>
    /// Закешированный список глав.
    /// </summary>
    private List<Chapter> allChapters;

    /// <summary>
    /// Список глав, ссылка-описание.
    /// </summary>
    private Dictionary<string, string> listOfChapters;


    #endregion

    public event EventHandler<Manga> DownloadProgressChanged;

    protected virtual void OnDownloadProgressChanged(Manga manga)
    {
      var handler = DownloadProgressChanged;
      if (handler != null)
        handler(this, manga);
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string property)
    {
      var handler = PropertyChanged;
      if (handler != null)
      {
        handler(this, new PropertyChangedEventArgs(property));
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Обновить информацию о манге - название, главы, обложка.
    /// </summary>
    public void Refresh()
    {
      var page = Page.GetPage(this.Url);
      if (string.IsNullOrWhiteSpace(page))
        return;

      this.Name = Getter.GetMangaName(page).ToString();
      this.listOfChapters = Getter.GetLinksOfMangaChapters(page, this.Url);
      this.Status = Getter.GetTranslateStatus(page);
      OnPropertyChanged("IsCompleted");
    }

    /// <summary>
    /// Получить список глав.
    /// </summary>
    /// <returns>Список глав.</returns>
    public List<Chapter> GetAllChapters()
    {
      if (listOfChapters == null)
        listOfChapters = Getter.GetLinksOfMangaChapters(Page.GetPage(this.Url), this.Url);
      this.allChapters = allChapters ??
             (allChapters = listOfChapters.Select(link => new Chapter(link.Key, link.Value, new Readmanga())).ToList());
      this.allChapters.ForEach(ch => ch.DownloadProgressChanged += (sender, args) => this.DownloadProgressChanged(ch, this));
      return this.allChapters;
    }

    /// <summary>
    /// Скачать все главы.
    /// </summary>
    public void Download(string mangaFolder = null, string volumePrefix = null, string chapterPrefix = null)
    {
      throw new NotImplementedException("Obsolete class");
    }

    public override string ToString()
    {
      return this.Name;
    }

    #endregion

    #region Конструктор

    /// <summary>
    /// Открыть мангу.
    /// </summary>
    /// <param name="url">Ссылка на мангу.</param>
    public Manga(string url)
    {
      this.Url = url;
      this.Refresh();
    }

    public Manga() { }

    #endregion
  }
}
