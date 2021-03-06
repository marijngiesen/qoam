﻿namespace QOAM.Core.Import
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    using CsvHelper;
    using CsvHelper.Configuration;

    using NLog;

    using QOAM.Core.Helpers;

    using Validation;

    public class DoajImport
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly DoajSettings doajSettings;

        private static readonly CsvConfiguration CsvConfiguration = new CsvConfiguration
                                                                        {
                                                                            HasHeaderRecord = true,
                                                                            Delimiter = ",",
                                                                            TrimFields = true
                                                                        };

        public DoajImport(DoajSettings doajSettings)
        {
            Requires.NotNull(doajSettings, "doajSettings");
            
            this.doajSettings = doajSettings;
        }

        public IList<Journal> GetJournals()
        {
            return this.ParseJournals(this.DownloadJournals());
        }

        private string DownloadJournals()
        {
            Logger.Info("Downloading journals...");

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;

                return webClient.DownloadString(this.doajSettings.CsvUrl);
            }
        }

        public IList<Journal> ParseJournals(string csv)
        {
            Logger.Info("Parsing journals...");

            using (var csvReader = new CsvReader(new StringReader(csv), CsvConfiguration))
            {
                var importRecords = csvReader.GetRecords<DoajImportRecord>().ToList();
                return importRecords.Select(i => i.ToJournal()).Where(j => j.IsValid()).ToList();
            }
        }
    }
}