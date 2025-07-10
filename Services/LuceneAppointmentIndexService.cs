using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using HealthcareApi.Models;

namespace HealthcareApi.Services;

public class LuceneAppointmentIndexService
{
    private readonly IndexWriter _writer;
    private readonly StandardAnalyzer _analyzer;
    private static readonly LuceneVersion VERSION = LuceneVersion.LUCENE_48;
    private static readonly string[] Fields = new[]
    {
        nameof(Appointment.AppointmentID),
        nameof(Appointment.AppointmentDate),
        nameof(Appointment.Reason),
        nameof(Appointment.Status),
        nameof(Appointment.Notes)
    };

    public LuceneAppointmentIndexService(IndexWriter writer, StandardAnalyzer analyzer)
    {
        _writer   = writer;
        _analyzer = analyzer;
    }

    public void IndexAppointment(Appointment a)
    {
        _writer.DeleteDocuments(new Term(nameof(Appointment.AppointmentID), a.AppointmentID.ToString()));
        if (a.isActive)
        {
            var doc = new Document
            {
                new StringField(nameof(Appointment.AppointmentID), a.AppointmentID.ToString(), Field.Store.YES),
                new StringField(nameof(Appointment.AppointmentDate), a.AppointmentDate.ToString("o"), Field.Store.YES),
                new TextField(nameof(Appointment.Reason), a.Reason ?? "", Field.Store.YES),
                new TextField(nameof(Appointment.Status), a.Status ?? "", Field.Store.YES),
                new TextField(nameof(Appointment.Notes), a.Notes ?? "", Field.Store.YES)
            };
            _writer.AddDocument(doc);
        }
        _writer.Flush(applyAllDeletes: true, triggerMerge: false);
    }

    public PagedResult<AppointmentWithNamesDTO> Search(
        string? queryText, int pageNumber, int pageSize,
        string? sortField = null, string? sortOrder = null)
    {
        using var reader   = DirectoryReader.Open(_writer, applyAllDeletes: true);
        var searcher       = new IndexSearcher(reader);
        Query luceneQuery  = string.IsNullOrWhiteSpace(queryText)
            ? new MatchAllDocsQuery()
            : new MultiFieldQueryParser(VERSION, Fields, _analyzer)
                  {
                      DefaultOperator = Operator.OR
                  }
                  .Parse(QueryParserBase.Escape(queryText) + "*");
        TopDocs top = string.IsNullOrWhiteSpace(sortField)
            ? searcher.Search(luceneQuery, pageNumber * pageSize)
            : searcher.Search(
                luceneQuery,
                pageNumber * pageSize,
                new Sort(new SortField(sortField, SortFieldType.STRING,
                    sortOrder?.ToLower() == "desc")));

        var hits = top.ScoreDocs
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(h =>
            {
                var d = searcher.Doc(h.Doc);
                return new AppointmentWithNamesDTO
                {
                    AppointmentID   = int.Parse(d.Get(nameof(Appointment.AppointmentID))),
                    AppointmentDate = DateTime.Parse(d.Get(nameof(Appointment.AppointmentDate))),
                    Reason          = d.Get(nameof(Appointment.Reason)),
                    Status          = d.Get(nameof(Appointment.Status)),
                    Notes           = d.Get(nameof(Appointment.Notes))
                };
            })
            .ToList();

        return new PagedResult<AppointmentWithNamesDTO>
        {
            Data       = hits,
            TotalCount = (int)top.TotalHits,
            PageNumber = pageNumber,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(top.TotalHits / (double)pageSize)
        };
    }
}
