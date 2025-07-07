using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using HealthcareApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthcareApi.Services
{
    public class LuceneAppointmentIndexService
    {
        private readonly IndexWriter _writer;
        private readonly StandardAnalyzer _analyzer;
        private static readonly LuceneVersion VERSION = LuceneVersion.LUCENE_48;

        private static readonly string[] Fields = new[]
        {
            nameof(AppointmentWithNamesDTO.PatientName),
            nameof(AppointmentWithNamesDTO.DoctorName),
            nameof(AppointmentWithNamesDTO.AppointmentDate),
            nameof(AppointmentWithNamesDTO.Reason),
            nameof(AppointmentWithNamesDTO.Status),
            nameof(AppointmentWithNamesDTO.Notes)
        };

        public LuceneAppointmentIndexService(IndexWriter writer, StandardAnalyzer analyzer)
        {
            _writer   = writer;
            _analyzer = analyzer;
        }

        /// <summary>
        /// Indexes an appointment for free-text search, storing all searchable fields.
        /// </summary>
        public void IndexAppointment(AppointmentWithNamesDTO dto)
        {
            _writer.DeleteDocuments(new Term(nameof(AppointmentWithNamesDTO.AppointmentID), dto.AppointmentID.ToString()));

            var doc = new Document
            {
                new StringField(nameof(AppointmentWithNamesDTO.AppointmentID),
                                dto.AppointmentID.ToString(),
                                Field.Store.YES),

                new TextField(nameof(AppointmentWithNamesDTO.PatientName),
                              dto.PatientName ?? string.Empty,
                              Field.Store.YES),
                new TextField(nameof(AppointmentWithNamesDTO.DoctorName),
                              dto.DoctorName ?? string.Empty,
                              Field.Store.YES),

                new StringField(nameof(AppointmentWithNamesDTO.AppointmentDate),
                                dto.AppointmentDate.ToString("yyyy-MM-dd"),
                                Field.Store.YES),

                new TextField(nameof(AppointmentWithNamesDTO.Reason),
                              dto.Reason ?? string.Empty,
                              Field.Store.YES),
                new TextField(nameof(AppointmentWithNamesDTO.Status),
                              dto.Status ?? string.Empty,
                              Field.Store.YES),
                new TextField(nameof(AppointmentWithNamesDTO.Notes),
                              dto.Notes ?? string.Empty,
                              Field.Store.YES)
            };

            _writer.AddDocument(doc);
            _writer.Flush(triggerMerge: false, applyAllDeletes: true);
        }

        /// <summary>
        /// Searches the Lucene index for appointments matching the free-text query
        /// across PatientName, DoctorName, AppointmentDate, Reason, Status, and Notes.
        /// </summary>
        public PagedResult<AppointmentWithNamesDTO> Search(string queryText, int pageNumber, int pageSize)
        {
            using var reader   = DirectoryReader.Open(_writer, applyAllDeletes: true);
            var searcher       = new IndexSearcher(reader);

            Query luceneQuery;

            if (string.IsNullOrWhiteSpace(queryText))
            {
                luceneQuery = new MatchAllDocsQuery();
            }
            else
            {
                var parser = new MultiFieldQueryParser(VERSION, Fields, _analyzer)
                {
                    DefaultOperator = Operator.AND
                };
                var escaped = QueryParserBase.Escape(queryText);
                luceneQuery = parser.Parse(escaped);
            }

            var topDocs = searcher.Search(luceneQuery, pageNumber * pageSize);
            var hits    = topDocs.ScoreDocs
                                  .Skip((pageNumber - 1) * pageSize)
                                  .Take(pageSize);

            var results = hits.Select(h =>
            {
                var d = searcher.Doc(h.Doc);
                return new AppointmentWithNamesDTO
                {
                    AppointmentID   = int.Parse(d.Get(nameof(AppointmentWithNamesDTO.AppointmentID))),
                    PatientName     = d.Get(nameof(AppointmentWithNamesDTO.PatientName)),
                    DoctorName      = d.Get(nameof(AppointmentWithNamesDTO.DoctorName)),
                    AppointmentDate = DateTime.Parse(d.Get(nameof(AppointmentWithNamesDTO.AppointmentDate))),
                    Reason          = d.Get(nameof(AppointmentWithNamesDTO.Reason)),
                    Status          = d.Get(nameof(AppointmentWithNamesDTO.Status)),
                    Notes           = d.Get(nameof(AppointmentWithNamesDTO.Notes))
                };
            }).ToList();

            return new PagedResult<AppointmentWithNamesDTO>
            {
                Data       = results,
                TotalCount = topDocs.TotalHits
            };
        }
    }
}
