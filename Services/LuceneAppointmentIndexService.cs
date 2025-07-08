using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using HealthcareApi.Models;
using System;
using System.Linq;

namespace HealthcareApi.Services
{
    public class LuceneAppointmentIndexService
    {
        private readonly IndexWriter _writer;
        private readonly StandardAnalyzer _analyzer;
        private static readonly LuceneVersion VERSION = LuceneVersion.LUCENE_48;

        // Use DTO field names so Search returns AppointmentWithNamesDTO correctly
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
        /// Indexes an Appointment entity for free-text search.
        /// </summary>
        public void IndexAppointment(Appointment entity)
        {
            // Delete any existing document for this AppointmentID
            _writer.DeleteDocuments(new Term(nameof(Appointment.AppointmentID), entity.AppointmentID.ToString()));

            // Build combined names
            var patientName = entity.Patient != null
                ? $"{entity.Patient.FirstName} {entity.Patient.LastName}"
                : string.Empty;
            var doctorName = entity.Doctor != null
                ? $"{entity.Doctor.FirstName} {entity.Doctor.LastName}"
                : string.Empty;

            // Create and add document
            var doc = new Document
            {
                // Store the AppointmentID for retrieval/parsing
                new StringField(nameof(Appointment.AppointmentID),
                                entity.AppointmentID.ToString(),
                                Field.Store.YES),

                // Indexed text fields (DTO names)
                new TextField(nameof(AppointmentWithNamesDTO.PatientName),
                              patientName,
                              Field.Store.YES),
                new TextField(nameof(AppointmentWithNamesDTO.DoctorName),
                              doctorName,
                              Field.Store.YES),

                // Date as lexicographically sortable string
                new StringField(nameof(AppointmentWithNamesDTO.AppointmentDate),
                                entity.AppointmentDate.ToString("yyyy-MM-dd"),
                                Field.Store.YES),

                // Other searchable fields
                new TextField(nameof(AppointmentWithNamesDTO.Reason),
                              entity.Reason ?? string.Empty,
                              Field.Store.YES),
                new TextField(nameof(AppointmentWithNamesDTO.Status),
                              entity.Status ?? string.Empty,
                              Field.Store.YES),
                new TextField(nameof(AppointmentWithNamesDTO.Notes),
                              entity.Notes ?? string.Empty,
                              Field.Store.YES)
            };

            _writer.AddDocument(doc);
            _writer.Flush(triggerMerge: false, applyAllDeletes: true);
        }

        /// <summary>
        /// Performs a paged, multi-field, AND-based free-text search
        /// and returns AppointmentWithNamesDTO results.
        /// </summary>
        public PagedResult<AppointmentWithNamesDTO> Search(string queryText, int pageNumber, int pageSize)
        {
            using var reader   = DirectoryReader.Open(_writer, applyAllDeletes: true);
            var searcher       = new IndexSearcher(reader);

            Query luceneQuery = string.IsNullOrWhiteSpace(queryText)
                ? new MatchAllDocsQuery()
                : new MultiFieldQueryParser(VERSION, Fields, _analyzer)
                      {
                          DefaultOperator = Operator.AND
                      }
                      .Parse(QueryParserBase.Escape(queryText));

            var topDocs = searcher.Search(luceneQuery, pageNumber * pageSize);
            var hits    = topDocs.ScoreDocs
                                  .Skip((pageNumber - 1) * pageSize)
                                  .Take(pageSize);

            var results = hits.Select(h =>
            {
                var d = searcher.Doc(h.Doc);
                return new AppointmentWithNamesDTO
                {
                    AppointmentID   = int.Parse(d.Get(nameof(Appointment.AppointmentID))),
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
