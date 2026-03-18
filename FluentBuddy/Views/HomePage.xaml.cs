using System;
using Microsoft.Maui.Controls;

namespace FluentBuddy.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//settings");
    }

    private async void OnChatClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//chat");
    }

    private async void OnGrammarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//grammar");
    }

    private async void OnDictionaryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//dictionary");
    }

    private async void OnQuizClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//quiz");
    }

    private async void OnCardPointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Grid grid && grid.Parent is Border border)
        {
            await border.ScaleTo(1.03, 120, Easing.CubicOut);
        }
    }

    private async void OnCardPointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Grid grid && grid.Parent is Border border)
        {
            await border.ScaleTo(1.0, 120, Easing.CubicIn);
        }
    }
}