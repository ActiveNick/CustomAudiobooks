﻿//==========================================================================
//
// Author:  Nick Landry
// Title:   Senior Technical Evangelist - Microsoft US DX - NY Metro
// Twitter: @ActiveNick
// Blog:    www.AgeofMobility.com
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Disclaimer: Portions of this code may been simplified to demonstrate
// useful application development techniques and enhance readability.
// As such they may not necessarily reflect best practices in enterprise 
// development, and/or may not include all required safeguards.
// 
// This code and information are provided "as is" without warranty of any
// kind, either expressed or implied, including but not limited to the
// implied warranties of merchantability and/or fitness for a particular
// purpose.
//
// To learn more about Universal Windows app development using Cortana
// and the Speech SDK, watch the full-day course for free on
// Microsoft Virtual Acdemy (MVA) at http://aka.ms/cortanamva
//
//==========================================================================
using CustomAudiobooks.Data;
using CustomAudiobooks.Common;
using CustomAudiobooks.Helper;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace CustomAudiobooks
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        AudiobookItem item;

        SystemMediaTransportControls systemMediaControls = null;

        public ItemPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
        }

        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            item = await AudiobookDataSource.GetItemAsync((string)e.NavigationParameter);
            this.DefaultViewModel["Item"] = item;
        }

        void InitializeTransportControls()
        {
            // Handle events from system transport contrtols when playing background-capable media
            systemMediaControls = SystemMediaTransportControls.GetForCurrentView();
            systemMediaControls.ButtonPressed += SystemMediaControls_ButtonPressed;
            systemMediaControls.IsPlayEnabled = true;
            systemMediaControls.IsPauseEnabled = true;
            systemMediaControls.IsStopEnabled = true;

            //rootPage.SystemMediaTransportControlsInitialized = true;

            //AudiobookMediaPlayer.CurrentStateChanged += MediaElement_CurrentStateChanged;
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitializeTransportControls();

            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (systemMediaControls != null)
            {
                systemMediaControls.ButtonPressed -= SystemMediaControls_ButtonPressed;
                systemMediaControls.IsPlayEnabled = false;
                systemMediaControls.IsPauseEnabled = false;
                systemMediaControls.IsStopEnabled = false;
                systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                systemMediaControls.DisplayUpdater.ClearAll();
                systemMediaControls.DisplayUpdater.Update();
                systemMediaControls = null;
            }

            AudiobookMediaPlayer.Source = null;

            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        /// <summary>
        /// Handler for the system transport controls button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SystemMediaControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs e)
        {
            switch (e.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        AudiobookMediaPlayer.Play();
                    });
                    break;

                case SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        AudiobookMediaPlayer.Pause();
                    });
                    break;

                case SystemMediaTransportControlsButton.Stop:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        AudiobookMediaPlayer.Stop();
                    });
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Handler for the media element state change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            SystemMediaTransportControls systemMediaControls = SystemMediaTransportControls.GetForCurrentView();
            switch (AudiobookMediaPlayer.CurrentState)
            {
                default:
                case MediaElementState.Closed:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;

                case MediaElementState.Opening:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;

                case MediaElementState.Buffering:
                case MediaElementState.Playing:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;

                case MediaElementState.Paused:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;

                case MediaElementState.Stopped:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (AudiobookMediaPlayer.CurrentState == MediaElementState.Paused)
            {
                AudiobookMediaPlayer.Play();
            }
            else
            {
                string fulltext = item.Title + " \n\n" +
                                  item.Subtitle + " \n\n" +
                                  item.Description + " \n\n" +
                                  item.Content;

                SpeechHelper.ReadText(fulltext, AudiobookMediaPlayer);
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            AudiobookMediaPlayer.Pause();
        }
    }
}