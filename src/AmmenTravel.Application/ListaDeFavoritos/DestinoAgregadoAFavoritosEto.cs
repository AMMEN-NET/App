using System;

namespace AmmenTravel.ListaDeFavoritos
{
    // ETO = Event Transfer Object
    public class DestinoAgregadoAFavoritosEto
    {
        public Guid UserId { get; set; }
        public Guid DestinoId { get; set; }
    }
}