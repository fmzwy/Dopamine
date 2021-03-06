﻿using Dopamine.Common.Presentation.Interfaces;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Database;
using Dopamine.Common.Database.Entities;
using Prism.Mvvm;

namespace Dopamine.Common.Presentation.ViewModels.Entities
{
    public class ArtistViewModel : BindableBase, ISemanticZoomable
    {
        private Artist artist;
        private bool isHeader;
        public Artist Artist
        {
            get { return this.artist; }
            set { SetProperty<Artist>(ref this.artist, value); }
        }

        public string ArtistName
        {
            get { return this.Artist.ArtistName; }
            set
            {
                this.Artist.ArtistName = value;
                RaisePropertyChanged(nameof(this.ArtistName));
            }
        }

        public string SortArtistName
        {
            get { return DatabaseUtils.GetSortableString(this.ArtistName, true); }
        }

        public string Header
        {
            get { return SemanticZoomUtils.GetGroupHeader(this.Artist.ArtistName, true); }
        }

        public bool IsHeader
        {
            get { return this.isHeader; }
            set { SetProperty<bool>(ref this.isHeader, value); }
        }
       
        public override string ToString()
        {
            return this.ArtistName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Artist.Equals(((ArtistViewModel)obj).Artist);
        }

        public override int GetHashCode()
        {
            return this.Artist.GetHashCode();
        }
    }
}
