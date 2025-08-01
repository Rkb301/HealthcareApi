using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using HealthcareApi.Models;
using HealthcareApi.Repositories;

namespace HealthcareApi.Services;

public class LuceneDoctorIndexService
{
    private readonly IndexWriter _writer;
    private readonly StandardAnalyzer _analyzer;
    private static readonly LuceneVersion VERSION = LuceneVersion.LUCENE_48;
    private readonly IDoctorRepository _doctorRepository;
    private static readonly string[] Fields = new[]
    {
        nameof(Doctor.DoctorID),
        nameof(Doctor.FirstName),
        nameof(Doctor.LastName),
        nameof(Doctor.Specialization),
        nameof(Doctor.ContactNumber),
        nameof(Doctor.Email),
        nameof(Doctor.Schedule)
    };

    public LuceneDoctorIndexService(IndexWriter writer, StandardAnalyzer analyzer, IDoctorRepository doctorRepository)
    {
        _writer = writer;
        _analyzer = analyzer;
        _doctorRepository = doctorRepository;
    }

    public void IndexDoctor(Doctor d)
    {
        if (d?.DoctorID == null || d.DoctorID <= 0)
        {
            // Log warning and skip invalid doctors
            return;
        }

        _writer.DeleteDocuments(new Term(nameof(Doctor.DoctorID), d.DoctorID.ToString()));
        if (d.isActive)
        {
            var doc = new Document
            {
                new StringField(nameof(Doctor.DoctorID), d.DoctorID.ToString(), Field.Store.YES),
                new TextField(nameof(Doctor.FirstName), d.FirstName ?? "", Field.Store.YES),
                new TextField(nameof(Doctor.LastName), d.LastName ?? "", Field.Store.YES),
                new TextField(nameof(Doctor.Specialization), d.Specialization ?? "", Field.Store.YES),
                new TextField(nameof(Doctor.ContactNumber), d.ContactNumber ?? "", Field.Store.YES),
                new TextField(nameof(Doctor.Email), d.Email ?? "", Field.Store.YES),
                new TextField(nameof(Doctor.Schedule), d.Schedule ?? "", Field.Store.YES)
            };
            _writer.AddDocument(doc);
        }
        _writer.Flush(applyAllDeletes: true, triggerMerge: false);
    }


    public PagedResult<Doctor> Search(
        string? queryText, int pageNumber, int pageSize,
        string? sortField = null, string? sortOrder = null
    )
    {
        using var reader = DirectoryReader.Open(_writer, applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);
        Query luceneQuery = string.IsNullOrWhiteSpace(queryText)
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

        // Safe parsing with null checks
        var hits = top.ScoreDocs
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(h =>
            {
                var d = searcher.Doc(h.Doc);

                // Safe parsing for DoctorID - this prevents the exception
                var doctorIdStr = d.Get(nameof(Doctor.DoctorID));
                if (string.IsNullOrEmpty(doctorIdStr) || !int.TryParse(doctorIdStr, out var doctorId))
                {
                    // Skip this document if DoctorID is invalid
                    return null;
                }

                return new Doctor
                {
                    DoctorID = doctorId,
                    FirstName = d.Get(nameof(Doctor.FirstName)) ?? "",
                    LastName = d.Get(nameof(Doctor.LastName)) ?? "",
                    Specialization = d.Get(nameof(Doctor.Specialization)) ?? "",
                    ContactNumber = d.Get(nameof(Doctor.ContactNumber)) ?? "",
                    Email = d.Get(nameof(Doctor.Email)) ?? "",
                    Schedule = d.Get(nameof(Doctor.Schedule)) ?? ""
                };
            })
            .Where(doctor => doctor != null) // Filter out null entries
            .ToList();

        return new PagedResult<Doctor>
        {
            Data = hits,
            TotalCount = (int)top.TotalHits,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }


    public async Task RebuildIndexAsync()
    {
        // Clear the existing index
        _writer.DeleteAll();
        _writer.Commit();
        
        var allDoctors = await _doctorRepository.GetAllActiveDoctorsAsync();
        
        foreach (var doctor in allDoctors)
        {
            IndexDoctor(doctor);
        }
        
        _writer.Commit();
    }

}
