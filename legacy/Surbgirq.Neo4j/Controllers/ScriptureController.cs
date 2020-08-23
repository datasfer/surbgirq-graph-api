using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Neo4jClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Surbgirq.GraphApi.Extensions;
using Surbgirq.GraphApi.GraphDB;
using Surbgirq.GraphApi.Interfaces;
using Surbgirq.GraphApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Surbgirq.GraphApi.Controllers
{
    [ApiController]
    [Route("scripture")]
    public class ScriptureController : Controller
    {
        IDriver _driver;
        //IGraphClient _neo4jClient;
        private readonly ILogger<ScriptureController> _logger;
        private readonly IGraphRepository _graphRepository;
        public ScriptureController( IDriver driver,
          //                        IGraphClient neo4jClient,
                                    IGraphRepository graphRepository,
                                    ILogger<ScriptureController> logger) 
        {
            _driver = driver;
            _logger = logger;
            _graphRepository = graphRepository;
            //_neo4jClient = neo4jClient;
        }

        [HttpGet("query")]
        public async Task<ContentResult> runCypher(string query)
        {
            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }

        // curl -X GET "http://192.0.2.10:33010/Scripture/books" -H "accept: */*"
        // curl -X GET "http://localhost:33010/Scripture/books" -H "accept: */*"
        [HttpGet("books")]
        public async Task<ContentResult> getBooks()
        {
            var query = "match (b:Book) return b;";
            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }
        

        [HttpGet("books/language/{language}")]
        public async Task<ContentResult> getBooksByLanguage(string language)
        {
            var query = $"match (b:Book:{language}) return b;";
            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }

        [HttpGet("books/new")]
        public async Task<ContentResult> getBooksNewTestament()
        {
            var query = $"match (b:Book:New) where b.testament = 'New' return b;";
            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }

        [HttpGet("books/old")]
        public async Task<ContentResult> getBooksOldTestament()
        {
            var query = $"match (b:Book:Old) where b.testament = 'Old' return b;";
            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }

        [HttpGet("books/{book}")]
        public async Task<ContentResult> getBook(string book)
        {
            var query = $"match (b:Book) where b.book = '{book}' return b;";
            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }

        [HttpGet("books/{language}/{book}")]
        public async Task<ContentResult> getBook(string language, string book)
        {
            var query = $"match (b:Book:{language}) where b.book = '{book}' return b;";
            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }

       
        [HttpGet("books/{book}/chapters")]
        public async Task<ContentResult> getBookChaptersCount(string book)
        {
            
            var query = $"match (b:Book) where b.book = '{book}' return b.chapters as num;";
            var jsonString = (await _graphRepository.RunQuery(query)).ToList();
            var res = toJsonProps(jsonString);
            return Content(res, "application/json");
        }

        [HttpGet("books/{girq}/chapters/{num}/sections")]
        public async Task<ContentResult> getBookChapterSections(string girq, int num)
        {

            var query = $"match (b:Book {{book: '{girq}' }})-[:CONTAINS]-()-[*]->(s:Section) WHERE s.chapter={num} " +
                $"return s.num as num, s.title as title, b.book as book  " +
                $"order by s.num;";
         
            var jsonString = (await _graphRepository.RunQuery(query)).ToList();

            var res = toJsonPropsArr(jsonString);
            return Content(res, "application/json");
        }

        [HttpGet("translations")]
        public async Task<ContentResult> getTranslations()
        {
            var query = "match (t:Translation) return t;";
            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }

        [HttpGet("scriptures")]
        public async Task<ContentResult> getScriptures()
        {
            var query = "match (s:Scripture) return s;";
            var str =  await getQueryJson(query);
            return Content(str, "application/json");

        }



        /// Verses
        [HttpGet("{language}/books/{book}/chapters/{chapter}/oneverse/{verseNum}/")]
        public async Task<ContentResult> getVerse(string language, string book, int chapter, int verseNum)
        {
            var query = $@"MATCH (n:Book:{book}:{language}) where n.book = '{book}'
              match (n)<-[r:CONTAINS]->(ch:Chapter)-[:CONTAINS]->(v:Verse) 
              where ch.num = {chapter} and v.verse = {verseNum}
              return v";

            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }


        /// Verse range
        [HttpGet("{language}/books/{book}/chapters/{chapter}/verses/{v1}/")]
        public async Task<ContentResult> getVerseRange(string language, string book, int chapter, int v1, int v2)
        {
            Console.WriteLine("Verse Range enter");
            Console.WriteLine($"{language}/books/{book}/chapters/{chapter}/verses/{v1}? to={v2}");
            
            var query = $@"MATCH (n:Book:{book}:{language}) where n.book = '{book}'
              match (n)<-[r:CONTAINS]->(ch:Chapter)-[:CONTAINS]->(v:Verse) 
              where ch.num = {chapter} and v.verse >= {v1} and v.verse <= {v2}
              return v order by v.verse";

            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }



        /// Chapter's  sections
        [HttpGet("{language}/books/{book}/chapters/{chapter}/sections/")]
        public async Task<ContentResult> getChapterTitles(string language, string book, int chapter)
        {
            var query = $@"MATCH (n:Book:{book}:{language}) where n.book = '{book}'
            MATCH (n)-[r:CONTAINS]-(ch:Chapter)-[r1:CONTAINS*]->(sec:Section)
            where ch.num = {chapter} 
            return sec order by ch.num, sec.num";           

            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }


        /// Chapter's concret  sections
        [HttpGet("{language}/books/{book}/chapters/{chapter}/sections/{secnum}")]
        public async Task<ContentResult> getChapterTitles(string language, string book, int chapter, int secnum)
        {
            var query = $@"MATCH (n:Book:{book}:{language}) where n.book = '{book}'
            MATCH (n)-[r:CONTAINS]-(ch:Chapter)-[r1:CONTAINS*]->(sec:Section)
            where ch.num = {chapter} and sec.num = {secnum}
            return sec order by ch.num, sec.num";           

            Console.WriteLine(query);
            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }


        /// Book sections
        [HttpGet("{language}/books/{book}/sections")]
        public async Task<ContentResult> getBookSections(string language, string book)
        {
            var query = $@"MATCH (n:Book:{book}:{language}) where n.book = '{book}'
            MATCH (n)-[r:CONTAINS]-(ch:Chapter)-[r1:CONTAINS*]->(sec:Section)
            return sec order by ch.num, sec.num";           

            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }

      /*  [HttpGet("{language}/books/{book}/chapters/{chapter}/sections/{section}")]
        public async Task<ContentResult> getChapterTitles(string language, string book, int chapter)
        {
            var query = $@"MATCH (n:Book:{book}:{language}) where n.book = '{book}'
            MATCH (n:Book:Matthew:Ashxarhabar)-[r:CONTAINS]-(ch:Chapter)-[r1:CONTAINS*]->(sec:Section)
            where ch.num = {chapter} 
            return ch.title, sec.title order by ch.num, sec.num";           

            Console.WriteLine(query);
            var str = await getQueryJson(query);
            return Content(str, "application/json");
        }
*/
        /// <summary>
        /// converting to json cypher output
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<string> getQueryJson(string query)
        {
            var jsonString = await _graphRepository.RunQuery(query);
            Console.WriteLine(jsonString);
            return toJsonPropsArr(jsonString);
            
        }

        private static string toJsonEntityArr(IEnumerable<IDictionary<string, object>> jsonString)
        {
            List<JObject> res = new List<JObject>();
            foreach (var jStr in jsonString)
            {
                var j = jStr.Values.AsJson();
                var jObj = JObject.Parse(j);
                res.Add(jObj);
                //res.Add(JObject.Parse(str[0].Item2)));
            }

            // var g = $"[{string.Join(",", jsonString.ToArray())}]";
            //return ss; // Content($"[{ss}]", "application/json");
            var str = res.Aggregate("", (a, b) => $"{a},{b}").Trim(',');
            return $"[{str}]";
        }

        private static string toJsonPropsArr(IEnumerable<IDictionary<string, object>> jsonString)
        {
            List<JObject> res = new List<JObject>();

            foreach (var jStr in jsonString)
            {
                var j = jStr.AsJson();
                var jObj = JObject.Parse(j);
                res.Add(jObj);
            }
            var str = res.Aggregate("", (a, b) => $"{a},{b}").Trim(',');
            return $"[{str}]";
        }

        private static string toJsonProps(IEnumerable<IDictionary<string, object>> jsonString)
        {
            List<JObject> res = new List<JObject>();

            foreach (var jStr in jsonString)
            {
                var j = jStr.AsJson();
                var jObj = JObject.Parse(j);
                res.Add(jObj);
            }
            var str = res.Aggregate("", (a, b) => $"{a},{b}").Trim(',');
            return $"{str}";
        }


        async Task<bool> test()
        { 
            var ttt = await _graphRepository.FindAll<Company>(("name", "LexCorp"));

            var ys =  await _graphRepository.GetNodesByLabel<Company>();

            foreach (var c in ys)
            {
                Debug.WriteLine(c.ToString());
            }
            

            var xs = await _graphRepository.GetAllEntities<Company>();

           // xs.ToList().ForEach(a => Debug.WriteLine(a.Node));

            IAsyncSession session = _driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Read));
            try
            {
                await session.ReadTransactionAsync(PrintChapters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to connect neo4j");
            }
            finally
            {
                await session.CloseAsync();
            }


            //AddEmployAndMakeFriends();

            //var neo4jClient = new GraphClient(new Uri("http://localhost:11006"));
            //neo4jClient.Connect();
            //var query = neo4jClient.Cypher
            //    .Match("MATCH (n:Chapter) RETURN n")
            //    .Return<Chapter>("Chapter").Results;

            ////            IDriver driver = GraphDatabase.Driver("neo4j://localhost:7687", AuthTokens.Basic("username", "CHANGE_ME"));
            //IAsyncSession session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));
            //try
            //{
            //    IResultCursor cursor = await session.RunAsync("MATCH (n:Chapter) RETURN n");
            //    var res = await cursor.ConsumeAsync();

            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "failed to connect neo4j");
            //}
            //finally
            //{
            //    await session.CloseAsync();
            //}

           // await _driver.CloseAsync();

            return true;
        }

        

        private async Task<int> PrintChapters(IAsyncTransaction tx)
        {
            //var cursor = await tx.RunAsync("MATCH (a)-[:KNOWS]->(b) RETURN a.name, b.name");
            // IResultCursor cursor = await tx.RunAsync("MATCH (n:Chapter) RETURN n.` book`, n.` num`");
            IResultCursor cursor = await tx.RunAsync("MATCH (n:Chapter) RETURN n");

            var count = 0;

            await cursor.ForEachAsync(record =>
            {
               count++;

                var node = record["n"].As<INode>();
                var chapter = new Chapter
                {
                    Book = node[" book"].As<string>(),
                    Number = node[" num"].As<int>(),
                };

                Debug.WriteLine(chapter.ToString());
                //Debug.WriteLine($"Book: {record["n.` book`"]}, Num: {record["n.` num`"]}");
            });

            return count;
        }

        // Create a company node
        private Task<IResultCursor> AddCompany(IAsyncTransaction tx, string name)
        {
            return tx.RunAsync("CREATE (a:Company {name: $name})", new { name });
        }

        // Create a person node
        private Task<IResultCursor> AddPerson(IAsyncTransaction tx, string name)
        {
            return tx.RunAsync("CREATE (a:Person {name: $name})", new { name });
        }

        // Create an employment relationship to a pre-existing company node.
        // This relies on the person first having been created.
        //private IResult Employ(ITransaction tx, string personName, string companyName)
        //{
        //    return tx.Run(@"MATCH (person:Person {name: $personName})
        //     MATCH (company:Company {name: $companyName})
        //     CREATE (person)-[:WORKS_FOR]->(company)", new { personName, companyName });
        //}

        // Create a friendship between two people.
        //private IResult MakeFriends(ITransaction tx, string name1, string name2)
        //{
        //    return tx.Run(@"MATCH (a:Person {name: $name1})
        //     MATCH (b:Person {name: $name2})
        //     MERGE (a)-[:KNOWS]->(b)", new { name1, name2 });
        //}

        //// Match and display all friendships.
        //private int PrintFriendships(ITransaction tx)
        //{
        //    var result = tx.Run("MATCH (a)-[:KNOWS]->(b) RETURN a.name, b.name");

        //    var count = 0;
        //    foreach (var record in result)
        //    {
        //        count++;
        //        Console.WriteLine($"{record["a.name"]} knows {record["b.name"]}");
        //    }

        //    return count;
        //}

        private void AddEmployAndMakeFriends()
        {
            // To collect the session bookmarks
            var savedBookmarks = new List<Bookmark>();

            IAsyncSession session1;
            IAsyncSession session2;

            try
            {
                // Create the first person and employment relationship.
                session1 = _driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Write));
                {
                    session1.WriteTransactionAsync(tx => AddCompany(tx, "Wayne Enterprises"));
                    session1.WriteTransactionAsync(tx => AddPerson(tx, "Alice"));
                    //session1.WriteTransactionAsync(tx => Employ(tx, "Alice", "Wayne Enterprises"));

                    savedBookmarks.Add(session1.LastBookmark);
                }

                // Create the second person and employment relationship.
                session2 = _driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Write));
                {
                    session2.WriteTransactionAsync(tx => AddCompany(tx, "LexCorp"));
                    session2.WriteTransactionAsync(tx => AddPerson(tx, "Bob"));
                    //session2.WriteTransaction(tx => Employ(tx, "Bob", "LexCorp"));

                    savedBookmarks.Add(session2.LastBookmark);
                }

                session1.CloseAsync();
                session2.CloseAsync();
            }
            finally
            {
                //session1.CloseAsync();
                //session2.CloseAsync();
            }

            //// Create a friendship between the two people created above.
            var session3 = _driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Read));
            session3.ReadTransactionAsync(PrintChapters);
            session3.CloseAsync();
        }


        private async Task<List<IRecord>> queryRead(string query)
        {
            List<IRecord> _records = null;

            IAsyncSession session = _driver.AsyncSession();
            try
            {
                _records = await session.ReadTransactionAsync(async tx =>
                {

                    // Send cypher query to the database
                    var _reader = await tx.RunAsync(query);

                    List<IRecord> _records = await _reader.ToListAsync();


                    return _records;
                });

            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }

            finally
            {
                // asynchronously close session
                await session.CloseAsync();
            }


            return _records;
        }

        public class Video
        {
            public string Id { get;  set; }
            public string YoutubeId { get; set; }
            public string Title { get; set; }
            public DateTime? Published { get; set; }
        }
        private async Task<List<Video>> GetVideos(IDriver driver)
        {
            var statement = "MATCH (n:Video) RETURN n";
            var session = driver.AsyncSession();
                return await session.ReadTransactionAsync(async tx => {
                    var result = await tx.RunAsync(statement);
                    return await result.ToListAsync<Video>(record => {
                        var node = record[0].As<INode>();
                        return new Video
                        {
                            Id = node.Properties.GetValueOrDefault("id", null).As<string>(),
                            YoutubeId = node.Properties.GetValueOrDefault("youtubeId", null).As<string>(),
                            Title = node.Properties.GetValueOrDefault("title", null).As<string>(),
                            Published = node.Properties.GetValueOrDefault("published", null).As<DateTimeOffset?>()?.UtcDateTime
                        };
                    });
                });
            
        }


        //[HttpGet]
        //public List<string> GetPeople()
        //{
        //    //var session = _driver.AsyncSession();
        //    //{
        //    //    return session.ReadTransactionAsync(async (tx) =>
        //    //    {
        //    //        var result = tx.RunAsync("MATCH (n:Chapter) RETURN n");
        //    //        //return result.Result.FetchAsync() (record => record[0].As<string>()).ToList();
        //    //    });
        //    //}

        //    return null;
        //}
    }
}
