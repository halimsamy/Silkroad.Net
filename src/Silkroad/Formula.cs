namespace Silkroad;

/// <summary>
///     Implements different formulas for different purposes, Reversed from Silkroad GameServer.
///     Functions here are self-documented due to their very meaningful names, so it's silly re-doc them.
///     The map-related functions only works after Alexandria update, because the map was expended back then.
///     If you want to use those map-related functions on a very old version of Silkroad before Alexandria update,
///     you would have to update the constants used in this class, but the formulas would still be the same.
/// </summary>
public static class Formula {
    private const short RegionScale = 256;
    private const byte RegionHeight = 192;
    private const byte RegionWidth = 192;
    private const byte WorldScale = 10;
    private const byte XStartSector = 135;
    private const byte YStartSector = 92;

    public static bool IsInSightRegion(short r1, short r2) {
        /*
            *************************
            *       *       *       *
            * +127	* +128  *  +129 *
            *       *		*	    *
            *************************
            *       *		*	    *
            *  -1	*   0	*   +1  *
            *       *		*	    *
            *************************
            *       *		*	    *
            * -129	* -128	*  -127 *
            *       *       *       *
            *************************
        */
        var regionGap = Math.Abs(r1 - r2);
        return regionGap is >= RegionScale - 1 and <= RegionScale + 1 or 1 or 0;
    }

    public static float Angle(float y1, float y2, float x1, float x2) {
        return (float)(Math.Atan2(y2 - y1, x2 - x1) * 180 / Math.PI * 182);
    }

    #region Stats

    public static int BaseDefByStats(int stats) {
        return (int)(0.4f * stats);
    }

    public static int BaseMaxAtkByStats(int stats) {
        return (int)(0.65f * stats);
    }

    public static int BaseMaxHealthManaByStats(byte level, int stats) {
        return (int)(Math.Pow(1.02f, level - 1) * stats * 10);
    }

    public static int BaseMinAtkByStats(int stats) {
        return (int)(0.45f * stats);
    }

    public static int MagBalance(byte level, int stats) {
        return (int)(100f * stats / (28f + level * 4f));
    }

    public static int PhyBalance(byte level, int stats) {
        return (int)(100f - 100f * 2f / 3f * (28f + level * 4f - stats) / (28f + level * 4f));
    }

    #endregion

    #region Region

    public static short GetRegion(byte sectorX, byte sectorY) {
        return (short)(sectorY * RegionScale + sectorX);
    }

    public static short GetRegion(float worldX, float worldY) {
        return GetRegion(GetSectorX(worldX), GetSectorY(worldY));
    }

    #endregion

    #region SectorX

    public static byte GetSectorX(short region) {
        return (byte)(region % RegionScale);
    }

    public static byte GetSectorX(float worldX) {
        return (byte)Math.Floor((double)(worldX / RegionHeight) + XStartSector);
    }

    #endregion

    #region SectorY

    public static byte GetSectorY(short region) {
        return (byte)((region - GetSectorX(region)) / RegionScale);
    }

    public static byte GetSectorY(float worldY) {
        return (byte)Math.Floor((double)(worldY / RegionWidth) + YStartSector);
    }

    #endregion

    #region OffsetX

    public static float GetOffsetX(byte sectorX, float worldX) {
        return (worldX - (sectorX - XStartSector) * RegionHeight) * WorldScale;
    }

    public static float GetOffsetX(short region, float worldX) {
        return (worldX - (GetSectorX(region) - XStartSector) * RegionHeight) * WorldScale;
    }

    public static float GetOffsetX(float worldX) {
        return (float)Math.Round(
            (worldX / RegionHeight - GetSectorX(worldX) + XStartSector) * RegionHeight * WorldScale);
    }

    #endregion

    #region OffsetY

    public static float GetOffsetY(byte sectorY, float worldY) {
        return (worldY - (sectorY - YStartSector) * RegionWidth) * WorldScale;
    }

    public static float GetOffsetY(short region, float worldY) {
        return (worldY - (GetSectorY(region) - YStartSector) * RegionWidth) * WorldScale;
    }

    public static float GetOffsetY(float worldY) {
        return (float)Math.Round((worldY / RegionWidth - GetSectorY(worldY) + YStartSector) * RegionWidth * WorldScale);
    }

    #endregion

    #region WorldX

    public static float GetWorldX(short region, float offsetX) {
        return (GetSectorX(region) - XStartSector) * RegionHeight + offsetX / WorldScale;
    }

    public static float GetWorldX(byte sectorX, float offsetX) {
        return (sectorX - XStartSector) * RegionHeight + offsetX / WorldScale;
    }

    #endregion

    #region WorldY

    public static float GetWorldY(short region, float offsetY) {
        return (GetSectorY(region) - YStartSector) * RegionWidth + offsetY / WorldScale;
    }

    public static float GetWorldY(byte sectorY, float offsetY) {
        return (sectorY - YStartSector) * RegionWidth + offsetY / WorldScale;
    }

    #endregion
}