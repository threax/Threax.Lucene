# Threax.Lucene
This library makes using Lucene.Net a bit easier by managing the lucene lifecycle for you and integrating with the .Net dependency injection library.

## How to Use
The following is the most basic lucene service you can implement. The code contains comments that shows how each section works.

Given the following sample data
```
public class SampleData
{
    public Guid SampleDataId { get; set; }

    public String Description { get; set; }
}
```

This can be indexed with:
```
public class BasicSearch : SearchWithIndexGuid<SampleData, BasicSearch>
{
    //Define per field boost settings
    private const float boostDescription = 1.0f;

    //Setup the names of the columns in the index
    private const String DescriptionFuzzy = nameof(DescriptionFuzzy);
    private const String DescriptionStandard = nameof(DescriptionStandard);
    private const String DescriptionStore = nameof(DescriptionStore);

    //Set the names of the fields you want to query
    private static readonly String[] QueryFields = new String[]
    {
        DescriptionFuzzy, DescriptionStandard,
    };

    //Constructor, set up analyzer and pass it to the base class with SetConfig.
    public BasicSearch(ILuceneDirectoryProvider<BasicSearch> directoryProvider, LuceneServiceOptions<BasicSearch> options) : base("SampleDataId", directoryProvider, options)
    {
        var standard = new StandardAnalyzer(version);
        var english = new EnglishAnalyzer(version, EnglishAnalyzer.DefaultStopSet);
        var analyzer = new PerFieldAnalyzerWrapper(standard, new Dictionary<String, Analyzer>
        {
            { DescriptionFuzzy, english }
        });
        SetConfig(analyzer, QueryFields);
    }

    //Index function, takes in the input sample data.
    public async Task Index(IEnumerable<SampleData> descriptions)
    {
        //Call the base class index function, which will call your callback with the writer and everything setup.
        await base.Index(async (writer, currentIndexDocs) =>
        {
            //Go through all your items
            foreach (var desc in descriptions)
            {
                //Remove the item from the list of current index docs.
                currentIndexDocs.Remove(desc.SampleDataId);

                //Create documents
                var doc = new Document();

                //Add each field, you can also set boosts here
                doc.Add(new TextField(DescriptionFuzzy, desc.Description, Field.Store.NO)
                {
                    Boost = boostDescription
                });
                doc.Add(new TextField(DescriptionStandard, desc.Description, Field.Store.NO)
                {
                    Boost = boostDescription
                });
                doc.Add(new StringField(DescriptionStore, desc.Description, Field.Store.YES));

                //Add documents to index and call the update function
                doc.Add(new StringField(IdField, desc.SampleDataId.ToString(), Field.Store.YES));
                writer.UpdateDocument(new Lucene.Net.Index.Term(IdField, desc.SampleDataId.ToString()), doc); //Use the page links as ids, see if we can avoid the string
            }
        });
    }

    //This function is called to convert lucene resutls to your final return type
    protected override IEnumerable<SampleData> CreateResults(IndexSearcher searcher, TopDocs hits)
    {
        foreach (var scoreDoc in hits.ScoreDocs)
        {
            var doc = searcher.Doc(scoreDoc.Doc);
            yield return new SampleData()
            {
                SampleDataId = Guid.Parse(doc.Get(IdField)),
                Description = doc.Get(DescriptionStore)
            };
        }
    }
}
```

Finally add this class to your startup with:
```
services.AddThreaxLucene<BasicSearch>(o => Configuration.Bind("Lucene", o));
```

Then you can search by calling the search function on an instance of your search class. That will return an IEnumerable over your search type.