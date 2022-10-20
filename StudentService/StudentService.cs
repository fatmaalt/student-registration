using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
//using AutoMapper;

//using dotnet_rpg.Data;
//using dotnet_rpg.Dtos.Character;
//using dotnet_rpg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using student_registration.Data;
using student_registration.Dtos.Student;
using student_registration.Models;
using student_registration.Models;
//using StudentRegistrationAPI.Dtos.Student;
//using Microsoft.EntityFrameworkCore;
namespace student_registration.Services.StudentService
{
    public class StudentService : IStudentService
    {
        private static List<Studentt> students = new List<Studentt>{
          new Studentt(),
          new Studentt{Id =2, FirstName="fatma"}
        };

        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StudentService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _mapper = mapper;
        }
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        private String GetUserRole() => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);

        public async Task<ServiceResponse<List<GetStudentDto>>> AddStudent(AddStudentDto newStudent)
        {
            var serviceResponse = new ServiceResponse<List<GetStudentDto>>();
            Studentt studentt = _mapper.Map<Studentt>(newStudent);
            studentt.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
            _context.Students.Add(studentt);
            await _context.SaveChangesAsync();
            serviceResponse.Data = await _context.Students
            .Where(c => c.User.Id == GetUserId())
            .Select(c => _mapper.Map<GetStudentDto>(c))
            .ToListAsync();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetStudentDto>>> GetAllStudents()
        {
            var response = new ServiceResponse<List<GetStudentDto>>();
            var dbStudents = GetUserRole().Equals("Admin")?
            await _context.Students.ToListAsync():
            await _context.Students.Where(c => c.User.Id == GetUserId())
            .ToListAsync();
            response.Data = dbStudents.Select(c => _mapper.Map<GetStudentDto>(c)).ToList();

            return response;
        }

        public async Task<ServiceResponse<GetStudentDto>> GetStudentById(int id)
        {
            var serviceResponse = new ServiceResponse<GetStudentDto>();
            var dbStudent = await _context.Students.FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());
            serviceResponse.Data = _mapper.Map<GetStudentDto>(dbStudent);

            return serviceResponse;

        }

        public async Task<ServiceResponse<GetStudentDto>> UpdateStudent(UpdateStudentDto updatedStudent)
        {
            ServiceResponse<GetStudentDto> serviceResponse = new ServiceResponse<GetStudentDto>();

            try
            {

                Studentt student = await _context.Students
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == updatedStudent.Id);
              
                if (student.User.Id == GetUserId())
                {

                    student.FirstName = updatedStudent.FirstName;
                    student.LastName = updatedStudent.LastName;
                    student.DoB = updatedStudent.DoB;
                    student.EducationLevel = updatedStudent.EducationLevel;
                    student.Specialization = updatedStudent.Specialization;
                    student.UniversityName = updatedStudent.UniversityName;

                    await _context.SaveChangesAsync();

                    serviceResponse.Data = _mapper.Map<GetStudentDto>(student);
                }
                else
                {
                    serviceResponse.Sucess = false;
                    serviceResponse.Message = "Student not found";
                }
            }

            catch (Exception ex)
            {
                serviceResponse.Sucess = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;

        }




        public async Task<ServiceResponse<List<GetStudentDto>>> DeleteStudent(int id)
        {
            ServiceResponse<List<GetStudentDto>> serviceResponse = new ServiceResponse<List<GetStudentDto>>();
            try
            {
                Studentt student = await _context.Students
                    .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());
                if (student != null)
                {
                    _context.Students.Remove(student);
                    await _context.SaveChangesAsync();
                    serviceResponse.Data = (_context.Students
                    .Where(c => c.User.Id == GetUserId())
                        .Select(c => _mapper.Map<GetStudentDto>(c))).ToList();
                    serviceResponse.Message = "Student found.";
                }
                else
                {
                    serviceResponse.Sucess = false;
                    serviceResponse.Message = "Student not found.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Sucess = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
    }
}
