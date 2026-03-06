using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmmenTravel.ExternalService
{
    public class EventoTicketmasterDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string UrlTicket { get; set; }
        public DateTime FechaInicio { get; set; }
        public string ImagenUrl { get; set; }
    }
}