using System;

namespace Catalog.API.Dtos.Response
{
    public class HealthReportResponseDto
    {
        public Boolean Status { get; }
        public TimeSpan TotalDuration { get; }
    }
}
