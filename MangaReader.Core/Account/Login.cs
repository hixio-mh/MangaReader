﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MangaReader.Core.Manga;
using MangaReader.Core.Services;

namespace MangaReader.Core.Account
{
  public abstract class Login : Entity.Entity
  {
    public static Guid Type { get { return Guid.Empty; } }

    public bool IsLogined
    {
      get { return isLogined; }
      set
      {
        isLogined = value;
        OnLoginStateChanged(value);
      }
    }

    public event EventHandler<bool> LoginStateChanged;

    public virtual string Name { get; set; }

    public virtual string Password { get; set; }

    public virtual bool CanLogin { get { return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Password); } }

    public abstract Uri MainUri { get; set; }

    public abstract Uri LogoutUri { get; }

    public abstract Uri BookmarksUri { get; }

    /// <summary>
    /// Указатель блокировки клиента файла.
    /// </summary>
    protected internal object ClientLock { get; set; }

    /// <summary>
    /// Клиент с куками авторизованного пользователя.
    /// </summary>
    protected internal CookieClient Client
    {
      get { return this.client ?? (this.client = new CookieClient() {BaseAddress = MainUri.ToString()}); }
    }

    private CookieClient client;
    private bool isLogined;

    public abstract Task<bool> DoLogin();

    public virtual async Task<bool> Logout()
    {
      IsLogined = false;
      using (TimedLock.Lock(ClientLock))
      {
        await Page.GetPageAsync(LogoutUri, Client);
      }
      return true;
    }

    public async Task<List<IManga>> GetBookmarks()
    {
      if (this.CanLogin)
      {
        Log.AddFormat("Start load bookmarks from '{0}'.", this.MainUri);
        var bookmarks = await DownloadBookmarks();
        Log.AddFormat("Finish load bookmarks from '{0}'.", this.MainUri);
        return bookmarks;
      }
      return new List<IManga>();
    }

    protected abstract Task<List<IManga>> DownloadBookmarks();
    
    public static T Get<T>() where T : Login
    {
      return NHibernate.Repository.Get<T>().SingleOrDefault() ?? Activator.CreateInstance<T>();
    }

    protected Login()
    {
      this.ClientLock = new object();
    }

    protected virtual void OnLoginStateChanged(bool e)
    {
      LoginStateChanged?.Invoke(this, e);
    }
  }
}
