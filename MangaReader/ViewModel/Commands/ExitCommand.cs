﻿using System;
using System.Windows;
using MangaReader.Properties;
using MangaReader.Services;
using MangaReader.Services.Config;
using MangaReader.ViewModel.Commands.Primitives;

namespace MangaReader.ViewModel.Commands
{
  public class ExitCommand : BaseCommand
  {
    public static bool CommandRunned { get; set; }

    public override string Name { get { return Strings.Library_Exit; } }

    public override void Execute(object parameter)
    {
      if (!CommandRunned)
      {
        CommandRunned = true;

        Log.Add("Application will be closed.");
        WindowModel.Instance.SaveWindowState();
        Client.Close();
        Application.Current.Exit += (sender, args) => Environment.Exit(args.ApplicationExitCode);
        Application.Current.Shutdown(0);
      }
    }
  }
}