using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

namespace TestKyzKadr
{
    internal class Program
    {
        public static void Main()
        {
            var teachers = ReadTeachers("C:\\Development\\С#\\TestKyzKadr\\TestKyzKadr\\Data\\Учителя.txt");
            var students = ReadStudents("C:\\Development\\С#\\TestKyzKadr\\TestKyzKadr\\Data\\Ученики.txt");
            var exams = new List<Exams>(); // Предполагается, что записи уже есть
            //Так как в exams предполагаются записи то можно по экзаменам определить колличество учеников у учителя, надеюсь правильно понял

            /* 1
            Ошибки: Не правильная нумерация в enum исправил на (1,2) 
            изменил названия предметов в enum что бы они соответствовали данным в файле ученики
            так же исправил резделители в файлах с данными для удобства считывания данных на формат CSV
            */
            var TeacherWFS = FindTeacherWithFewestStudents(exams, teachers); //2
            double averagePhysicsScore = CalculateAveragePhysicsScore(exams); //3
            int highScoringStudents = CountHighScoringMathStudents(exams, teachers); //4
            var teacherWithSecondMostStudents = FindSecondMostStudentsTeacher(exams, teachers); //5
            if (teacherWithSecondMostStudents != null)
                Console.WriteLine($"Учитель с вторым по величине количеством учеников: {teacherWithSecondMostStudents.Name} {teacherWithSecondMostStudents.LastName}");
            else
                Console.WriteLine("Недостаточно данных для определения учителя с вторым по величине количеством учеников.");
        }

        public static Teacher FindSecondMostStudentsTeacher(List<Exams> exams, List<Teacher> teachers)
        {
            if (exams == null || !exams.Any() || teachers == null || !teachers.Any())
                return null; // Возвращаем null, если списки пусты

            // Группировка экзаменов по TeacherId и подсчёт уникальных StudentId для каждого учителя
            var teacherStudentCounts = exams
                .GroupBy(exam => exam.TeacherId)
                .Select(group => new
                {
                    TeacherId = group.Key,
                    StudentCount = group.Select(x => x.StudentId).Distinct().Count()
                })
                .OrderByDescending(x => x.StudentCount) 
                .ToList();

            if (teacherStudentCounts.Count < 2)
                return null; 

            var secondMostStudentsTeacherId = teacherStudentCounts[1].TeacherId;

            return teachers.FirstOrDefault(t => t.ID == secondMostStudentsTeacherId);
        }

        public static int CountHighScoringMathStudents(List<Exams> exams, List<Teacher> teachers)
        {
            if (exams == null || !exams.Any() || teachers == null || !teachers.Any())
                return 0;

            // Фильтрация учителей по имени Alex
            var teacherIds = teachers
                .Where(teacher => teacher.Name.Equals("Alex", StringComparison.OrdinalIgnoreCase))
                .Select(teacher => teacher.ID)
                .ToList();

            if (!teacherIds.Any())
                return 0;

            // Фильтрация экзаменов по предмету математика, баллам более 90 и ID учителя
            int studentCount = exams
                .Where(exam => exam.Lesson == LessonType.Математика &&
                               exam.Score > 90 &&
                               teacherIds.Contains(exam.TeacherId))
                .Select(exam => exam.StudentId)
                .Distinct()
                .Count();

            return studentCount;
        }

        public static double CalculateAveragePhysicsScore(List<Exams> exams)
        {
            if (exams == null || !exams.Any())
                return 0;

            var filteredExams = exams
                .Where(exam => exam.Lesson == LessonType.Физика && exam.ExamDate.Year == 2023)
                .ToList();

            if (!filteredExams.Any())
                return 0;

            double averageScore = Convert.ToDouble(filteredExams.Average(exam => exam.Score));
            return averageScore;
        }

        private static Teacher FindTeacherWithFewestStudents(List<Exams> exams, List<Teacher> teachers)
        {
            // Группировка по TeacherId и подсчёт уникальных StudentId для каждого учителя
            var teacherStudentCount = exams
                .GroupBy(exam => exam.TeacherId)
                .Select(group => new
                {
                    TeacherId = group.Key,
                    UniqueStudents = group.Select(x => x.StudentId).Distinct().Count()
                })
                .ToList();

            if (!teacherStudentCount.Any())
                return null; // Возвращаем null, если ни один учитель не найден (просто что бы код ошибки не выдавал)

            var minStudentCount = teacherStudentCount?.Min(x => x.UniqueStudents);

            var teacherWithFewestStudentsId = teacherStudentCount
                .FirstOrDefault(x => x.UniqueStudents == minStudentCount)?.TeacherId;

            Teacher teacherWithFewestStudents = teachers.FirstOrDefault(t => t.ID == teacherWithFewestStudentsId);

            return teacherWithFewestStudents;
        }

        private static List<Teacher> ReadTeachers(string filePath)
        {
            var teachers = new List<Teacher>();
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length >= 4)
                {
                    teachers.Add(new Teacher
                    {
                        Name = parts[0].Trim(),
                        LastName = parts[1].Trim(),
                        Age = int.Parse(parts[2].Trim()),
                        Lesson = (LessonType)Enum.Parse(typeof(LessonType), parts[3].Trim())
                    });
                }
            }
            return teachers;
        }

        private static List<Student> ReadStudents(string filePath)
        {
            var students = new List<Student>();
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    students.Add(new Student
                    {
                        Name = parts[0].Trim(),
                        LastName = parts[1].Trim(),
                        Age = int.Parse(parts[2].Trim())
                    });
                }
            }
            return students;
        }

        public class Person
        {
            public long ID { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            public int Age { get; set; }
        }

        public class Teacher : Person
        {
            public LessonType Lesson { get; set; }
        }

        public class Student : Person
        {
        }

        public class Exams
        {
            public LessonType Lesson { get; set; }
            public long StudentId { get; set; }
            public long TeacherId { get; set; }
            public decimal Score { get; set; }
            public DateTime ExamDate { get; set; }
            public Student Student { get; set; }
            public Teacher Teacher { get; set; }
        }

        public enum LessonType
        {
            Математика = 1,
            Физика = 2
        }
    }
}
