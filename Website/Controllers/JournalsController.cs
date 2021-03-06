﻿namespace QOAM.Website.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using AttributeRouting;
    using AttributeRouting.Web.Mvc;

    using QOAM.Core;
    using QOAM.Core.Repositories;
    using QOAM.Core.Repositories.Filters;
    using QOAM.Website.Helpers;
    using QOAM.Website.Models;
    using QOAM.Website.ViewModels.Journals;

    using Validation;

    [RoutePrefix("journals")]
    public class JournalsController : ApplicationController
    {
        private readonly IJournalRepository journalRepository;
        private readonly IBaseJournalPriceRepository baseJournalPriceRepository;
        private readonly ILanguageRepository languageRepository;
        private readonly ISubjectRepository subjectRepository;
        private readonly IInstitutionJournalRepository institutionJournalRepository;
        private readonly IBaseScoreCardRepository baseScoreCardRepository;
        private readonly IValuationScoreCardRepository valuationScoreCardRepository;
        private readonly IValuationJournalPriceRepository valuationJournalPriceRepository;
        private readonly IInstitutionRepository institutionRepository;

        public JournalsController(IJournalRepository journalRepository, IBaseJournalPriceRepository baseJournalPriceRepository, IValuationJournalPriceRepository valuationJournalPriceRepository, IValuationScoreCardRepository valuationScoreCardRepository, ILanguageRepository languageRepository, ISubjectRepository subjectRepository, IInstitutionJournalRepository institutionJournalRepository, IBaseScoreCardRepository baseScoreCardRepository, IUserProfileRepository userProfileRepository, IAuthentication authentication, IInstitutionRepository institutionRepository)
            : base(userProfileRepository, authentication)
        {
            Requires.NotNull(journalRepository, "journalRepository");
            Requires.NotNull(baseJournalPriceRepository, "baseJournalPriceRepository");
            Requires.NotNull(valuationJournalPriceRepository, "valuationJournalPriceRepository");
            Requires.NotNull(valuationScoreCardRepository, "valuationScoreCardRepository");
            Requires.NotNull(languageRepository, "languageRepository");
            Requires.NotNull(subjectRepository, "keywordRepository");
            Requires.NotNull(institutionJournalRepository, "institutionJournalRepository");
            Requires.NotNull(baseScoreCardRepository, "scoreCardRepository");
            Requires.NotNull(institutionRepository, "institutionRepository");
            Requires.NotNull(valuationJournalPriceRepository, "valuationJournalPriceRepository");
            
            this.journalRepository = journalRepository;
            this.baseJournalPriceRepository = baseJournalPriceRepository;
            this.languageRepository = languageRepository;
            this.subjectRepository = subjectRepository;
            this.institutionJournalRepository = institutionJournalRepository;
            this.baseScoreCardRepository = baseScoreCardRepository;
            this.valuationScoreCardRepository = valuationScoreCardRepository;
            this.institutionRepository = institutionRepository;
            this.valuationJournalPriceRepository = valuationJournalPriceRepository;
        }
        
        [GET("")]
        public ViewResult Index(IndexViewModel model)
        {
            model.Languages = this.languageRepository.All.ToSelectListItems("<All languages>");
            model.Disciplines = this.subjectRepository.All.ToSelectListItems("<All disciplines>");
            model.Journals = this.journalRepository.Search(model.ToFilter());
            
            return this.View(model);
        }

        [GET("{id:int}")]
        public ActionResult Details(int id, string returnUrl)
        {
            var journal = this.journalRepository.Find(id);
            
            if (journal == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.ReturnUrl = returnUrl;

            return this.View(journal);
        }

        [GET("{id:int}/prices")]
        public PartialViewResult Prices(PricesViewModel model)
        {
            this.ViewBag.RefererUrl = model.RefererUrl;

            model.InstitutionJournals = this.institutionJournalRepository.Find(model.ToInstitutionJournalPriceFilter());
            model.BaseJournalPrices = this.baseJournalPriceRepository.Find(model.ToJournalPriceFilter(null));
            model.ValuationJournalPrices = this.valuationJournalPriceRepository.Find(model.ToJournalPriceFilter(FeeType.Article));
            model.Journal = this.journalRepository.Find(model.Id);

            return this.PartialView(model);
        }

        [GET("{id:int}/basejournalprices")]
        public PartialViewResult BaseJournalPrices(PricesViewModel model)
        {
            this.ViewBag.RefererUrl = model.RefererUrl;

            var journalPrices = this.baseJournalPriceRepository.Find(model.ToJournalPriceFilter(null));

            return this.PartialView(journalPrices);
        }

        [GET("{id:int}/valuationjournalprices")]
        public PartialViewResult ValuationJournalPrices(PricesViewModel model)
        {
            this.ViewBag.RefererUrl = model.RefererUrl;

            var journalPrices = this.valuationJournalPriceRepository.Find(model.ToJournalPriceFilter(FeeType.Article));

            return this.PartialView(journalPrices);
        }

        [GET("{id:int}/institutionalprices")]
        public PartialViewResult InstitutionJournalPrices(PricesViewModel model)
        {
            this.ViewBag.RefererUrl = model.RefererUrl;

            var institutionJournals = this.institutionJournalRepository.Find(model.ToInstitutionJournalPriceFilter());

            return this.PartialView(institutionJournals);
        }

        [GET("{id:int}/scorecards")]
        public PartialViewResult ScoreCards(ScoreCardsViewModel model)
        {
            model.BaseScoreCards = this.baseScoreCardRepository.Find(model.ToFilter());
            model.ValuationScoreCards = this.valuationScoreCardRepository.Find(model.ToFilter());

            return this.PartialView(model);
        }

        [GET("{id:int}/basescorecards")]
        public PartialViewResult BaseScoreCards(ScoreCardsViewModel model)
        {
            model.BaseScoreCards = this.baseScoreCardRepository.Find(model.ToFilter());

            return this.PartialView(model);
        }

        [GET("{id:int}/valuationscorecards")]
        public PartialViewResult ValuationScoreCards(ScoreCardsViewModel model)
        {
            model.ValuationScoreCards = this.valuationScoreCardRepository.Find(model.ToFilter());

            return this.PartialView(model);
        }

        [GET("{id:int}/comments")]
        public PartialViewResult Comments(CommentsViewModel model)
        {
            model.CommentedBaseScoreCards = this.baseScoreCardRepository.Find(model.ToFilter());
            model.CommentedValuationScoreCards = this.valuationScoreCardRepository.Find(model.ToFilter());

            return this.PartialView(model);
        }

        [GET("{id:int}/basescorecardcomments")]
        public PartialViewResult BaseScoreCardComments(CommentsViewModel model)
        {
            model.CommentedBaseScoreCards = this.baseScoreCardRepository.Find(model.ToFilter());

            return this.PartialView(model);
        }

        [GET("{id:int}/valuationscorecardcomments")]
        public PartialViewResult ValuationScoreCardComments(CommentsViewModel model)
        {
            model.CommentedValuationScoreCards = this.valuationScoreCardRepository.Find(model.ToFilter());

            return this.PartialView(model);
        }

        [GET("{id:int}/institutionjournallicense")]
        [Authorize(Roles = ApplicationRole.InstitutionAdmin + "," + ApplicationRole.Admin)]
        public ViewResult InstitutionJournalLicense(int id, int institutionId, string refererUrl)
        {
            var model = new InstitutionJournalLicenseViewModel(
                this.journalRepository.Find(id),
                this.institutionJournalRepository.Find(id, institutionId), 
                refererUrl,
                this.institutionRepository.All.ToSelectListItems("<Select institution>"));
            
            return this.View(model);
        }

        [POST("{id:int}/institutionjournallicense")]
        [Authorize(Roles = ApplicationRole.InstitutionAdmin + "," + ApplicationRole.Admin)]
        [ValidateAntiForgeryToken]
        public ActionResult InstitutionJournalLicense(int id, InstitutionJournalLicenseViewModel model)
        {
            // Ensure that only admin and institutional admin users can use the update all journals of a publisher
            if (model.UpdateAllJournalsOfPublisher && (!this.User.IsInRole(ApplicationRole.Admin) && !this.User.IsInRole(ApplicationRole.InstitutionAdmin)))
            {
                return new HttpUnauthorizedResult();
            }

            var journal = this.journalRepository.Find(id);
            
            if (this.ModelState.IsValid)
            {
                var institutionJournalsToModify = new List<InstitutionJournal>();

                if (model.UpdateAllJournalsOfPublisher)
                {
                    var institutionJournals = this.institutionJournalRepository.FindAll(new InstitutionJournalFilter
                                                                                            {
                                                                                                InstitutionId = model.Institution,
                                                                                                PublisherId = journal.PublisherId
                                                                                            });

                    var publisherJournals = this.journalRepository.SearchAll(new JournalFilter { PublisherId = journal.PublisherId });

                    foreach (var publisherJournal in publisherJournals)
                    {
                        var institutionJournal = institutionJournals.FirstOrDefault(i => i.JournalId == publisherJournal.Id) ?? new InstitutionJournal();
                        institutionJournal.DateAdded = DateTime.Now;
                        institutionJournal.Link = model.Link;
                        institutionJournal.JournalId = publisherJournal.Id;
                        institutionJournal.UserProfileId = this.Authentication.CurrentUserId;
                        institutionJournal.InstitutionId = model.Institution;

                        institutionJournalsToModify.Add(institutionJournal);
                    }
                }
                else
                {
                    var institutionJournal = this.institutionJournalRepository.Find(id, model.Institution) ?? new InstitutionJournal();
                    institutionJournal.DateAdded = DateTime.Now;
                    institutionJournal.Link = model.Link;
                    institutionJournal.JournalId = journal.Id;
                    institutionJournal.UserProfileId = this.Authentication.CurrentUserId;
                    institutionJournal.InstitutionId = model.Institution;

                    institutionJournalsToModify.Add(institutionJournal);
                }

                foreach (var institutionJournal in institutionJournalsToModify)
                {
                    this.institutionJournalRepository.InsertOrUpdate(institutionJournal);
                }

                this.institutionJournalRepository.Save();

                return this.Redirect(model.RefererUrl);
            }

            model.JournalTitle = journal.Title;
            model.JournalLink = journal.Link;
            model.JournalPublisher = journal.Publisher.Name;

            return this.View(model);
        }

        [POST("{id:int}/institutionjournallicensedelete")]
        [Authorize(Roles = ApplicationRole.InstitutionAdmin + "," + ApplicationRole.Admin)]
        [ValidateAntiForgeryToken]
        public ActionResult InstitutionJournalLicenseDelete(int id, InstitutionJournalLicenseDeleteViewModel model)
        {
            if (!this.User.IsInRole(ApplicationRole.Admin) && !this.User.IsInRole(ApplicationRole.InstitutionAdmin))
            {
                return new HttpUnauthorizedResult();
            }

            if (this.ModelState.IsValid)
            {
                var institutionJournal = this.institutionJournalRepository.Find(id, model.Institution);
                if (institutionJournal == null)
                {
                    return new HttpNotFoundResult();
                }

                this.institutionJournalRepository.Delete(institutionJournal);
                this.institutionJournalRepository.Save();

                return this.Redirect(model.RefererUrl);
            }

            return this.RedirectToAction("InstitutionJournalLicense", new { id, InstitutionId = model.Institution, model.RefererUrl });
        }

        [GET("titles")]
        [OutputCache(CacheProfile = CacheProfile.OneQuarter)]
        public JsonResult Titles(string query)
        {
            return this.Json(this.journalRepository.Titles(query).Select(s => new { value = s }).Take(AutoCompleteItemsCount).ToList(), JsonRequestBehavior.AllowGet);
        }

        [GET("issns")]
        [OutputCache(CacheProfile = CacheProfile.OneQuarter)]
        public JsonResult Issns(string query)
        {
            return this.Json(this.journalRepository.Issns(query).Select(s => new { value = s }).Take(AutoCompleteItemsCount).ToList(), JsonRequestBehavior.AllowGet);
        }

        [GET("publishers")]
        [OutputCache(CacheProfile = CacheProfile.OneQuarter)]
        public JsonResult Publishers(string query)
        {
            return this.Json(this.journalRepository.Publishers(query).Select(s => new { value = s }).Take(AutoCompleteItemsCount).ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}