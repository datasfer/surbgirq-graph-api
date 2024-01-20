using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolyGraph.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Newtonsoft.Json;

namespace HolyGraph.API.Controllers
{
    public class VerseController : BaseApiController
    {
        private readonly IDriver _driver;

        public VerseController()
        {
            //_driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "CHANGE_ME"));
            _driver = GraphDatabase.Driver("bolt://192.0.2.10:7687", AuthTokens.Basic("neo4j", "CHANGE_ME"));
        }
    }
}