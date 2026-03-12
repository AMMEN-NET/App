using System;
using System.Net;
using System.Text;

namespace AmmenTravel.Notificaciones
{
    /// <summary>
    /// Helper estático para generar plantillas HTML de emails de notificación.
    /// Centraliza el diseño para mantener consistencia visual en todos los emails.
    /// </summary>
    public static class EmailTemplateHelper
    {
        /// <summary>
        /// Genera un email HTML para una notificación individual (modo inmediato).
        /// </summary>
        public static string GenerarEmailNotificacion(string titulo, string mensaje, string? linkAccion = null, string textoBoton = "Ver en AmmenTravel")
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='es'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); padding: 30px; }");
            sb.AppendLine(".header { text-align: center; border-bottom: 2px solid #4CAF50; padding-bottom: 20px; margin-bottom: 20px; }");
            sb.AppendLine(".header h1 { color: #333; margin: 0; font-size: 24px; }");
            sb.AppendLine(".content { padding: 10px 0; }");
            sb.AppendLine(".content h2 { color: #333; font-size: 20px; margin-bottom: 10px; }");
            sb.AppendLine(".content p { color: #555; font-size: 16px; line-height: 1.6; }");
            sb.AppendLine(".cta-button { display: inline-block; background-color: #4CAF50; color: white !important; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 20px; font-size: 16px; }");
            sb.AppendLine(".footer { text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; color: #999; font-size: 12px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");

            // Header
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>🌍 AmmenTravel</h1>");
            sb.AppendLine("</div>");

            // Content
            sb.AppendLine("<div class='content'>");
            sb.AppendLine($"<h2>{Escape(titulo)}</h2>");
            sb.AppendLine($"<p>{Escape(mensaje)}</p>");
            sb.AppendLine("</div>");

            // CTA Button
            if (!string.IsNullOrEmpty(linkAccion))
            {
                var urlCompleta = linkAccion.StartsWith("http") ? linkAccion : $"https://ammentravel.com{linkAccion}";
                sb.AppendLine("<div style='text-align: center;'>");
                sb.AppendLine($"<a href='{Escape(urlCompleta)}' class='cta-button'>{Escape(textoBoton)}</a>");
                sb.AppendLine("</div>");
            }

            // Footer
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("<p>Recibiste este email porque tenés activadas las notificaciones por email en tu perfil de AmmenTravel.</p>");
            sb.AppendLine("<p>© 2026 AmmenTravel - Tu compañero de viajes</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        /// <summary>
        /// Genera un email HTML para notificaciones de logros/gamificación.
        /// </summary>
        public static string GenerarEmailLogro(string titulo, string mensaje)
        {
            return GenerarEmailNotificacion(titulo, mensaje, "/mi-perfil/mis-calificaciones", "Ver mi perfil");
        }

        /// <summary>
        /// Genera un email HTML para notificaciones de eventos en favoritos.
        /// </summary>
        public static string GenerarEmailEventoFavorito(string titulo, string mensaje)
        {
            return GenerarEmailNotificacion(titulo, mensaje, "/favoritos", "Ver mis favoritos");
        }

        /// <summary>
        /// Genera un email HTML para notificaciones de opiniones en destinos favoritos.
        /// </summary>
        public static string GenerarEmailOpinionFavorito(string titulo, string mensaje, Guid? destinoId = null)
        {
            var link = destinoId.HasValue ? $"/destinos/{destinoId.Value}" : "/favoritos";
            return GenerarEmailNotificacion(titulo, mensaje, link, "Ver destino");
        }

        /// <summary>
        /// Genera un email HTML para notificaciones de reactivación.
        /// </summary>
        public static string GenerarEmailReactivacion(string titulo, string mensaje)
        {
            return GenerarEmailNotificacion(titulo, mensaje, "/mi-perfil/mis-calificaciones", "Compartir una experiencia");
        }

        private static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return WebUtility.HtmlEncode(text);
        }
    }
}
