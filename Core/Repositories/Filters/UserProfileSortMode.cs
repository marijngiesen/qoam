﻿namespace QOAM.Core.Repositories.Filters
{
    using System.ComponentModel.DataAnnotations;

    public enum UserProfileSortMode
    {
        [Display(Name = "UserProfileSortMode_Name", ResourceType = typeof(Strings))]
        Name,

        [Display(Name = "UserProfileSortMode_DateRegistered", ResourceType = typeof(Strings))]
        DateRegistered,
        
        [Display(Name = "UserProfileSortMode_Institution", ResourceType = typeof(Strings))]
        Institution,

        [Display(Name = "UserProfileSortMode_NumberOfBaseJournalScoreCards", ResourceType = typeof(Strings))]
        NumberOfBaseJournalScoreCards,

        [Display(Name = "UserProfileSortMode_NumberOfValuationJournalScoreCards", ResourceType = typeof(Strings))]
        NumberOfValuationJournalScoreCards
    }
}