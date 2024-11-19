using CommonResources;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CommonResources
{
    public class ButtonsBar : StackPanel
    {
        List<FrameworkElement> _otherButtons = new List<FrameworkElement>();

        protected override Size MeasureOverride(Size constraint)
        {

            _otherButtons.Clear();
            Button otherButton = null;
            double width = 0;

            foreach (FrameworkElement child in InternalChildren)
            {
                //if (child.Name == "OtherButton")
                if (child.Name.StartsWith("OtherButton"))
                    otherButton = child as Button;
            }

            otherButton.Click += OtherButton_Click;


            foreach (FrameworkElement child in InternalChildren)
            {
                child.Measure(constraint);

                bool desideredVisible = false;
                double desideredWidth = 0;
                if (child is ButtonsBarButton)
                {
                    ButtonsBarButton childButton = child as ButtonsBarButton;
                    desideredVisible = childButton.DesideredVisibility == Visibility.Visible;
                    if (childButton.DesideredWidth == null || childButton.DesideredWidth == 0)
                        childButton.DesideredWidth = childButton.DesiredSize.Width;

                    desideredWidth = childButton.DesideredWidth.Value;
                }

                if (child is ButtonsBarToggleButton)
                {
                    ButtonsBarToggleButton childToggleButton = child as ButtonsBarToggleButton;
                    desideredVisible = childToggleButton.DesideredVisibility == Visibility.Visible;
                    if (childToggleButton.DesideredWidth == null || childToggleButton.DesideredWidth == 0)
                        childToggleButton.DesideredWidth = childToggleButton.DesiredSize.Width;

                    desideredWidth = childToggleButton.DesideredWidth.Value;
                }

                if (child is ButtonsBarDropDownButton)
                {
                    ButtonsBarDropDownButton childDDButton = child as ButtonsBarDropDownButton;
                    desideredVisible = childDDButton.DesideredVisibility == Visibility.Visible;
                    if (childDDButton.DesideredWidth == null || childDDButton.DesideredWidth == 0)
                        childDDButton.DesideredWidth = childDDButton.DesiredSize.Width;

                    desideredWidth = childDDButton.DesideredWidth.Value;
                }

                if (!desideredVisible)
                {
                    child.Visibility = Visibility.Collapsed;
                    continue;
                }

                width += desideredWidth;

                if (width < constraint.Width - otherButton.Width)
                    child.Visibility = Visibility.Visible;
                else if (child.Name != "OtherButton")
                {
                    _otherButtons.Add(child);
                    child.Visibility = Visibility.Collapsed;
                }

            }


            if (_otherButtons.Any())
            {
                ContextMenu contextMenu = UpdateOtherButtonMenu(otherButton);

                otherButton.ContextMenu = contextMenu;
            }
            else
            {
                otherButton.Visibility = Visibility.Hidden;
            }
            return base.MeasureOverride(constraint);
        }

        private ContextMenu UpdateOtherButtonMenu(Button otherButton)
        {
            otherButton.Visibility = Visibility.Visible;
            ContextMenu contextMenu = new ContextMenu();

            foreach (FrameworkElement el in _otherButtons)
            {
                if (el != null)
                {
                    MenuItem menuItem = new MenuItem();

                    string otherText = "";


                    if (el is ButtonsBarButton)
                    {
                        ButtonsBarButton button = el as ButtonsBarButton;
                        otherText = button.OtherText;

                        menuItem.Header = otherText;
                        menuItem.Tag = el;
                        menuItem.Icon = CreateIcon(button.Content);
                        menuItem.Click += MenuItem_Click;
                        menuItem.IsEnabled = el.IsEnabled;


                        contextMenu.Items.Add(menuItem);
                    }
                    else if (el is ButtonsBarToggleButton)
                    {
                        ButtonsBarToggleButton toggleButton = el as ButtonsBarToggleButton;

                        otherText = toggleButton.OtherText;

                        menuItem.IsCheckable = true;

                        if (toggleButton.IsChecked.HasValue)
                            menuItem.IsChecked = toggleButton.IsChecked.Value;

                        menuItem.Header = otherText;
                        menuItem.Tag = el;
                        menuItem.Icon = CreateIcon(toggleButton.Content);
                        menuItem.Click += MenuItem_Click;
                        menuItem.IsEnabled = el.IsEnabled;

                        contextMenu.Items.Add(menuItem);

                    }

                    if (el is ButtonsBarDropDownButton)
                    {
                        ButtonsBarDropDownButton dropDownButton = (el as ButtonsBarDropDownButton);

                        otherText = dropDownButton.OtherText;

                        DependencyObject depObj = dropDownButton.DropDownContent as DependencyObject;
                        //DependencyObject depObj = dropDownButton.PopupContent as DependencyObject;
                        if (depObj != null)
                        {
                            //sub button
                            List<Button> buttons = new List<Button>();
                            depObj.GetChildOfType<Button>(buttons);

                            foreach (Button btn in buttons)
                            {
                                MenuItem subMenuItem = new MenuItem();
                                subMenuItem.Header = btn.ToolTip;
                                subMenuItem.Tag = btn;
                                subMenuItem.Icon = CreateIcon(btn.Content);
                                subMenuItem.Click += MenuItem_Click;
                                subMenuItem.IsEnabled = btn.IsEnabled;

                                menuItem.Items.Add(subMenuItem);
                            }

                            //sub togglebutton
                            List<ToggleButton> toggleButtons = new List<ToggleButton>();
                            depObj.GetChildOfType<ToggleButton>(toggleButtons);

                            foreach (ToggleButton btn in toggleButtons)
                            {
                                if (btn.Visibility != Visibility.Visible)
                                    continue;

                                MenuItem subMenuItem = new MenuItem();
                                subMenuItem.Header = btn.ToolTip;
                                subMenuItem.Tag = btn;
                                subMenuItem.Icon = CreateIcon(btn.Content);
                                subMenuItem.Click += MenuItem_Click;
                                subMenuItem.IsEnabled = btn.IsEnabled;
                                subMenuItem.IsCheckable = true;

                                if (btn.IsChecked.HasValue)
                                    subMenuItem.IsChecked = btn.IsChecked.Value;

                                menuItem.Items.Add(subMenuItem);
                            }
                        }

                        menuItem.Icon = CreateIcon(dropDownButton.Content);
                        menuItem.Header = otherText;
                        contextMenu.Items.Add(menuItem);

                    }
                }
            }

            return contextMenu;
        }

        TextBlock CreateIcon(object content)
        {
            if (content is string)
            {
                return CreateIcon(content as string);
            }
            else if (content is Grid)
            {
                List<TextBlock> textBlocks = new List<TextBlock>();
                (content as Grid).GetChildOfType(textBlocks);
                TextBlock firstTB = textBlocks.FirstOrDefault();
                if (firstTB != null)
                {
                    return CreateIcon(firstTB.Text as string);
                }

            }


            return new TextBlock();
        }

        TextBlock CreateIcon(string content)
        {
            TextBlock icon = new TextBlock();
            icon.Style = Application.Current.Resources["menuTextBlockStyle"] as Style;
            icon.Text = content as string;
            icon.FontSize = 12;
            icon.TextAlignment = TextAlignment.Left;
            icon.VerticalAlignment = VerticalAlignment.Center;

            return icon;

        }

        private void OtherButton_Click(object sender, RoutedEventArgs e)
        {
            //Update other menu items
            Button otherButton = sender as Button;
            if (otherButton.ContextMenu != null)
            {
                foreach (MenuItem menuItem in otherButton.ContextMenu.Items)
                {
                    if (menuItem.Tag is ButtonsBarToggleButton)
                    {
                        ButtonsBarToggleButton btn = menuItem.Tag as ButtonsBarToggleButton;
                        if (btn.IsChecked == true)
                            menuItem.IsChecked = true;
                        else
                            menuItem.IsChecked = false;
                    }
                    
                }
            }

            otherButton.ContextMenu.IsOpen = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            FrameworkElement fwElement = menuItem.Tag as FrameworkElement;

            if (fwElement is Button)
            {
                Button button = fwElement as Button;
                ButtonAutomationPeer peer = new ButtonAutomationPeer(button);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
            }
            else if (fwElement is ToggleButton)
            {
                ToggleButton button = fwElement as ToggleButton;
                ToggleButtonAutomationPeer peer = new ToggleButtonAutomationPeer(button);
                IToggleProvider invokeProv = peer.GetPattern(PatternInterface.Toggle) as IToggleProvider;
                invokeProv.Toggle();

                //Aggiorno i check sui fratelli.
                //N.B. Funziona solo quando la logica è che solo uno tra i fratelli sia checked. (tipo radio button)
                var parent = menuItem.GetParentOfType<StackPanel>();
                foreach (MenuItem subMenuItem in parent.Children)
                {
                    if (subMenuItem != menuItem)
                        subMenuItem.IsChecked = false;

                }


            }
            else if (fwElement is DropDownButton)
            {
                DropDownButton ddButton = fwElement as DropDownButton;


            }
        }

    }

    public class FwElementData
    {
        public bool IsOtherButton { get; set; } = false;//false: in other button
        public bool DesideredVisible { get; set; } = false;
        public double DesideredWidth {get;set; } = 0;
    }

    public class ButtonsBarButton : Button
    {
        public double? DesideredWidth
        {
            get;
            set;
        } = null;

        /// <summary>
        /// Registers a dependency property as backing store for the Content property
        /// </summary>
        public static readonly DependencyProperty DesideredVisibilityProperty =
            DependencyProperty.Register("DesideredVisibility", typeof(Visibility), typeof(ButtonsBarButton),
            new FrameworkPropertyMetadata(Visibility.Visible,
                  FrameworkPropertyMetadataOptions.AffectsRender |
                  FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>The Content.</value>
        public Visibility DesideredVisibility
        {
            get { return (Visibility)GetValue(DesideredVisibilityProperty); }
            set { SetValue(DesideredVisibilityProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property as backing store for the Content property
        /// </summary>
        public static readonly DependencyProperty OtherTextProperty =
            DependencyProperty.Register("OtherText", typeof(string), typeof(ButtonsBarButton),
            new FrameworkPropertyMetadata("",
                  FrameworkPropertyMetadataOptions.AffectsRender |
                  FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>The Content.</value>
        public string OtherText
        {
            get { return (string)GetValue(OtherTextProperty); }
            set { SetValue(OtherTextProperty, value); }
        }


    }

    public class ButtonsBarToggleButton : ToggleButton
    {
        public double? DesideredWidth { get; set; } = null;

        /// <summary>
        /// Registers a dependency property as backing store for the Content property
        /// </summary>
        public static readonly DependencyProperty DesideredVisibilityProperty =
            DependencyProperty.Register("DesideredVisibility", typeof(Visibility), typeof(ButtonsBarToggleButton),
            new FrameworkPropertyMetadata(Visibility.Visible,
                  FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>The Content.</value>
        public Visibility DesideredVisibility
        {
            get { return (Visibility)GetValue(DesideredVisibilityProperty); }
            set { SetValue(DesideredVisibilityProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property as backing store for the Content property
        /// </summary>
        public static readonly DependencyProperty OtherTextProperty =
            DependencyProperty.Register("OtherText", typeof(string), typeof(ButtonsBarToggleButton),
            new FrameworkPropertyMetadata("",
                  FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>The Content.</value>
        public string OtherText
        {
            get { return (string)GetValue(OtherTextProperty); }
            set { SetValue(OtherTextProperty, value); }
        }


    }

    public class ButtonsBarDropDownButton : MenuDropDownButton
    {
        public double? DesideredWidth { get; set; } = null;

        /// <summary>
        /// Registers a dependency property as backing store for the Content property
        /// </summary>
        public static readonly DependencyProperty DesideredVisibilityProperty =
            DependencyProperty.Register("DesideredVisibility", typeof(Visibility), typeof(ButtonsBarDropDownButton),
            new FrameworkPropertyMetadata(Visibility.Visible,
                  FrameworkPropertyMetadataOptions.AffectsRender |
                  FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>The Content.</value>
        public Visibility DesideredVisibility
        {
            get { return (Visibility)GetValue(DesideredVisibilityProperty); }
            set { SetValue(DesideredVisibilityProperty, value); }
        }

        /// <summary>
        /// Registers a dependency property as backing store for the Content property
        /// </summary>
        public static readonly DependencyProperty OtherTextProperty =
            DependencyProperty.Register("OtherText", typeof(string), typeof(ButtonsBarDropDownButton),
            new FrameworkPropertyMetadata("",
                  FrameworkPropertyMetadataOptions.AffectsRender |
                  FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>The Content.</value>
        public string OtherText
        {
            get { return (string)GetValue(OtherTextProperty); }
            set { SetValue(OtherTextProperty, value); }
        }

    }




}
