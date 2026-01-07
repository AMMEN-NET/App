using System;
using Volo.Abp.Application.Dtos;

namespace AmmenTravel.Viajeros
{
    public class ViajeroDto : EntityDto<Guid>
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}