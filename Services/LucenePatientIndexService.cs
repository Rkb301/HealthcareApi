using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using HealthcareApi.Models;
using System.Linq;
using System.Linq.Expressions;

public class LucenePatientIndexService
{
    private readonly IndexWriter _writer;
    private readonly StandardAnalyzer _analyzer;
    private static readonly LuceneVersion VERSION = LuceneVersion.LUCENE_48;
    private static readonly string[] Fields = new[]
    {
        nameof(Patient.PatientID),
        nameof(Patient.FirstName),
        nameof(Patient.LastName),
        "DateOfBirth",
        nameof(Patient.Gender),
        nameof(Patient.ContactNumber),
        nameof(Patient.Address),
        nameof(Patient.MedicalHistory),
        nameof(Patient.Allergies),
        nameof(Patient.CurrentMedications)
    };

    public LucenePatientIndexService(IndexWriter writer, StandardAnalyzer analyzer)
    {
        _writer = writer;
        _analyzer = analyzer;
    }

    public void IndexPatient(Patient p)
    {
        _writer.DeleteDocuments(new Term(nameof(Patient.PatientID), p.PatientID.ToString()));

        var doc = new Document
        {
            new StringField(nameof(Patient.PatientID), p.PatientID.ToString(), Field.Store.YES),
            new TextField(nameof(Patient.FirstName), p.FirstName ?? "", Field.Store.YES),
            new TextField(nameof(Patient.LastName),  p.LastName  ?? "", Field.Store.YES),
            new StringField("DateOfBirth", p.DateOfBirth?.ToString("yyyy-MM-dd") ?? "", Field.Store.YES),
            new TextField(nameof(Patient.Gender), p.Gender ?? "", Field.Store.YES),
            new TextField(nameof(Patient.ContactNumber), p.ContactNumber ?? "", Field.Store.YES),
            new TextField(nameof(Patient.Address),       p.Address       ?? "", Field.Store.YES),
            new TextField(nameof(Patient.MedicalHistory),p.MedicalHistory?? "", Field.Store.YES),
            new TextField(nameof(Patient.Allergies),     p.Allergies     ?? "", Field.Store.YES),
            new TextField(nameof(Patient.CurrentMedications), p.CurrentMedications ?? "", Field.Store.YES)
        };

        _writer.AddDocument(doc);
        _writer.Flush(triggerMerge: false, applyAllDeletes: true);
    }

    public PagedResult<Patient> Search(string queryText, int pageNumber, int pageSize, string? sortField = null, string? sortOrder = null)
    {
        using var reader = DirectoryReader.Open(_writer, applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        Query luceneQuery = string.IsNullOrWhiteSpace(queryText)
            ? new MatchAllDocsQuery()
            : new MultiFieldQueryParser(VERSION, Fields, _analyzer)
            {
                DefaultOperator = Operator.AND
            }
            .Parse(QueryParserBase.Escape(queryText));

        Sort sort = null;
        if (!string.IsNullOrWhiteSpace(sortField))
        {
            var sortFieldObj = CreateSortField(sortField, sortOrder);
            sort = new Sort(sortFieldObj);
        }

        TopDocs topDocs = sort != null 
            ? searcher.Search(luceneQuery, pageNumber * pageSize, sort)
            : searcher.Search(luceneQuery, pageNumber * pageSize);

        var hits = topDocs.ScoreDocs
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var results = hits.Select(h =>
        {
            var d = searcher.Doc(h.Doc);

            return new Patient
            {
                FirstName = d.Get(nameof(Patient.FirstName)),
                LastName = d.Get(nameof(Patient.LastName)),
                DateOfBirth = DateOnly.Parse(d.Get("DateOfBirth")),
                Gender = d.Get(nameof(Patient.Gender)),
                ContactNumber = d.Get(nameof(Patient.ContactNumber)),
                Address = d.Get(nameof(Patient.Address)),
                MedicalHistory = d.Get(nameof(Patient.MedicalHistory)),
                Allergies = d.Get(nameof(Patient.Allergies)),
                CurrentMedications = d.Get(nameof(Patient.CurrentMedications))
            };
        }).ToList();

        return new PagedResult<Patient>
        {
            Data = results,
            TotalCount = topDocs.TotalHits,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)topDocs.TotalHits / pageSize)
        };
    }

    private SortField CreateSortField(string fieldName, string? sortOrder)
    {
        bool reverse = false;
        if (!string.IsNullOrWhiteSpace(sortOrder) && sortOrder.ToLower() == "desc")
        {
            reverse = true;
        }

        if (sortOrder == "desc")
        {
            return fieldName.ToLower() switch
            {
                "firstname" => new SortField(nameof(Patient.FirstName), SortFieldType.STRING, reverse),
                "lastname" => new SortField(nameof(Patient.LastName), SortFieldType.STRING, reverse),
                "dateofbirth" => new SortField("DateOfBirth", SortFieldType.STRING, reverse),
                "contactnumber" => new SortField(nameof(Patient.ContactNumber), SortFieldType.STRING, reverse),
                "medicalhistory" => new SortField(nameof(Patient.Address), SortFieldType.STRING, reverse),
                "allergies" => new SortField(nameof(Patient.Allergies), SortFieldType.STRING, reverse),
                "currentmedications" => new SortField(nameof(Patient.CurrentMedications), SortFieldType.STRING, reverse),
                "" => new SortField(nameof(Patient.PatientID), SortFieldType.STRING, reverse),
                _ => new SortField(nameof(Patient.PatientID), SortFieldType.INT32, reverse) // Default sort
            };
        }
        else if (sortOrder == "asc")
        {
            return fieldName.ToLower() switch
            {
                "firstname" => new SortField(nameof(Patient.FirstName), SortFieldType.STRING),
                "lastname" => new SortField(nameof(Patient.LastName), SortFieldType.STRING),
                "dateofbirth" => new SortField("DateOfBirth", SortFieldType.STRING),
                "contactnumber" => new SortField(nameof(Patient.ContactNumber), SortFieldType.STRING),
                "medicalhistory" => new SortField(nameof(Patient.Address), SortFieldType.STRING),
                "allergies" => new SortField(nameof(Patient.Allergies), SortFieldType.STRING),
                "currentmedications" => new SortField(nameof(Patient.CurrentMedications), SortFieldType.STRING),
                "" => new SortField(nameof(Patient.PatientID), SortFieldType.STRING),
                _ => new SortField(nameof(Patient.PatientID), SortFieldType.INT32) // Default sort
            };
        }
        else
        {
            Console.WriteLine("error");
            return null;
        }
    }
}
