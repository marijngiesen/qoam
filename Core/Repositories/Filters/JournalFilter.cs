﻿namespace QOAM.Core.Repositories.Filters
{
    using System.Collections.Generic;
    using System.Web.Helpers;

    public class JournalFilter
    {
        public JournalFilter()
        {
            this.SwotMatrix = new List<string>();
        }

        public string Title { get; set; }

        public string Issn { get; set; }

        public string Publisher { get; set; }

        public int? PublisherId { get; set; }

        public int? Discipline { get; set; }

        public int? Language { get; set; }

        public bool MustHaveBeenScored { get; set; }

        public JournalSortMode SortMode { get; set; }

        public SortDirection SortDirection { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public bool SubmittedOnly { get; set; }

        public IList<string> SwotMatrix { get; set; } 
    }
}