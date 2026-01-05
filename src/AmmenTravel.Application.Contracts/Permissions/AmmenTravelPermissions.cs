namespace AmmenTravel.Permissions
{
    public static class AmmenTravelPermissions
    {
        public const string GroupName = "AmmenTravel";
        public static class Destinos
        {
            public const string Default = GroupName + ".Destinos";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }

        // Aquí pueden estar tus otros permisos (ej: Favoritos, etc.)
    }
}