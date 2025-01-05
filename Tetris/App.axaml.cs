using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Tetris.Model;
using Tetris.Model.Persistence;
using Tetris.ViewModels;
using Tetris.Views;

namespace Tetris;

public class App : Application
{
    private GameState _gameState = null!;
    private MainViewModel _viewModel = null!;

    private TopLevel? TopLevel
    {
        get
        {
            return ApplicationLifetime switch
            {
                IClassicDesktopStyleApplicationLifetime desktop => TopLevel.GetTopLevel(desktop.MainWindow),
                ISingleViewApplicationLifetime singleViewPlatform => TopLevel.GetTopLevel(singleViewPlatform.MainView),
                _ => null
            };
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        _viewModel = new MainViewModel();
        _viewModel.LoadGame += ViewModel_LoadGame;
        _viewModel.SaveGame += ViewModel_SaveGame;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _viewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new AndroidView
            {
                DataContext = _viewModel
            };

            if (Current?.TryGetFeature<IActivatableLifetime>() is { } activatableLifetime)
            {
                activatableLifetime.Activated += (sender, args) =>
                {
                    if (args.Kind == ActivationKind.Background)
                    {
                        if (_viewModel.GetState)
                        {
                            _viewModel.BackgroundPause(true);
                        }
                    }
                };
                activatableLifetime.Deactivated += (sender, args) =>
                {
                    if (args.Kind == ActivationKind.Background)
                    {
                        if (_viewModel.GetState)
                        {
                            _viewModel.BackgroundPause(false);
                        }
                    }
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void ViewModel_SaveGame(object? sender, GameState gameState)
    {
        if (TopLevel == null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Tetris save",
                    "Saving is not supported",
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
            return;
        }

        try
        {
            var file = await TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save game state",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Text file")
                    {
                        Patterns = new[] { "*.txt" }
                    }
                }
            });

            if (file != null)
                using (var stream = await file.OpenWriteAsync())
                {
                    gameState.SaveGame(stream);
                }
        }
        catch (Exception e)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Tetris saving",
                    "Failed to save file!" + e.Message,
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
        }
    }

    private async void ViewModel_LoadGame(object? sender, EventArgs e)
    {
        if (TopLevel == null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Tetris save",
                    "Saving is not supported",
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
            return;
        }

        try
        {
            var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Save game state",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Text file")
                    {
                        Patterns = new[] { "*.txt" }
                    }
                }
            });

            if (files.Count > 0)
                using (var stream = await files[0].OpenReadAsync())
                {
                    _gameState = new GameState(new TetrisDataAccess(), stream);
                    _viewModel.LoadSave(_gameState);
                    await _viewModel.InitializeGameAsync();
                }
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "Tetris saving",
                    "Failed to load file!" + ex.Message,
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
        }
    }
}