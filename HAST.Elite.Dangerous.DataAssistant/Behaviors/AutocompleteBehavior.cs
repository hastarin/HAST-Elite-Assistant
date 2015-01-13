// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 13-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 13-01-2015
// ***********************************************************************
// Modified from a version found on Github
// https://github.com/Nimgoble/WPFTextBoxAutoComplete/blob/master/WPFTextBoxAutoComplete/AutoCompleteBehavior.cs
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>Class AutoCompleteBehavior.</summary>
    public static class AutoCompleteBehavior
    {
        #region Static Fields (Public)

        /// <summary>
        /// The items source
        /// </summary>
        public static readonly DependencyProperty AutoCompleteItemsSource =
            DependencyProperty.RegisterAttached(
                "AutoCompleteItemsSource",
                typeof(IEnumerable<String>),
                typeof(AutoCompleteBehavior),
                new UIPropertyMetadata(null, OnAutoCompleteItemsSource));

        #endregion

        #region Static Fields

        private static readonly KeyEventHandler onKeyDown = OnPreviewKeyDown;

        private static readonly TextChangedEventHandler onTextChanged = OnTextChanged;

        #endregion

        #region Public Methods and Operators

        /// <summary>Gets the automatic complete items source.</summary>
        /// <param name="obj">The object.</param>
        /// <returns>IEnumerable<String>.</returns>
        public static IEnumerable<String> GetAutoCompleteItemsSource(DependencyObject obj)
        {
            var objRtn = obj.GetValue(AutoCompleteItemsSource);
            if (objRtn is IEnumerable<String>)
            {
                return (objRtn as IEnumerable<String>);
            }

            return null;
        }

        /// <summary>Sets the automatic complete items source.</summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetAutoCompleteItemsSource(DependencyObject obj, IEnumerable<String> value)
        {
            obj.SetValue(AutoCompleteItemsSource, value);
        }

        #endregion

        #region Methods

        /// <summary>Handles the <see cref="AutoCompleteItemsSource" /> event.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="System.Windows.DependencyPropertyChangedEventArgs" /> instance containing the event data.
        /// </param>
        private static void OnAutoCompleteItemsSource(object sender, DependencyPropertyChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (sender == null || tb == null)
            {
                return;
            }

            if (e.NewValue == null)
            {
                tb.TextChanged -= onTextChanged;
                tb.PreviewKeyDown -= onKeyDown;
            }
            else
            {
                tb.TextChanged += onTextChanged;
                tb.PreviewKeyDown += onKeyDown;
            }
        }

        /// <summary>Handles the <see cref="PreviewKeyDown" /> event.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="System.Windows.Input.KeyEventArgs" /> instance containing the event data.
        /// </param>
        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            var tb = e.OriginalSource as TextBox;
            if (tb == null)
            {
                return;
            }

            //If we pressed enter and if the selected text goes all the way to the end, move our caret position to the end
            if (tb.SelectionLength > 0 && (tb.SelectionStart + tb.SelectionLength == tb.Text.Length))
            {
                tb.SelectionStart = tb.CaretIndex = tb.Text.Length;
                tb.SelectionLength = 0;
            }
        }

        /// <summary>Handles the <see cref="TextChanged" /> event.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="System.Windows.Controls.TextChangedEventArgs" /> instance containing the event data.
        /// </param>
        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if ((from change in e.Changes where change.RemovedLength > 0 select change).Any()
                && !(from change in e.Changes where change.AddedLength > 0 select change).Any())
            {
                return;
            }

            var tb = e.OriginalSource as TextBox;
            if (sender == null || tb == null)
            {
                return;
            }

            var values = GetAutoCompleteItemsSource(tb);
            //No reason to search if we don't have any values.
            if (values == null)
            {
                return;
            }

            //No reason to search if there's nothing there.
            if (String.IsNullOrEmpty(tb.Text))
            {
                return;
            }

            var textLength = tb.Text.Length;

            //Do search and changes here.
            IEnumerable<string> matches;
            var toMatch = tb.Text;
            var isCaseSensitive = false;
            if (isCaseSensitive)
            {
                matches = from value in (from subvalue in values where subvalue.Length >= textLength select subvalue)
                          where value.Substring(0, textLength) == toMatch
                          select value;
            }
            else
            {
                toMatch = tb.Text.ToUpperInvariant();
                matches = from value in (from subvalue in values where subvalue.Length >= textLength select subvalue)
                          where value.Substring(0, textLength).ToUpperInvariant() == toMatch
                          select value;
            }

            //Nothing.  Leave 'em alone
            if (!matches.Any())
            {
                return;
            }

            var match = matches.ElementAt(0);
            //String remainder = match.Substring(textLength, (match.Length - textLength));
            tb.TextChanged -= onTextChanged;
            tb.Text = match;
            tb.CaretIndex = textLength;
            tb.SelectionStart = textLength;
            tb.SelectionLength = (match.Length - textLength);
            tb.TextChanged += onTextChanged;
        }

        #endregion
    }
}