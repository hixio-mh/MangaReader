﻿using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MangaReader.Core.Services.Config;
using MangaReader.UI.AddNewManga;
using MangaReader.ViewModel.Commands.AddManga;
using MangaReader.ViewModel.Primitive;

namespace MangaReader.ViewModel
{
  public class AddNewModel : BaseViewModel
  {
    private string inputText;
    protected internal Window window { get { return this.view as Window; } }

    public ObservableCollection<LoginModel> Logins { get; set; }

    public ICommand Add { get; set; }

    public string InputText
    {
      get { return inputText; }
      set
      {
        inputText = value;
        OnPropertyChanged();
      }
    }

    public override void Load()
    {
      base.Load();

      foreach (var setting in ConfigStorage.Instance.DatabaseConfig.MangaSettings)
      {
        this.Logins.Add(new LoginModel(new Login(), setting));
      }
    }

    public override void Show()
    {
      base.Show();

      this.window.ShowDialog();
    }
    
    public AddNewModel(Window view) : base(view)
    {
      this.Add = new AddSelected(this);
      this.Logins = new ObservableCollection<LoginModel>();
    }
  }
}