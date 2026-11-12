using System;
using System.Collections.Generic;
using System.Windows;
using CursorlessVisualStudio.Core;
using Microsoft.VisualStudio.PlatformUI;

namespace CursorlessVisualStudio
{
    internal partial class SelectHatDialog : DialogWindow
    {
        private readonly Action<HatMark> select;

        public SelectHatDialog(Action<HatMark> select)
        {
            InitializeComponent();
            this.select = select;
            ColorInput.ItemsSource = Enum.GetValues(typeof(HatColor));
            ShapeInput.ItemsSource = Enum.GetValues(typeof(HatShape));
            ColorInput.SelectedIndex = 0;
            ShapeInput.SelectedIndex = 0;
            Loaded += (sender, args) => CharacterInput.Focus();
        }

        private void SelectClick(object sender, RoutedEventArgs args)
        {
            try
            {
                select(new HatMark((HatColor)ColorInput.SelectedItem, (HatShape)ShapeInput.SelectedItem, CharacterInput.Text));
                DialogResult = true;
            }
            catch (ArgumentException exception) { ErrorText.Text = exception.Message; }
            catch (KeyNotFoundException exception) { ErrorText.Text = exception.Message; }
            catch (InvalidOperationException exception) { ErrorText.Text = exception.Message; }
        }
    }
}
