namespace Silkroad.Network.Messaging;

/// <summary>
///     Implements a number Sequence generator for Silkroad network messages.
///     This was made to ensure that a <see cref="Message" /> is not injected by a third-party.
/// </summary>
internal sealed class MessageSequence {
    /// <summary>
    ///     The default seed.
    /// </summary>
    private const uint DefaultSeed = 0x9ABFB3B6;

    /// <summary>
    ///     The first byte seed.
    /// </summary>
    private readonly byte _byte1;

    /// <summary>
    ///     The second byte seed.
    /// </summary>
    private readonly byte _byte2;

    /// <summary>
    ///     The primary byte seed, this is modified after every number generation
    ///     to ensure the next number is different.
    /// </summary>
    private byte _byte0;

    /// <summary>
    ///     Initializes a sequence with a specific seed.
    /// </summary>
    /// <param name="seed">The seed.</param>
    public MessageSequence(uint seed) {
        var mut0 = seed != 0 ? seed : DefaultSeed;
        var mut1 = GenerateValue(ref mut0);
        var mut2 = GenerateValue(ref mut0);
        var mut3 = GenerateValue(ref mut0);
        GenerateValue(ref mut0);

        this._byte1 = (byte)((mut1 & byte.MaxValue) ^ (mut2 & byte.MaxValue));
        if (this._byte1 == 0) this._byte1 = 1;

        this._byte2 = (byte)((mut0 & byte.MaxValue) ^ (mut3 & byte.MaxValue));
        if (this._byte2 == 0) this._byte2 = 1;

        this._byte0 = (byte)(this._byte2 ^ this._byte1);
    }

    /// <summary>
    ///     Generates the next number in the sequence.
    /// </summary>
    /// <returns>The next number in the sequence.</returns>
    public byte Next() {
        var value = (byte)(this._byte2 * (~this._byte0 + this._byte1));
        return this._byte0 = (byte)(value ^ (value >> 4));
    }


    private static uint GenerateValue(ref uint value) {
        for (var i = 0; i < 32; i++) {
            var v = value;
            v = (v >> 2) ^ value;
            v = (v >> 2) ^ value;
            v = (v >> 1) ^ value;
            v = (v >> 1) ^ value;
            v = (v >> 1) ^ value;
            value = (((value >> 1) | (value << 31)) & ~1u) | (v & 1);
        }

        return value;
    }
}