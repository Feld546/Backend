using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
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
    public class CandidateController : ControllerBase
    {
        private readonly OchakovoDbContext _context;

        public CandidateController(OchakovoDbContext context)
        {
            _context = context;
        }
        
        // GET: api/Candidate
        [HttpGet]
        public async Task<ActionResult<IEnumerable<candidate>>> Getcandidates()
        {
            return await _context.candidates
                .Include(c => c.candidatevacancies)       // 1. Подключаем таблицу связей
                .ThenInclude(cv => cv.Vacancy)            // 2. Подключаем саму вакансию, чтобы получить её Title
                .ToListAsync();
        }

        // GET: api/Candidate/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<candidate>> Getcandidate(int id)
        {
            var candidate = await _context.candidates.FindAsync(id);

            if (candidate == null)
            {
                return NotFound();
            }

            return candidate;
        }

        // PUT: api/Candidate/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putcandidate(int id, candidate candidate)
        {
            if (id != candidate.CandidateId)
            {
                return BadRequest();
            }

            _context.Entry(candidate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!candidateExists(id))
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


// POST: api/Candidate
[HttpPost]
public async Task<ActionResult<candidate>> Postcandidate([FromBody] CandidateCreateDto dto)
{
    // 1. Проверка входных данных
    if (dto == null) return BadRequest("Данные не получены (dto is null)");
    if (!ModelState.IsValid) return BadRequest(ModelState);

    try
    {
        // --- ШАГ А: Создание и сохранение кандидата ---
        var newCandidate = new candidate
        {
            FullName = dto.FullName ?? "Не указано",
            Gender = dto.Gender,
            BirthDate = dto.BirthDate, // DateOnly
            Phone = dto.Phone ?? "",
            Email = dto.Email ?? "",
            EducationType = dto.EducationType,
            InstitutionName = dto.InstitutionName ?? "",
            WorkReadiness = dto.WorkReadiness,
            Status = dto.Status
        };

        _context.candidates.Add(newCandidate);
        await _context.SaveChangesAsync(); // База данных присваивает CandidateId

        if (newCandidate.CandidateId == 0)
        {
            return StatusCode(500, "Ошибка: База данных не вернула CandidateId после сохранения.");
        }

        // --- ШАГ Б: Привязка вакансий ---
        if (dto.Candidatevacancies != null && dto.Candidatevacancies.Any())
        {
            foreach (var vacancyName in dto.Candidatevacancies)
            {
                if (string.IsNullOrWhiteSpace(vacancyName)) continue;

                // Безопасный поиск: игнорируем регистр и проверяем, что Title не null
                var vacancy = await _context.vacancies
                    .FirstOrDefaultAsync(v => v.Title != null && 
                                              v.Title.ToLower().Contains(vacancyName.ToLower()));

                if (vacancy != null)
                {
                    var link = new candidatevacancy
                    {
                        CandidateId = newCandidate.CandidateId, // Реальный ID из базы
                        VacancyId = vacancy.VacancyId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.candidatevacancies.Add(link);
                }
                // Если vacancy == null, просто пропускаем (вакансия не найдена в БД)
            }
            
            // Сохраняем только связи
            await _context.SaveChangesAsync(); 
        }

        // Если всё успешно, возвращаем 200 OK и ID кандидата
        return Ok(new { success = true, candidateId = newCandidate.CandidateId });
    }
    catch (DbUpdateException dbEx)
    {
        // Ошибка при работе с базой данных (например, конфликт внешних ключей)
        return StatusCode(500, new { 
            step = "Ошибка базы данных (DbUpdateException)",
            message = dbEx.Message, 
            inner = dbEx.InnerException?.Message 
        });
    }
    catch (Exception ex)
    {
        // Любая другая непредвиденная ошибка
        return StatusCode(500, new { 
            step = "Непредвиденная ошибка сервера",
            message = ex.Message, 
            inner = ex.InnerException?.Message,
            stackTrace = ex.StackTrace
        });
    }
}

        // DELETE: api/Candidate/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Deletecandidate(int id)
        {
            var candidate = await _context.candidates.FindAsync(id);
            if (candidate == null)
            {
                return NotFound();
            }

            _context.candidates.Remove(candidate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool candidateExists(int id)
        {
            return _context.candidates.Any(e => e.CandidateId == id);
        }
        
        public class CreateCandidateRequest
        {
            public candidate Candidate { get; set; }
            public List<int> VacancyIds { get; set; } = new List<int>();
        }

        public class CandidateCreateDto
        {
            [JsonPropertyName("candidateId")]
            public int CandidateId { get; set; }

            [JsonPropertyName("fullName")]
            public string FullName { get; set; } = null!;

            [JsonPropertyName("gender")]
            public string? Gender { get; set; }

            [JsonPropertyName("birthDate")]
            public DateOnly BirthDate { get; set; } // <-- ИСПРАВЛЕНО: DateOnly вместо DateTime

            [JsonPropertyName("phone")]
            public string Phone { get; set; } = null!;

            [JsonPropertyName("email")]
            public string Email { get; set; } = null!;

            [JsonPropertyName("educationType")]
            public string? EducationType { get; set; }

            [JsonPropertyName("institutionName")]
            public string InstitutionName { get; set; } = null!;

            [JsonPropertyName("workReadiness")]
            public string? WorkReadiness { get; set; }

            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("candidatevacancies")]
            public List<string> Candidatevacancies { get; set; } = new List<string>();
        }
    }
}
