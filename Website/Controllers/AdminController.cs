﻿namespace RU.Uci.OAMarket.Website.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using AttributeRouting;
    using AttributeRouting.Web.Mvc;

    using PagedList;

    using RU.Uci.OAMarket.Domain;
    using RU.Uci.OAMarket.Domain.Helpers;
    using RU.Uci.OAMarket.Domain.Import;
    using RU.Uci.OAMarket.Domain.Repositories;
    using RU.Uci.OAMarket.Website.Helpers;
    using RU.Uci.OAMarket.Website.Models;
    using RU.Uci.OAMarket.Website.ViewModels.Import;

    using Validation;

    [RoutePrefix("admin")]
    [Authorize(Roles = ApplicationRole.DataAdmin + "," + ApplicationRole.InstitutionAdmin)]
    public class AdminController : ApplicationController
    {
        private const string FoundISSNsSessionKey = "FoundISSNs";
        private const string NotFoundISSNsSessionKey = "NotFoundISSNs";

        private readonly JournalsImport journalsImport;
        private readonly UlrichsImport ulrichsImport;
        private readonly DoajImport doajImport;
        private readonly IJournalRepository journalRepository;

        public AdminController(JournalsImport journalsImport, UlrichsImport ulrichsImport, DoajImport doajImport, IJournalRepository journalRepository, IUserProfileRepository userProfileRepository, IAuthentication authentication)
            : base(userProfileRepository, authentication)
        {
            Requires.NotNull(journalsImport, "journalsImport");
            Requires.NotNull(ulrichsImport, "ulrichsImport");
            Requires.NotNull(doajImport, "doajImport");
            Requires.NotNull(journalRepository, "journalRepository");

            this.journalsImport = journalsImport;
            this.ulrichsImport = ulrichsImport;
            this.doajImport = doajImport;
            this.journalRepository = journalRepository;
        }

        [GET("")]
        public ViewResult Index()
        {
            return this.View();
        }

        [GET("import")]
        [Authorize(Roles = ApplicationRole.DataAdmin)]
        public ViewResult Import()
        {
            return this.View(new ImportViewModel());
        }

        [POST("import")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRole.DataAdmin)]
        public ActionResult Import(ImportViewModel model)
        {
            if (ModelState.IsValid)
            {
                var journals = this.GetJournalsFromSource(model.Source);
                var journalsISSNs = journals.Select(j => j.ISSN).ToSet(StringComparer.InvariantCultureIgnoreCase);

                var issns = GetISSNs(model);
                var issnsFound = issns.Intersect(journalsISSNs).ToList();
                var issnsNotFound = issns.Except(journalsISSNs).ToList();

                var journalsToImport = journals.Where(j => issnsFound.Contains(j.ISSN)).ToList();
                this.journalsImport.ImportJournals(journalsToImport, JournalsImportMode.InsertOnly);
                
                this.Session[FoundISSNsSessionKey] = issnsFound;
                this.Session[NotFoundISSNsSessionKey] = issnsNotFound;

                return this.RedirectToAction("Imported");
            }

            return this.View(model);
        }

        [GET("imported")]
        [Authorize(Roles = ApplicationRole.DataAdmin)]
        public ViewResult Imported()
        {
            var model = new ImportedViewModel
                        {
                            FoundISSNs = (IEnumerable<string>)this.Session[FoundISSNsSessionKey], 
                            NotFoundISSNs = (IEnumerable<string>)this.Session[NotFoundISSNsSessionKey]
                        };

            return this.View(model);
        }

        [GET("update")]
        [Authorize(Roles = ApplicationRole.DataAdmin)]
        public ViewResult Update()
        {
            return this.View(new UpdateViewModel());
        }

        [POST("update")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationRole.DataAdmin)]
        public ActionResult Update(UpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var journals = this.GetJournalsFromSource(model.Source);
                var issns = GetISSNs(model);

                var journalsISSNs = journals.Select(j => j.ISSN).ToSet(StringComparer.InvariantCultureIgnoreCase);
                var issnsFound = issns.Intersect(journalsISSNs).ToList();
                var issnsNotFound = issns.Except(journalsISSNs).ToList();

                var journalsToImport = journals.Where(j => issnsFound.Contains(j.ISSN)).ToList();
                this.journalsImport.ImportJournals(journalsToImport, JournalsImportMode.UpdateOnly);

                this.Session[FoundISSNsSessionKey] = issnsFound;
                this.Session[NotFoundISSNsSessionKey] = issnsNotFound;

                return this.RedirectToAction("Updated");
            }

            return this.View(model);
        }

        [GET("updated")]
        [Authorize(Roles = ApplicationRole.DataAdmin)]
        public ViewResult Updated()
        {
            var model = new UpdatedViewModel
            {
                FoundISSNs = (IEnumerable<string>)this.Session[FoundISSNsSessionKey],
                NotFoundISSNs = (IEnumerable<string>)this.Session[NotFoundISSNsSessionKey]
            };

            return this.View(model);
        }

        [GET("check")]
        [Authorize(Roles = ApplicationRole.InstitutionAdmin)]
        public ViewResult Check()
        {
            return this.View();
        }

        [POST("check")]
        [Authorize(Roles = ApplicationRole.InstitutionAdmin)]
        public ViewResult Check(CheckViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var parsedIssns = ParseISSNs(model.ISSNs);
                model.FoundISSNs = this.journalRepository.SearchByISSN(parsedIssns).Select(j => j.ISSN) ;
                model.NotFoundISSNs = parsedIssns.Except(model.FoundISSNs);
            }

            return this.View(model);
        }

        private IList<Journal> GetJournalsFromSource(JournalsImportSource importSource)
        {
            switch (importSource)
            {
                case JournalsImportSource.DOAJ:
                    return this.doajImport.GetJournals();
                case JournalsImportSource.Ulrichs:
                    return this.ulrichsImport.GetJournals(UlrichsImport.UlrichsJournalType.All);
                default:
                    throw new ArgumentOutOfRangeException("importSource");
            }
        }

        private static HashSet<string> GetISSNs(ImportViewModel model)
        {
            return ParseISSNs(model.ISSNs);
        }

        private static HashSet<string> GetISSNs(UpdateViewModel model)
        {
            return ParseISSNs(model.ISSNs);
        }

        private static HashSet<string> ParseISSNs(string issns)
        {
            return issns.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(s => s.Trim().Length > 0).Select(s => s.Trim()).ToSet(StringComparer.InvariantCultureIgnoreCase);
        }
    }
}