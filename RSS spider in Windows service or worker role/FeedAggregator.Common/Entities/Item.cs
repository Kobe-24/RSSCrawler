﻿using System;
using System.Runtime.Serialization;

namespace FeedAggregator.Common.Entities
{
    [Serializable]
    [DataContract(Name = "Item")]
    public class Item
    {        
        [DataMember(Name = "Id")]
        public string Id { get; set; }
        [DataMember(Name = "Title")]
        public string Title { get; set; }
        [DataMember(Name = "Summary")]
        public string Summary { get; set; }
        [DataMember(Name = "Source")]
        public string Source { get; set; }
        [DataMember(Name = "PublishDate")]
        public DateTime PublishDate { get; set; }
        [DataMember(Name = "Categories")]
        public string[] Categories { get; set; }
        [DataMember(Name = "Links")]
        public Link[] Links { get; set; }

        public string GetShortenendSummary()
        {
            int summaryLength = 400;

            var noHTMLSummary = HtmlRemoval.StripTagsCharArray(Summary);

            if (noHTMLSummary.Length > summaryLength)
            {
                var shortenedSummary = noHTMLSummary.Substring(0, summaryLength + 1);

                var lastSpacePosition = shortenedSummary.LastIndexOf(" ");

                if (lastSpacePosition != -1)
                {
                    return shortenedSummary.Substring(0, lastSpacePosition) + " ...";
                }
            }
            return noHTMLSummary;
        }
    }

    [Serializable]
    [DataContract(Name = "Link")]
    public class Link
    {
        [DataMember(Name = "Url")]
        public Uri Url { get; set; }
        [DataMember(Name = "ContentType")]
        public string ContentType { get; set; }
    }

    [Serializable]
    [DataContract(Name = "ItemWithScore")]
    public class ItemWithScore
    {
        [DataMember(Name = "Score")]
        public double Score { get; set; }        

        [DataMember(Name = "Item")]
        public Item Item { get; set; }

    }
}
