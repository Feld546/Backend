using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ochakovo.Context;
using Ochakovo.Entities;

namespace Ochakovo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VacancyController : ControllerBase
    {
        private readonly OchakovoDbContext _context;

        public VacancyController(OchakovoDbContext context)
        {
            _context = context;
        }

        // GET: api/Vacancy
        [HttpGet]
        public async Task<ActionResult<IEnumerable<vacancy>>> Getvacancies()
        {
            return await _context.vacancies.ToListAsync();
        }

        // GET: api/Vacancy/5
        [HttpGet("{id}")]
        public async Task<ActionResult<vacancy>> Getvacancy(int id)
        {
            var vacancy = await _context.vacancies.FindAsync(id);

            if (vacancy == null)
            {
                return NotFound();
            }

            return vacancy;
        }

        // PUT: api/Vacancy/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Putvacancy(int id, vacancy vacancy)
        {
            if (id != vacancy.VacancyId)
            {
                return BadRequest();
            }

            _context.Entry(vacancy).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!vacancyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Vacancy
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<vacancy>> Postvacancy(vacancy vacancy)
        {
            _context.vacancies.Add(vacancy);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getvacancy", new { id = vacancy.VacancyId }, vacancy);
        }

        // DELETE: api/Vacancy/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Deletevacancy(int id)
        {
            var vacancy = await _context.vacancies.FindAsync(id);
            if (vacancy == null)
            {
                return NotFound();
            }

            _context.vacancies.Remove(vacancy);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool vacancyExists(int id)
        {
            return _context.vacancies.Any(e => e.VacancyId == id);
        }
    }
}
