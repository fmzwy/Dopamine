﻿using Dopamine.Common.Services.Playback;
using Prism.Commands;
using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PlayAllControlViewModel : BindableBase
    {
        private IPlaybackService playbackService;
       
        public DelegateCommand PlayAllCommand { get; set; }
     
        public PlayAllControlViewModel(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.PlayAllCommand = new DelegateCommand(() => this.playbackService.EnqueueAsync(false,true));
        }
    }
}
