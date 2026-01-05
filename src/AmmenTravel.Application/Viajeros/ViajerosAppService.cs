using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace AmmenTravel.Viajeros
{
    public class ViajerosAppService : AmmenTravelAppService, IViajerosAppService
    {
        private readonly IIdentityUserRepository _userRepository;

        public ViajerosAppService(IIdentityUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<ViajeroDto>> GetListAsync(string filtro = null)
        {
            var usuarios = await _userRepository.GetListAsync(
                sorting: "UserName",
                maxResultCount: 50,
                filter: filtro
            );

            return usuarios.Select(u => new ViajeroDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Name = u.Name,
                Surname = u.Surname
            }).ToList();
        }
    }
}