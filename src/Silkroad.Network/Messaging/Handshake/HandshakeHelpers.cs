namespace Silkroad.Network.Messaging.Handshake;

/// <summary>
///     Implement some helpers for the Handshake process.
/// </summary>
public static class HandshakeHelpers {
    public static uint PowMod(uint g, uint x, uint p) {
        long r = 1;
        long m = g;

        if (x == 0) return 1;

        while (x != 0) {
            if ((x & 1) > 0) r = m * r % p;

            x >>= 1;
            m = m * m % p;
        }

        return (uint)r;
    }

    public static void KeyTransformValue(Span<byte> value, uint key, byte keyByte) {
        value[0] ^= (byte)(value[0] + ((key >> 00) & byte.MaxValue) + keyByte);
        value[1] ^= (byte)(value[1] + ((key >> 08) & byte.MaxValue) + keyByte);
        value[2] ^= (byte)(value[2] + ((key >> 16) & byte.MaxValue) + keyByte);
        value[3] ^= (byte)(value[3] + ((key >> 24) & byte.MaxValue) + keyByte);

        value[4] ^= (byte)(value[4] + ((key >> 00) & byte.MaxValue) + keyByte);
        value[5] ^= (byte)(value[5] + ((key >> 08) & byte.MaxValue) + keyByte);
        value[6] ^= (byte)(value[6] + ((key >> 16) & byte.MaxValue) + keyByte);
        value[7] ^= (byte)(value[7] + ((key >> 24) & byte.MaxValue) + keyByte);
    }

    public static byte[] GetKey(uint a, uint b) {
        var x = ((ulong)b << 32) | a;
        var bytes = BitConverter.GetBytes(x);
        return BitConverter.IsLittleEndian ? bytes : bytes.Reverse().ToArray();
    }
}