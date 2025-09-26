using AmmenTravel.DestinosDTO;
using System;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;

namespace AmmenTravel.InterfaceDestinoAppService;
    internal interface IDestinoAppService:

        ICrudAppService< //Defines CRUD methods
        guardarDestinoDTO, //Used to show books
        Guid, //Primary key of the book entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateDestinoDTO> //Used to create/update a book
    {

    }
    
