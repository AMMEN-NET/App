using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using AmmenTravel.DestinosDTO;
using AmmenTravel.InterfaceDestinoAppService;

namespace AmmenTravel.Destinos;

public class DestinoAppService :
    CrudAppService<
        DestinoTuristico, //The Destino entity
        guardarDestinoDTO, //Used to show destinos
        Guid, //Primary key of the destino entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateDestinoDTO>, //Used to create/update a destino
    IDestinoAppService //implement the IDestinoAppService
{
    public DestinoAppService(IRepository<DestinoTuristico, Guid> repository)
        : base(repository)
    {

    }
}