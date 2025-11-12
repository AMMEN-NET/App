using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmmenTravel.Common
{
    public interface IUserOwned
    {
        Guid UserId { get; set; }
        //Sirve para poder filtrar por usuario las entidades que implementen esta interfaz
    }
}
