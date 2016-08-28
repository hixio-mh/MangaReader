﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MangaReader.Core;
using MangaReader.Core.Account;
using MangaReader.Core.Exception;
using MangaReader.Core.Manga;
using MangaReader.Core.Services;
using MangaReader.Core.Services.Config;

namespace Hentaichan
{
  public class Parser : BaseSiteParser
  {
    private static readonly string AdultOnly = "Доступ ограничен. Только зарегистрированные пользователи подтвердившие, что им 18 лет.";

    public static CookieClient GetClient()
    {
      var setting = ConfigStorage.GetPlugin<Hentaichan>().GetSettings();
      var client = new CookieClient();
      if (setting != null)
      {
        var login = setting.Login as HentaichanLogin;
        if (login == null || !login.CanLogin || string.IsNullOrWhiteSpace(login.UserId))
        {
          if (login == null)
          {
            login = new HentaichanLogin() {Name = setting.Login.Name, Password = setting.Login.Password};
            setting.Login = login;
          }

          login.DoLogin().Wait();
        }
        if (!string.IsNullOrWhiteSpace(login.UserId))
        {
          var host = Generic.GetLoginMainUri<Hentaichan>().Host;
          client.Cookie.Add(new Cookie("dle_user_id", login.UserId, "/", host));
          client.Cookie.Add(new Cookie("dle_password", login.PasswordHash, "/", host));
        }
      }
      return client;
    }

    public override void UpdateNameAndStatus(IManga manga)
    {
      var name = string.Empty;
      try
      {
        var document = new HtmlDocument();
        var page = Page.GetPage(manga.Uri);
        document.LoadHtml(page.Content);
        var nameNode = document.DocumentNode.SelectSingleNode("//head/title");
        string[] subString = { "Все главы", "Все части" };
        if (nameNode != null && subString.Any(s => nameNode.InnerText.Contains(s)))
        {
          name = subString
            .Aggregate(nameNode.InnerText, (current, s) => current.Replace(s, string.Empty))
            .Trim().TrimEnd('-').Trim()
            .Replace("\\'", "'");
        }
      }
      catch (NullReferenceException ex) { Log.Exception(ex); }
      name = WebUtility.HtmlDecode(name);

      this.UpdateName(manga, name);
    }

    public override void UpdateContent(IManga manga)
    {
      var chapters = new List<Chapter>();
      try
      {
        var document = new HtmlDocument();
        var pages = new List<Uri>() {manga.Uri};
        for (int i = 0; i < pages.Count; i++)
        {
          var content = Page.GetPage(pages[i], GetClient()).Content;
          if (content.Contains(AdultOnly))
            throw new GetSiteInfoException(AdultOnly, manga);
          document.LoadHtml(content);

          // Посчитать странички.
          if (i == 0)
          {
            var pageNodes = document.DocumentNode.SelectNodes("//div[@id=\"pagination_related\"]//a");
            if (pageNodes != null)
            {
              foreach (var node in pageNodes)
              {
                pages.Add(new Uri(manga.Uri + node.Attributes[0].Value));
              }
              pages = pages.Distinct().ToList();
            }
          }

          var chapterNodes = document.DocumentNode.SelectNodes("//div[@class=\"related_info\"]");
          if (chapterNodes != null)
          {
            foreach (var node in chapterNodes)
            {
              var link = node.SelectSingleNode(".//h2//a");
              var desc = node.SelectSingleNode(".//div[@class=\"related_tag_list\"]");
              chapters.Add(new Chapter(new Uri(manga.Uri, link.Attributes[0].Value), desc.InnerText));
            }
          }
        }
      }
      catch (NullReferenceException ex)
      {
        var status = "Возможно требуется регистрация";
        Library.Status = status;
        Log.Exception(ex, status, manga.Uri.OriginalString);
      }
      catch (GetSiteInfoException ex)
      {
        Library.Status = string.Format("{0}. {1}", manga.Name, AdultOnly);
        Log.Exception(ex);
      }

      manga.Chapters.AddRange(chapters);
    }

    public static void UpdatePages(MangaReader.Core.Manga.Chapter chapter)
    {
      chapter.Pages.Clear();
      var pages = new List<MangaPage>();
      try
      {
        var document = new HtmlDocument();
        document.LoadHtml(Page.GetPage(new Uri(chapter.Uri.OriginalString.Replace("/manga/", "/online/")), GetClient()).Content);

        var i = 0;
        var imgs = Regex.Match(document.DocumentNode.OuterHtml, @"""(fullimg.*)", RegexOptions.IgnoreCase).Groups[1].Value.Remove(0, 9);
        foreach (Match match in Regex.Matches(imgs, @"""(.*?)"","))
        {
          pages.Add(new MangaPage(chapter.Uri, new Uri(match.Groups[1].Value), i++));
        }
      }
      catch (NullReferenceException ex) { Log.Exception(ex); }

      chapter.Pages.AddRange(pages);
    }
  }
}
