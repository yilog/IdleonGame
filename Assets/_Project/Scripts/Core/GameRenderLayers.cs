namespace IdleonGame.Core
{
    public static class GameRenderLayers
    {
        public static class SortingOrders
        {
            public const int TilemapBackground = -10;
            public const int TilemapGround = 0;
            public const int TilemapDecoration = 10;
            public const int TilemapCollision = 20;
            public const int TilemapMonsterSpawn = 30;
            public const int Monster = 45;
            public const int Player = 50;
            public const int Projectile = 55;
            public const int WorldItem = 60;
            public const int SceneMarker = 70;
            public const int PortalRequirement = 75;
        }
    }
}
