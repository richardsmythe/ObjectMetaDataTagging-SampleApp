﻿using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Api.Services;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using ObjectMetaDataTagging.Utilities;

namespace ObjectMetaDataTagging.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {

        private readonly IObjectMetaDataTaggingFacade<BaseTag> _taggingFacade;
        private readonly IGenerateTestData _generateTestData;
        private List<IEnumerable<KeyValuePair<string, object>>> testData;
        private static bool isTestDataInitialised = false;

        public TagController(
            IObjectMetaDataTaggingFacade<BaseTag> taggingFacade,
            IGenerateTestData generateTestData
            )
        {
            _taggingFacade = taggingFacade;
            _generateTestData = generateTestData;


            // Check if data is already initialised before calling InitialiseTestData
            if (!isTestDataInitialised)
            {
                InitialiseTestData();
                isTestDataInitialised = true;
            }
        }

        private async Task InitialiseTestData()
        {
            testData = await _generateTestData.GenerateTestData();

        }

        [HttpGet("print-object-graph")]
        public async Task<IActionResult> PrintObjectGraph()
        {
            var objectGraph = await _taggingFacade.GetObjectGraph();
            ObjectGraphBuilder.PrintObjectGraph(objectGraph);
            return Ok(objectGraph);
        }
    }
}
