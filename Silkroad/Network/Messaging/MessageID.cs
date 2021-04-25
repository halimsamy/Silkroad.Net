using System;
using System.Runtime.InteropServices;

namespace Silkroad.Network.Messaging {
    /// <summary>
    ///     Specify a unique <see cref="Message" /> ID, used to identify messages.
    ///     This acts like HTTP response codes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = sizeof(ushort))]
    // ReSharper disable once InconsistentNaming
    public struct MessageID : IEquatable<MessageID> {
        /// <summary>
        ///     Initializes a MessageID with a full specific ID (a.k.a Opcode).
        /// </summary>
        /// <param name="value">The ID value.</param>
        public MessageID(ushort value) {
            this.Value = value;
        }

        /// <summary>
        ///     Initializes a MessageID with it's raw parts.
        /// </summary>
        /// <param name="dir">The message direction.</param>
        /// <param name="type">The message type.</param>
        /// <param name="op">The message operation code.</param>
        public MessageID(MessageDirection dir, MessageType type, ushort op) {
            this.Value = 0;
            this.Direction = dir;
            this.Type = type;
            this.Operation = op;
        }

        /// <summary>
        ///     The full value of the ID (a.k.a. Opcode).
        /// </summary>
        public ushort Value { get; set; }

        /// <summary>
        ///     The direction of the <see cref="Message" />.
        /// </summary>
        public MessageDirection Direction {
            get => (MessageDirection) ((this.Value & DirectionMask) >> DirectionOffset);
            set => this.Value = (ushort) ((this.Value & ~DirectionMask) |
                                          ((Convert.ToByte(value) << DirectionOffset) & DirectionMask));
        }

        /// <summary>
        ///     The type of the <see cref="Message" />.
        /// </summary>
        public MessageType Type {
            get => (MessageType) ((this.Value & TypeMask) >> TypeOffset);
            set => this.Value =
                (ushort) ((this.Value & ~TypeMask) | ((Convert.ToByte(value) << TypeOffset) & TypeMask));
        }

        /// <summary>
        ///     The operation of the <see cref="Message" />.
        /// </summary>
        public ushort Operation {
            // TODO: enum this value.
            get => (ushort) ((this.Value & OperationMask) >> OperationOffset);
            set => this.Value = (ushort) ((this.Value & ~OperationMask) | ((value << OperationOffset) & OperationMask));
        }


        public bool Equals(MessageID other) {
            return this.Value == other.Value;
        }

        public override string ToString() {
            return $"[{this.Value:X4}] [{this.Direction}] [{this.Type}] [{this.Operation:X4}]";
        }

        public override bool Equals(object obj) {
            return obj is MessageID other && this.Equals(other);
        }

        public override int GetHashCode() {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return this.Value.GetHashCode();
        }

        #region Bit Masking

        // (MSB)                                                                       (LSB)
        // | 15 | 14 | 13 | 12 | 11 | 10 | 09 | 08 | 07 | 06 | 05 | 04 | 03 | 02 | 01 | 00 |
        // |   DIR   |   TYPE  |                       OPERATION                           |

        private const int OperationSize = 12;
        private const int OperationOffset = 0;
        private const ushort OperationMask = ((1 << OperationSize) - 1) << OperationOffset;

        private const int TypeSize = 2;
        private const int TypeOffset = OperationOffset + OperationSize;
        private const ushort TypeMask = ((1 << TypeSize) - 1) << TypeOffset;

        private const int DirectionSize = 2;
        private const int DirectionOffset = TypeOffset + TypeSize;
        private const ushort DirectionMask = ((1 << DirectionSize) - 1) << DirectionOffset;

        #endregion Bit Masking

        #region Known MessageIDs

        // A list of <see cref="MessageID"/>s values (a.k.a. Opcodes).
        //  ReSharper disable InconsistentNaming

        public const ushort
            HANDSHAKE = 0x5000,
            HANDSHAKE_ACCEPT = 0x9000,
            IDENTITY = 0x2001,
            HEARTBEAT = 0x2002,
            MODULE_CERTIFICATION_REQUEST = 0x6003,
            MODULE_CERTIFICATION_RESPONSE = 0xA003,
            MODULE_RELAY_REQUEST = 0x6008,
            MODULE_RELAY_RESPONSE = 0xA008,
            NODE_STATUS1 = 0x2005,
            NODE_STATUS2 = 0x6005,
            MASSIVE = 0x600D;

        public static class UNKNOWN {
            public const ushort
                UNKNOWN_4 = 0x6130, //  UNKNOWN
                UNKNOWN_5 = 0xA130, //  UNKNOWN
                UNKNOWN_6 = 0x6110, //  <C->AS> [ECSRO]
                UNKNOWN_7 = 0x34F2, //  <AS->C>
                UNKNOWN_8 = 0x9001; //  <C->S> HARDWARE ID PACKET OPCODE FOR MOST PRIVSRO GUARDS.
        }

        public static class Download {
            public static class Request {
                public const ushort
                    FILE_REQUEST = 0x6004,
                    FILE_COMPLETE = 0xA004;
            }

            public static class Response {
                public const ushort
                    FILE_CHUNK = 0x1001;
            }
        }

        public static class Gateway {
            public static class Request {
                public const ushort
                    CHECKVERSION = 0x6000,
                    PATCH = 0x6100,
                    NEWS = 0x6104,
                    SERVERLIST = 0x6101,
                    SERVERLIST_PING = 0x6106,
                    SERVERLIST_PING2 = 0x6107,
                    LOGIN = 0x6102,
                    LOGIN2 = 0x610A,
                    PASSCODE = 0x6117,
                    LOGIN_IBUV = 0x6323,
                    LOGIN_IBUV_CHALLENGE = 0x2322;
            }

            public static class Response {
                public const ushort
                    CHECKVERSION = 0x6000,
                    PATCH = 0xA100,
                    SERVERLIST = 0xA101,
                    SERVERLIST_PING = 0xA106,
                    SERVERLIST_PING2 = 0xA107,
                    LOGIN = 0xA102,
                    LOGIN2 = 0x2116,
                    PASSCODE = 0xA117,
                    AGENT_AUTH = 0xA10A,
                    QUEUE_POSITION = 0x210E,
                    LOGIN_IBUV_CONFIRM = 0xA323,
                    LOGIN_IBUV_CHALLENGE = 0x2322,
                    NEWS = 0xA104;
            }
        }

        public static class Agent {
            public static class Request {
                public const ushort
                    // TEMP
                    CHAR_MOVEMENT = 0x7021,
                    CHAR_MOVEMENT_ANGLE = 0x7024,
                    CHAR_ACTION = 0x7074,

                    // ACADEMY
                    ACADEMY_CREATE = 0x7470,
                    ACADEMY_DISBAND = 0x7471,
                    ACADEMY_KICK = 0x7473,
                    ACADEMY_LEAVE = 0x7474,
                    ACADEMY_GRADE = 0x7475,
                    ACADEMY_UPDATE_COMMENT = 0x7477,
                    ACADEMY_HONOR_RANK = 0x7478,
                    ACADEMY_MATCHING_REGISTER = 0x747A,
                    ACADEMY_MATCHING_CHANGE = 0x747B,
                    ACADEMY_MATCHING_DELETE = 0x747C,
                    ACADEMY_MATCHING_LIST = 0x747D,
                    ACADEMY_MATCHING_JOIN = 0x747E,
                    ACADEMY_MATCHING_RESPONSE = 0x347F,
                    ACADEMY_UNKNOWN1 = 0x7472,
                    ACADEMY_UNKNOWN2 = 0x7476,
                    ACADEMY_UNKNOWN3 = 0x7483,

                    // ALCHEMY
                    ALCHEMY_REINFORCE = 0x7150,
                    ALCHEMY_ENCHANT = 0x7151,
                    ALCHEMY_MANUFACTURE = 0x7155,
                    ALCHEMY_DISMANTLE = 0x7157,
                    ALCHEMY_SOCKET = 0x716A,

                    // AUTHENTICATION
                    AUTH = 0x6103,

                    // BATTLEARENA
                    BATTLEARENA_REQUEST = 0x74D3,

                    // CAS
                    CAS_CLIENT = 0x6314,
                    CAS_SERVER_RESPONSE = 0x6316,

                    // CHARACTER
                    CHARACTER_SELECTION_JOIN = 0x7001,
                    CHARACTER_SELECTION_ACTION = 0x7007,
                    CHARACTER_SELECTION_RENAME = 0x7450,

                    // CHAT
                    CHAT = 0x7025,

                    // COMMUNITY
                    COMMUNITY_FRIEND_ADD = 0x7302,
                    COMMUNITY_FRIEND_RESPONSE = 0x3303,
                    COMMUNITY_FRIEND_DELETE = 0x7304,
                    COMMUNITY_MEMO_OPEN = 0x7308,
                    COMMUNITY_MEMO_SEND = 0x7309,
                    COMMUNITY_MEMO_DELETE = 0x730A,
                    COMMUNITY_MEMO_LIST = 0x730B,
                    COMMUNITY_MEMO_SEND_GROUP = 0x730C,
                    COMMUNITY_BLOCK = 0x730D,

                    // CONFIG
                    CONFIG_UPDATE = 0x7158,

                    // CONSIGNMENT
                    CONSIGNMENT_DETAIL = 0x7506,
                    CONSIGNMENT_CLOSE = 0x7507,
                    CONSIGNMENT_REGISTER = 0x7508,
                    CONSIGNMENT_UNREGISTER = 0x7509,
                    CONSIGNMENT_BUY = 0x750A,
                    CONSIGNMENT_SETTLE = 0x750B,
                    CONSIGNMENT_SEARCH = 0x750C,
                    CONSIGNMENT_LIST = 0x750E,

                    // COS
                    COS_COMMAND = 0x70C5,
                    COS_TERMINATE = 0x70C6,
                    COS_UPDATE_RIDESTATE = 0x70CB,
                    COS_UNSUMMON = 0x7116,
                    COS_NAME = 0x7117,
                    COS_UPDATE_SETTINGS = 0x7420,
                    COS_UNKNOWN1 = 0x70C7,

                    // ENTITY - IN [W.I.P] ?
                    ENTITY_SELECT_OBJECT = 0x7045,
                    ENTITY_NPC_OPEN = 0x7046,
                    ENTITY_NPC_CLOSE = 0x704B,

                    // ENVIRONMENT -- ONLY SERVER

                    // EXCHANGE
                    EXCHANGE_START = 0x7081,
                    EXCHANGE_CONFIRM = 0x7082,
                    EXCHANGE_APPROVE = 0x7083,
                    EXCHANGE_CANCEL = 0x7084,

                    // FGW
                    FGW_RECALL_LIST = 0x7519,
                    FGW_RECALL_MEMBER = 0x751A,
                    FGW_RECALL_RESPONSE = 0x751C,
                    FGW_EXIT = 0x751D,

                    // CTF
                    FLAGWAR_REGISTER = 0x74B2,

                    // FRPVP
                    FRPVP_UPDATE = 0x7516,

                    // GAME
                    GAME_READY = 0x3012,
                    GAME_INVITE = 0x3080,
                    GAME_RESET_COMPLETE = 0x35B6,

                    // GUIDE
                    GUIDE = 0x70EA,

                    // GUILD
                    GUILD_CREATE = 0x70F0,
                    GUILD_DISBAND = 0x70F1,
                    GUILD_LEAVE = 0x70F2,
                    GUILD_INVITE = 0x70F3,
                    GUILD_KICK = 0x70F4,
                    GUILD_DONATE_OBSOLETE = 0x70F6,
                    GUILD_UPDATE_NOTICE = 0x70F9,
                    GUILD_PROMOTE = 0x70FA,
                    GUILD_UNION_INVITE = 0x70FB,
                    GUILD_UNION_LEAVE = 0x70FC,
                    GUILD_UNION_KICK = 0x70FD,
                    GUILD_UPDATE_SIEGE_AUTH = 0x70FF,
                    GUILD_TRANSFER = 0x7103,
                    GUILD_UPDATE_PERMISSION = 0x7104,
                    GUILD_ELECTION_START = 0x7105,
                    GUILD_ELECTION_PARTICIPATE = 0x7106,
                    GUILD_ELECTION_VOTE = 0x7107,
                    GUILD_WAR_START = 0x7110,
                    GUILD_WAR_END = 0x7112,
                    GUILD_WAR_REWARD = 0x7114,
                    GUILD_STORAGE_OPEN = 0x7250,
                    GUILD_STORAGE_CLOSE = 0x7251,
                    GUILD_STORAGE_LIST = 0x7252,
                    GUILD_UPDATE_NICKNAME = 0x7256,
                    GUILD_DONATE = 0x7258,
                    GUILD_MERCENARY_ATTR = 0x7259,
                    GUILD_MERCENARY_TERMINATE = 0x725A,
                    GUILD_GP_HISTORY = 0x7501,

                    // INVENTORY
                    INVENTORY_OPERATION = 0x7034,
                    INVENTORY_STORAGE_OPEN = 0x703C,
                    INVENTORY_ITEM_REPAIR = 0x703E,
                    INVENTORY_ITEM_USE = 0x704C,

                    // JOB
                    JOB_JOIN = 0x70E1,
                    JOB_LEAVE = 0x70E2,
                    JOB_ALIAS = 0x70E3,
                    JOB_RANKING = 0x70E4,
                    JOB_OUTCOME = 0x70E5,
                    JOB_PREV_INFO = 0x70E6,
                    JOB_EXPORT_DETAIL = 0x74D4,

                    // LOGOUT
                    LOGOUT = 0x7005,
                    LOGOUT_CANCEL = 0x7006,

                    // MAGICOPTION
                    MAGICOPTION_GRANT = 0x34A9,

                    // OPERATOR
                    OPERATOR_COMMAND = 0x7010,

                    // PARTY
                    PARTY_CREATE = 0x7060,
                    PARTY_LEAVE = 0x7061,
                    PARTY_INVITE = 0x7062,
                    PARTY_KICK = 0x7063,
                    PARTY_MATCHING_FORM = 0x7069,
                    PARTY_MATCHING_CHANGE = 0x706A,
                    PARTY_MATCHING_DELETE = 0x706B,
                    PARTY_MATCHING_LIST = 0x706C,
                    PARTY_MATCHING_JOIN = 0x706D,

                    // QUEST
                    QUEST_TALK = 0x30D4,
                    QUEST_DINGDONG = 0x70D8,
                    QUEST_ABANDON = 0x70D9,
                    QUEST_GATHER_CANCEL = 0x70DB,
                    QUEST_REWAD_SELECT = 0x7515,

                    // SIEGE
                    SIEGE_RETURN = 0x705D,
                    SIEGE_ACTION = 0x705E,

                    // SILK
                    SILK_GACHA_PLAY = 0x7118,
                    SILK_GACHA_EXCHANGE = 0x7119,
                    SILK_HISTORY = 0x711A,
                    SILK_CLIENT_UNKNOWN_1 = 0x7121,

                    // SKILL
                    SKILL_LEARN = 0x70A1,
                    SKILL_MASTERY_LEARN = 0x70A2,
                    SKILL_WITHDRAW = 0x7202,
                    SKILL_MASTERY_WITHDRAW = 0x7203,

                    // STALL
                    STALL_CREATE = 0x70B1,
                    STALL_DESTROY = 0x70B2,
                    STALL_TALK = 0x70B3,
                    STALL_BUY = 0x70B4,
                    STALL_LEAVE = 0x70B5,
                    STALL_UPDATE = 0x70BA,

                    // TAP
                    TAP_INFO = 0x74DF,
                    TAP_UPDATE = 0x74E0,

                    // TELEPORT
                    TELEPORT_DESIGNATE = 0x7059,
                    TELEPORT_USE = 0x705A,
                    TELEPORT_CANCEL = 0x705B;
            }

            public static class Response {
                public const ushort
                    // TEMP
                    CHAR_MOVEMENT_ANGLE = 0xB024,
                    CHAR_STAT = 0x303D,
                    CHAR_SPEED = 0x30D0,
                    CHAR_LEVELUP_EFFECT = 0x3054,
                    CHAR_INFO_UPDATE = 0x304E,
                    CHAR_XP_UPDATE = 0x3056,
                    CHAR_STAT_UPDATE_STR = 0xB050,
                    CHAR_STAT_UPDATE_INT = 0xB051,
                    CHAR_ACTION_DATA = 0xB070,
                    CHAR_ACTION_STATE = 0xB074,
                    CHAR_DATA_START = 0x34A5,
                    CHAR_DATA_CHUNK = 0x3013,
                    CHAR_DATA_END = 0x34A6,
                    CHAR_CELESTIAL_POSITION = 0x3020,
                    CHAR_HANDLE_EFFECT = 0x305C,

                    // ACADEMY
                    ACADEMY_CREATE = 0xB470,
                    ACADEMY_DISBAND = 0xB471,
                    ACADEMY_KICK = 0xB473,
                    ACADEMY_LEAVE = 0xB474,
                    ACADEMY_GRADE = 0xB475,
                    ACADEMY_UPDATE_COMMENT = 0xB477,
                    ACADEMY_HONOR_RANK = 0xB478,
                    ACADEMY_MATCHING_REGISTER = 0xB47A,
                    ACADEMY_MATCHING_CHANGE = 0xB47B,
                    ACADEMY_MATCHING_DELETE = 0xB47C,
                    ACADEMY_MATCHING_LIST = 0xB47D,
                    ACADEMY_MATCHING_JOIN = 0xB47E,
                    ACADEMY_UPDATE = 0x3C80,
                    ACADEMY_INFO = 0x3C81,
                    ACADEMY_UPDATE_BUFF = 0x3C82,
                    ACADEMY_UNKNOWN1 = 0xB472,
                    ACADEMY_UNKNOWN2 = 0xB476,
                    ACADEMY_UNKNOWN3 = 0xB483,
                    ACADEMY_UNKNOWN4 = 0x3C86,
                    ACADEMY_UNKNOWN5 = 0x3C87,

                    // ALCHEMY
                    ALCHEMY_REINFORCE = 0xB150,
                    ALCHEMY_ENCHANT = 0xB151,
                    ALCHEMY_MANUFACTURE = 0xB155,
                    ALCHEMY_CANCELED = 0x3156,
                    ALCHEMY_DISMANTLE = 0xB157,
                    ALCHEMY_SOCKET = 0xB16A,

                    // AUTHENTICATION
                    AUTH = 0xA103,

                    // BATTLEARENA
                    BATTLEARENA_OPERATION = 0x34D2,

                    // CAS
                    CAS_CLIENT = 0xA314,
                    CAS_SERVER_REQUEST = 0x6315,

                    // CHARACTER
                    CHARACTER_SELECTION_JOIN = 0xB001,
                    CHARACTER_SELECTION_ACTION = 0xB007,
                    CHARACTER_SELECTION_RENAME = 0xB450,

                    // CHAT
                    CHAT = 0xB025,
                    CHAT_UPDATE = 0x3026,
                    CHAT_RESTRICT = 0x302D,

                    // COMMUNITY
                    COMMUNITY_FRIEND_ADD = 0xB302,
                    COMMUNITY_FRIEND_DELETE = 0xB304,
                    COMMUNITY_FRIEND_INFO = 0x3305,
                    COMMUNITY_MEMO_OPEN = 0xB308,
                    COMMUNITY_MEMO_SEND = 0xB309,
                    COMMUNITY_MEMO_DELETE = 0xB30A,
                    COMMUNITY_MEMO_LIST = 0xB30B,
                    COMMUNITY_MEMO_SEND_GROUP = 0xB30C,
                    COMMUNITY_BLOCK = 0xB30D,

                    // CONFIG

                    // CONSIGNMENT
                    CONSIGNMENT_DETAIL = 0xB506,
                    CONSIGNMENT_CLOSE = 0xB507,
                    CONSIGNMENT_REGISTER = 0xB508,
                    CONSIGNMENT_UNREGISTER = 0xB509,
                    CONSIGNMENT_BUY = 0xB50A,
                    CONSIGNMENT_SETTLE = 0xB50B,
                    CONSIGNMENT_SEARCH = 0xB50C,
                    CONSIGNMENT_UPDATE = 0x350D,
                    CONSIGNMENT_LIST = 0xB50E,
                    CONSIGNMENT_BUFF_ADD = 0x3530,
                    CONSIGNMENT_BUFF_REMOVE = 0x3531,
                    CONSIGNMENT_BUFF_UPDATE = 0x3532,

                    // COS
                    COS_COMMAND = 0xB0C5,
                    COS_TERMINATE = 0xB0C6,
                    COS_INFO = 0x30C8,
                    COS_UPDATE = 0x30C9,
                    COS_UPDATE_STATE = 0x30CA,
                    COS_UPDATE_RIDESTATE = 0xB0CB,
                    COS_UNSUMMON = 0xB116,
                    COS_NAME = 0xB117,
                    COS_UPDATE_SETTINGS = 0xB420,
                    COS_UNKNOWN1 = 0xB0C7,

                    // ENTITY - IN [W.I.P] ?
                    ENTITY_GROUP_SPAWN_START = 0x3017,
                    ENTITY_GROUP_SPAWN_CHUNK = 0x3019,
                    ENTITY_GROUP_SPAWN_END = 0x3018,
                    ENTITY_SOLO_SPAWN = 0x3015,
                    ENTITY_SOLO_DESPAWN = 0x3016,
                    ENTITY_CHANGE_STATUS = 0x30BF,
                    ENTITY_POTION_UPDATE = 0x3057,
                    ENTITY_MOVEMENT = 0xB021,
                    ENTITY_PICKUP_ITEM_MOVE = 0xB023,
                    ENTITY_PICKUP_ITEM_ANIM = 0x3036,
                    ENTITY_SELECT_OBJECT = 0xB045,
                    ENTITY_NPC_OPEN = 0xB046,
                    ENTITY_NPC_CLOSE = 0xB04B,
                    ENTITY_TICKET = 0x3206,

                    // ENVIRONMENT -- TODO: Double Check.
                    // ENVIRONMENT_CELESTIAL_POSITION = 0x30C8,
                    // ENVIRONMENT_CELESTIAL_UPDATE = 0x30C9,
                    ENVIRONMENT_WEATHER = 0x3809,

                    // EXCHANGE -- TODO: Double Check as well as Request Opcodes.
                    EXCHANGE_START = 0xB081,
                    EXCHANGE_CONFIRM = 0xB082,
                    EXCHANGE_APPROVE = 0xB083,
                    EXCHANGE_CANCEL = 0xB084,
                    EXCHANGE_STARTED = 0x3085,
                    EXCHANGE_CONFIRMED = 0x3086,
                    EXCHANGE_APPROVED = 0x3087,
                    EXCHANGE_CANCELED = 0x3088,
                    EXCHANGE_UPDATE_GOLD = 0x3089,
                    EXCHANGE_UPDATE_ITEMS = 0x308C,

                    // FGW
                    FGW_RECALL_LIST = 0xB519,
                    FGW_RECALL_MEMBER = 0xB51A,
                    FGW_RECALL_REQUEST = 0x741A,
                    FGW_RECALL_RESPONSE = 0xB51C,
                    FGW_EXIT = 0xB51D,
                    FGW_UPDATE = 0x351E,

                    // CTF/FLAGWAR
                    FLAGWAR_UPDATE = 0x34B1,

                    // FRPVP
                    FRPVP_UPDATE = 0xB516,

                    // GAME
                    GAME_NOTIFY = 0x300C,
                    GAME_INVITE = 0x3080,
                    GAME_RESET = 0x35B5,
                    GAME_SERVERTIME = 0x34BE,

                    // GUIDE
                    GUIDE = 0xB0EA,

                    // GUILD
                    GUILD_ENTITY_UPDATE_HOSTILITY = 0x30EF,
                    GUILD_CREATE = 0xB0F0,
                    GUILD_DISBAND = 0xB0F1,
                    GUILD_LEAVE = 0xB0F2,
                    GUILD_INVITE = 0xB0F3,
                    GUILD_KICK = 0xB0F4,
                    GUILD_UPDATE = 0x38F5,
                    GUILD_DONATE_OBSOLETE = 0xB0F6,
                    GUILD_CLIENT_UNKNOWN1 = 0xB0F8,
                    GUILD_UPDATE_NOTICE = 0xB0F9,
                    GUILD_PROMOTE = 0xB0FA,
                    GUILD_UNION_INVITE = 0xB0FB,
                    GUILD_UNION_LEAVE = 0xB0FC,
                    GUILD_UNION_KICK = 0xB0FD,
                    GUILD_UPDATE_SIEGE_AUTH = 0xB0FF,
                    GUILD_ENTITY_UPDATE = 0x30FF,
                    GUILD_ENTITY_REMOVE = 0x3100,
                    GUILD_INFO_BEGIN = 0x34B3,
                    GUILD_INFO_CHUNK = 0x3101,
                    GUILD_INFO_END = 0x34B4,
                    GUILD_UNION_INFO = 0x3102,
                    GUILD_ENTITY_UPDATE_SIEGE_AUTH = 0x3103,
                    GUILD_TRANSFER = 0xB103,
                    GUILD_UPDATE_PERMISSION = 0xB104,
                    GUILD_ELECTION_START = 0xB105,
                    GUILD_ELECTION_PARTICIPATE = 0xB106,
                    GUILD_ELECTION_VOTE = 0xB107,
                    GUILD_ELECTION_UPDATE = 0x3908,
                    GUILD_WAR_INFO = 0x3109,
                    GUILD_WAR_CHUNK = 0xB110,
                    GUILD_WAR_END = 0xB112,
                    GUILD_CLIENT_UNKNOWN2 = 0xB113,
                    GUILD_WAR_REWARD = 0xB114,
                    GUILD_STORAGE_OPEN = 0xB250,
                    GUILD_STORAGE_CLOSE = 0xB251,
                    GUILD_STORAGE_LIST = 0xB252,
                    GUILD_STORAGE_BEGIN = 0x3253,
                    GUILD_STORAGE_END = 0x3254,
                    GUILD_STORAGE_DATA = 0x3255,
                    GUILD_UPDATE_NICKNAME = 0xB256,
                    GUILD_ENTITY_UPDATE_NICKNAME = 0x3256,
                    GUILD_ENTITY_UPDATE_CREST = 0x3257,
                    GUILD_DONATE = 0x7258,
                    GUILD_MERCENARY_ATTR = 0x7259,
                    GUILD_MERCENARY_TERMINATE = 0x725A,
                    GUILD_GP_HISTORY = 0xB501,

                    // INVENTORY
                    INVENTORY_ENTITY_EQUIP = 0x3038,
                    INVENTORY_ENTITY_UNEQUIP = 0x3039,
                    INVENTORY_UPDATE_ITEM_STATS = 0x3040,
                    INVENTORY_ENTITY_EQUIP_TIMER_START = 0x3041,
                    INVENTORY_ENTITY_EQUIP_TIMER_STOP = 0x3042,
                    INVENTORY_STORAGE_INFO_BEGIN = 0x3047,
                    INVENTORY_STORAGE_INFO_END = 0x3048,
                    INVENTORY_STORAGE_INFO_DATA = 0x3049,
                    INVENTORY_UPDATE_ITEM_DURABILITY = 0x3052,
                    INVENTORY_UPDATE_SIZE = 0x3092,
                    INVENTORY_UPDATE_AMMO = 0x3201,
                    INVENTORY_OPERATION = 0xB034,
                    INVENTORY_STORAGE_OPEN = 0xB03C,
                    INVENTORY_ITEM_REPAIR = 0xB03E,
                    INVENTORY_CLIENT_UNKNOWN = 0xB03F,
                    INVENTORY_ITEM_USE = 0xB04C,

                    // JOB
                    JOB_UPDATE_PRICE = 0x30E0,
                    JOB_JOIN = 0xB0E1,
                    JOB_LEAVE = 0xB0E2,
                    JOB_ALIAS = 0xB0E3,
                    JOB_RANKING = 0xB0E4,
                    JOB_OUTCOME = 0xB0E5,
                    JOB_PREV_INFO = 0xB0E6,
                    JOB_UPDATE_EXP = 0x30E6,
                    JOB_COS_DISTANCE = 0x30E7,
                    JOB_UPDATE_SCALE = 0x30E8,
                    JOB_EXPORT_DETAIL = 0xB4D4,
                    JOB_UPDATE_SAFETRADE = 0x34D5,

                    // LOGOUT
                    LOGOUT = 0xB005,
                    LOGOUT_CANCEL = 0xB006,
                    LOGOUT_SUCCESS = 0x300A,

                    // MAGICOPTION
                    MAGICOPTION_GRANT = 0x34AA,

                    // OPERATOR
                    OPERATOR_PUNISHMENT = 0x3405,
                    OPERATOR_COMMAND = 0xB010,

                    // PARTY
                    PARTY_CREATE = 0xB060,
                    PARTY_LEAVE = 0xB061,
                    PARTY_INVITE = 0xB062,
                    PARTY_KICK = 0xB063,
                    PARTY_UPDATE = 0x3864,
                    PARTY_CREATED = 0x3865,
                    PARTY_CREATED_FROM_MATCHING = 0x3065,
                    PARTY_CLIENT_OnJoinPartyAck = 0xB067,
                    PARTY_DISTRIBUTION = 0x3068,
                    PARTY_MATCHING_FORM = 0xB069,
                    PARTY_MATCHING_CHANGE = 0xB06A,
                    PARTY_MATCHING_DELETE = 0xB06B,
                    PARTY_MATCHING_LIST = 0xB06C,
                    PARTY_MATCHING_JOIN = 0xB06D,

                    // PK
                    PK_UPDATE_PENALTY = 0x30CD,
                    PK_UPDATE_DAILY = 0x30CE,
                    PK_UPDATE_LEVEL = 0x30D3,

                    // QUEST
                    QUEST_TALK = 0x30D4,
                    QUEST_UPDATE = 0x30D5,
                    QUEST_MARK_ADD = 0x30D6,
                    QUEST_MARK_REMOVE = 0x30D7,
                    QUEST_DINGDONG = 0xB0D8,
                    QUEST_ABANDON = 0xB0D9,
                    QUEST_GATHER = 0x30DA,
                    QUEST_GATHER_CANCEL = 0xB0DB,
                    QUEST_CAPTURE_RESULT = 0x30DC,
                    QUEST_NOTIFY = 0x30EC,
                    QUEST_REWARD_TALK = 0x3514,
                    QUEST_REWAD_SELECT = 0x3515,
                    QUEST_SCRIPT = 0x3CA2,

                    // SIEGE
                    SIEGE_RETURN = 0xB05D,
                    SIEGE_ACTION = 0xB05E,
                    SIEGE_UPDATE = 0x385F,

                    // SILK
                    SILK_GACHA_PLAY = 0xB118,
                    SILK_GACHA_EXCHANGE = 0xB119,
                    SILK_HISTORY = 0xB11A,
                    SILK_GACHA_ANNOUNCE = 0x3120,
                    SILK_CLIENT_UNKNOWN = 0xB121,
                    SILK_UPDATE = 0x3153,
                    SILK_NOTIFY = 0x3154,

                    // SKILL
                    SKILL_LEARN = 0xB0A1,
                    SKILL_MASTERY_LEARN = 0xB0A2,
                    SKILL_WITHDRAW = 0xB202,
                    SKILL_MASTERY_WITHDRAW = 0xB203,
                    SKILL_WITHDRAW_INFO_WND = 0x3204,
                    SKILL_DATA = 0xB071,
                    SKILL_BUFF_START = 0xB0BD,
                    SKILL_BUFF_END = 0xB072,

                    // STALL
                    STALL_CREATE = 0xB0B1,
                    STALL_DESTROY = 0xB0B2,
                    STALL_TALK = 0xB0B3,
                    STALL_BUY = 0xB0B4,
                    STALL_LEAVE = 0xB0B5,
                    STALL_ENTITY_ACTION = 0x30B7,
                    STALL_ENTITY_CREATE = 0x30B8,
                    STALL_ENTITY_DESTROY = 0x30B9,
                    STALL_UPDATE = 0xB0BA,
                    STALL_ENTITY_NAME = 0x30BB,

                    // TAP
                    TAP_INFO = 0xB4DF,
                    TAP_UPDATE = 0xB4E0,
                    TAP_ICON = 0x34E1,

                    // TELEPORT
                    TELEPORT_DESIGNATE = 0xB059,
                    TELEPORT_USE = 0xB05A,
                    TELEPORT_CANCEL = 0xB05B;
            }
        }

        #endregion
    }
}