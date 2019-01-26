using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommunityWiki.Data;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Models.ArticleTypes;
using CommunityWiki.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CommunityWiki.Controllers
{
    [Authorize(Policy = Constants.Policies.Admin)]
    [Route("api/article-types/{typeId}/fields")]
    [Produces("application/json")]
    public class FieldsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDateTimeService _dateTimeService;
        private readonly IMapper _mapper;
        private readonly ILogger<FieldsController> _logger;

        public FieldsController(ApplicationDbContext dbContext,
            IDateTimeService dateTimeService,
            IMapper mapper,
            ILogger<FieldsController> logger)
        {
            _dbContext = dbContext;
            _dateTimeService = dateTimeService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get(int typeId)
        {
            var fields = await _dbContext.ArticleTypeFieldDefinitions
                .Where(x => x.ArticleTypeId == typeId)
                .OrderBy(x => x.Order)
                .ProjectTo<FieldDefinitionModel>()
                .ToListAsync();

            return Ok(fields);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromRoute] int typeId,  [FromBody] FieldDefinitionModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var articleType = await _dbContext.ArticleTypes.FindAsync(typeId);
            if (articleType == null)
            {
                _logger.LogInformation("Article type {typeId} not found", typeId);
                return NotFound();
            }

            try
            {
                var fieldDef = new FieldDefinition
                {
                    ArticleType = articleType,
                    FieldType = model.FieldType,
                    Order = model.Order,
                    Name = model.Name,
                    Description = model.Description,
                    MaxLength = model.MaxLength,
                    IsRequired = model.IsRequired
                };

                _dbContext.ArticleTypeFieldDefinitions.Add(fieldDef);
                await _dbContext.SaveChangesAsync();

                model.Id = fieldDef.Id;

                return Created("", model);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating field definition");
                return StatusCode(500);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int typeId, [FromRoute] int id, [FromBody] FieldDefinitionModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var articleType = await _dbContext.ArticleTypes.FindAsync(typeId);
            if (articleType == null)
            {
                _logger.LogInformation("Article type {typeId} not found", typeId);
                return NotFound();
            }

            var fieldDef = await _dbContext.ArticleTypeFieldDefinitions.FindAsync(id);
            if (fieldDef == null)
            {
                _logger.LogInformation("Field definition {id} not found", id);
                return NotFound();
            }

            try
            {
                fieldDef.FieldType = model.FieldType;
                fieldDef.Name = model.Name;
                fieldDef.Description = model.Description;
                fieldDef.Order = model.Order;
                fieldDef.IsRequired = model.IsRequired;
                fieldDef.MaxLength = model.MaxLength;

                _dbContext.ArticleTypeFieldDefinitions.Update(fieldDef);
                await _dbContext.SaveChangesAsync();

                return Ok();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error updatig field definition");
                return StatusCode(500);
            }

        }
    }
}