using System;
using System.Collections.Generic;
using System.Linq;

namespace Appointment_System.Models
{
    public class TemplateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DayDTO> DaySchedules { get; set; }

        public class DayDTO
        {
            public string Id { get; set; }
            public int DayIndex { get; set; }
            public List<TimeRangeDTO> TimeRanges { get; set; }
        }

        public class TimeRangeDTO
        {
            public string Id { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public bool Selected { get; set; }
        }
    }

    public static class TemplateDTOExtensions
    {
        public static Template ToTemplate(this TemplateDTO dto, string providerId)
        {
            var template = new Template
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                ProviderId = providerId,
                Type = false, // Slot-based model
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Days = dto.DaySchedules?.Select(d => d.ToDay()).ToList()
            };

            return template;
        }

        public static TemplateDTO ToTemplateDTO(this Template template)
        {
            return new TemplateDTO
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                DaySchedules = template.Days?.Select(d => d.ToDayDTO()).ToList() ?? new List<TemplateDTO.DayDTO>()
            };
        }

        public static Day ToDay(this TemplateDTO.DayDTO dto)
        {
            var day = new Day
            {
                Index = dto.DayIndex,
                IsAvailable = dto.TimeRanges?.Any() == true,
                Segments = dto.TimeRanges?.Select(tr => tr.ToSegment()).ToList()
            };

            return day;
        }

        public static TemplateDTO.DayDTO ToDayDTO(this Day day)
        {
            return new TemplateDTO.DayDTO
            {
                Id = day.Id.ToString(),
                DayIndex = day.Index,
                TimeRanges = day.Segments?.Select(s => s.ToTimeRangeDTO()).ToList() ?? new List<TemplateDTO.TimeRangeDTO>()
            };
        }

        public static Segment ToSegment(this TemplateDTO.TimeRangeDTO dto)
        {
            // Parse time strings to TimeOnly
            TimeOnly startTime = TimeOnly.Parse(dto.StartTime);
            TimeOnly endTime = TimeOnly.Parse(dto.EndTime);
            
            var segment = new Segment
            {
                StartTime = startTime,
                EndTime = endTime,
                // Calculate duration based on start and end times
                DurationForSingleSlot = endTime.ToTimeSpan() - startTime.ToTimeSpan()
            };

            return segment;
        }

        public static TemplateDTO.TimeRangeDTO ToTimeRangeDTO(this Segment segment)
        {
            return new TemplateDTO.TimeRangeDTO
            {
                Id = segment.Id.ToString(),
                StartTime = segment.StartTime.ToString("HH:mm"),
                EndTime = segment.EndTime.ToString("HH:mm"),
            };
        }
    }
}
