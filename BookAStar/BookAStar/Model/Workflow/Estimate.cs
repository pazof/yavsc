﻿using BookAStar.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar.Model.Workflow
{
    public partial class Estimate
    {
        public long Id { get; set; }
        public long? CommandId { get; set; }
        public string CommandType { get; set; }
        // Markdown expected
        public string Description { get; set; }
        public int? Status { get; set; }
        public string Title { get; set; }
        public List<CommandLine> Bill { get; set; }
        /// <summary>
        /// List of attached graphic files
        /// to this estimate, as relative pathes to
        /// the command performer's root path.
        /// In db, they are separated by <c>:</c>
        /// </summary>
        /// <returns></returns>
        public List<string> AttachedGraphicList { get; private set; }

        public string AttachedGraphicsString
        {
            get { return AttachedGraphicList==null?null:string.Join(":", AttachedGraphicList); }
            set { AttachedGraphicList = value.Split(':').ToList(); }
        }
        /// <summary>
        /// List of attached files
        /// to this estimate, as relative pathes to
        /// the command performer's root path.
        /// In db, they are separated by <c>:</c>
        /// </summary>
        /// <returns></returns>
        public List<string> AttachedFiles { get; set; }
        public string AttachedFilesString
        {
            get { return AttachedFiles == null ? null : string.Join(":", AttachedFiles); }
            set { AttachedFiles = value.Split(':').ToList(); }
        }
        
        public string OwnerId { get; set; }
        
        public string ClientId { get; set; }
        
        public BookQueryData Query
        {
            get
            {
                if (CommandId.HasValue)
                {
                    return DataManager.Current.BookQueries.LocalGet(CommandId.Value);
                }
                return null;
            }
        }

        public ClientProviderInfo Client
        {
            get
            {
                return DataManager.Current.Contacts.LocalGet(ClientId);
            }
        }

        public FormattedString FormattedTotal
        {
            get
            {
                OnPlatform<Font> lfs = (OnPlatform < Font >) App.Current.Resources["LargeFontSize"];
                OnPlatform<Color> etc = (OnPlatform<Color>)App.Current.Resources["EmphasisTextColor"];
                
                return new FormattedString
                {

                    Spans = {
                        new Span { Text = "Total TTC: " },
                        new Span { Text = Total.ToString(),
                            ForegroundColor = etc.Android  ,
                            FontSize = (double) lfs.Android.FontSize },
                        new Span { Text = "€", FontSize = (double) lfs.Android.FontSize }
                    }
                };
            }
            set { }
        }
        public decimal Total { get
            {
                return Bill?.Aggregate((decimal)0, (t, l) => t + l.Count * l.UnitaryCost) ?? (decimal)0;
            }
        }
    }
}
