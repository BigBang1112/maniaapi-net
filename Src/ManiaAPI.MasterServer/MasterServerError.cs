namespace ManiaAPI.MasterServer;

/// <summary>
/// Error codes returned by the master servers in the <c>&lt;e&gt;&lt;v&gt;</c> element of a failed request.
/// </summary>
/// <remarks>
/// Recovered from <c>CNetMasterServer::GetErrorMessage</c> in TrackMania Forever, ManiaPlanet 4 and Trackmania Turbo,
/// and from the <c>CGameMasterServer::GetErrorMessage</c> override in TrackMania Forever. Several codes share a meaning,
/// so the duplicates have a numbered suffix. The server can still return a code that is not listed here.
/// </remarks>
public enum MasterServerError
{
    /// <summary>No error.</summary>
    NoError = 0,
    /// <summary>Error without a message.</summary>
    Unspecified = 1,
    /// <summary>Internal error.</summary>
    InternalError = 2,
    /// <summary>Unknown error.</summary>
    UnknownError = 3,
    /// <summary>Game unknown.</summary>
    GameUnknown = 4,
    /// <summary>Invalid version.</summary>
    InvalidVersion = 5,
    /// <summary>Invalid language.</summary>
    InvalidLanguage = 6,
    /// <summary>Invalid login.</summary>
    InvalidLogin = 7,
    /// <summary>Invalid password.</summary>
    InvalidPassword = 8,
    /// <summary>Invalid key.</summary>
    InvalidKey = 9,
    /// <summary>Invalid nickname.</summary>
    InvalidNickname = 10,
    /// <summary>Invalid new password.</summary>
    InvalidNewPassword = 11,
    /// <summary>Invalid mail.</summary>
    InvalidMail = 12,
    /// <summary>Login unknown: there is no account with this login.</summary>
    LoginUnknown = 13,
    /// <summary>Player unknown.</summary>
    PlayerUnknown = 14,
    /// <summary>Incorrect password.</summary>
    IncorrectPassword = 15,
    /// <summary>Wrong key.</summary>
    WrongKey = 16,
    /// <summary>Someone has already created an account with this login.</summary>
    AlreadyUsedLogin = 17,
    /// <summary>Not connected.</summary>
    NotConnected = 18,
    /// <summary>Already connected (from a different IP).</summary>
    AlreadyConnected = 19,
    /// <summary>Redirected to a new master server.</summary>
    RedirectedToNewMasterServer = 20,
    /// <summary>No master server.</summary>
    NoMasterServer = 21,
    /// <summary>No mail in the account.</summary>
    NoMailInAccount = 22,
    /// <summary>The server is overloaded, try again later.</summary>
    ServerOverloaded = 23,
    /// <summary>Challenge (map) unknown.</summary>
    ChallengeUnknown = 24,
    /// <summary>Challenge (map) record unknown.</summary>
    ChallengeRecordUnknown = 25,
    /// <summary>Invalid challenge (map).</summary>
    InvalidChallenge = 26,
    /// <summary>Invalid action.</summary>
    InvalidAction = 27,
    /// <summary>Invalid files.</summary>
    InvalidFiles = 28,
    /// <summary>Invalid code.</summary>
    InvalidCode = 29,
    /// <summary>Invalid upload rate.</summary>
    InvalidUploadRate = 30,
    /// <summary>Invalid download rate.</summary>
    InvalidDownloadRate = 31,
    /// <summary>Cannot be your own buddy.</summary>
    CannotBeYourOwnBuddy = 33,
    /// <summary>Already your buddy.</summary>
    AlreadyYourBuddy = 34,
    /// <summary>Buddy not found.</summary>
    BuddyNotFound = 35,
    /// <summary>Self abusing.</summary>
    SelfAbusing = 36,
    /// <summary>Cannot open file.</summary>
    CannotOpenFile = 37,
    /// <summary>Cannot write into file.</summary>
    CannotWriteIntoFile = 38,
    /// <summary>Cannot delete file.</summary>
    CannotDeleteFile = 39,
    /// <summary>Bad zone.</summary>
    BadZone = 40,
    /// <summary>Zone not found.</summary>
    ZoneNotFound = 41,
    /// <summary>Team unknown.</summary>
    TeamUnknown = 42,
    /// <summary>Duplicate teams.</summary>
    DuplicateTeams = 43,
    /// <summary>A challenge (map) with the same name already exists.</summary>
    ChallengeAlreadyExists = 44,
    /// <summary>No challenge (map), or there are no official challenges.</summary>
    NoChallenge = 45,
    /// <summary>Cannot read file.</summary>
    CannotReadFile = 46,
    /// <summary>Cannot share given challenge (map).</summary>
    CannotShareGivenChallenge = 47,
    /// <summary>Invalid date.</summary>
    InvalidDate = 48,
    /// <summary>Invalid record.</summary>
    InvalidRecord = 49,
    /// <summary>Unknown group.</summary>
    UnknownGroup = 50,
    /// <summary>Not in group.</summary>
    NotInGroup = 51,
    /// <summary>Already subscribed.</summary>
    AlreadySubscribed = 52,
    /// <summary>Challenge (map) attempt unknown.</summary>
    ChallengeAttemptUnknown = 53,
    /// <summary>Invalid author.</summary>
    InvalidAuthor = 54,
    /// <summary>Author unknown.</summary>
    AuthorUnknown = 55,
    /// <summary>Invalid challenge (map) name.</summary>
    InvalidChallengeName = 56,
    /// <summary>Invalid environment.</summary>
    InvalidEnvironment = 57,
    /// <summary>Invalid ping.</summary>
    InvalidPing = 58,
    /// <summary>Bad record.</summary>
    BadRecord = 59,
    /// <summary>News unknown.</summary>
    NewsUnknown = 60,
    /// <summary>Cannot change your zone.</summary>
    CannotChangeYourZone = 61,
    /// <summary>Invalid league.</summary>
    InvalidLeague = 62,
    /// <summary>Invalid match ID.</summary>
    InvalidIdMatch = 63,
    /// <summary>Ranking not found.</summary>
    RankingNotFound = 64,
    /// <summary>Match ID unknown.</summary>
    IdMatchUnknown = 65,
    /// <summary>Invalid difficulty.</summary>
    InvalidDifficulty = 66,
    /// <summary>Invalid record type.</summary>
    InvalidRecordType = 67,
    /// <summary>Record type unknown.</summary>
    RecordTypeUnknown = 68,
    /// <summary>Challenge (map) of the week unknown.</summary>
    ChallengeOfTheWeekUnknown = 69,
    /// <summary>Replay unknown.</summary>
    ReplayUnknown = 70,
    /// <summary>Invalid statistics. Codes 71 to 77 all have this meaning.</summary>
    InvalidStatistics = 71,
    /// <summary>Invalid statistics.</summary>
    InvalidStatistics2 = 72,
    /// <summary>Invalid statistics.</summary>
    InvalidStatistics3 = 73,
    /// <summary>Invalid statistics.</summary>
    InvalidStatistics4 = 74,
    /// <summary>Invalid statistics.</summary>
    InvalidStatistics5 = 75,
    /// <summary>Invalid statistics.</summary>
    InvalidStatistics6 = 76,
    /// <summary>Invalid statistics.</summary>
    InvalidStatistics7 = 77,
    /// <summary>Invalid request.</summary>
    InvalidRequest = 78,
    /// <summary>Not enough planets (coppers in TMF).</summary>
    NotEnoughPlanets = 79,
    /// <summary>Wrong login.</summary>
    WrongLogin = 80,
    /// <summary>Invalid mode.</summary>
    InvalidMode = 81,
    /// <summary>Code unknown.</summary>
    CodeUnknown = 82,
    /// <summary>Invalid profile.</summary>
    InvalidProfile = 83,
    /// <summary>Self favouriting.</summary>
    SelfFavouriting = 84,
    /// <summary>The server is already your favorite.</summary>
    ServerAlreadyFavorite = 85,
    /// <summary>Favorite not found.</summary>
    FavoriteNotFound = 86,
    /// <summary>Invalid medals.</summary>
    InvalidMedals = 87,
    /// <summary>Too many planets (coppers in TMF) earned with medals.</summary>
    TooManyPlanetsEarnedWithMedals = 88,
    /// <summary>Bad validation infos.</summary>
    BadValidationInfos = 89,
    /// <summary>Invalid solo account validation.</summary>
    InvalidSoloAccountValidation = 90,
    /// <summary>Bad solo account validation.</summary>
    BadSoloAccountValidation = 91,
    /// <summary>Account not validated: the characters entered do not match.</summary>
    AccountNotValidated = 92,
    /// <summary>Cannot subscribe to more groups.</summary>
    CannotSubscribeToMoreGroups = 93,
    /// <summary>Invalid collection.</summary>
    InvalidCollection = 94,
    /// <summary>A group with the same name already exists.</summary>
    GroupAlreadyExists = 95,
    /// <summary>Same login.</summary>
    SameLogin = 96,
    /// <summary>Transaction unknown.</summary>
    TransactionUnknown = 97,
    /// <summary>Bad payer for this transaction.</summary>
    BadPayerForTransaction = 98,
    /// <summary>Bad payee for this transaction.</summary>
    BadPayeeForTransaction = 99,
    /// <summary>Bad cost for this transaction.</summary>
    BadCostForTransaction = 100,
    /// <summary>Bad message for this transaction.</summary>
    BadMessageForTransaction = 101,
    /// <summary>Already paid.</summary>
    AlreadyPaid = 102,
    /// <summary>Bad packmask.</summary>
    BadPackmask = 103,
    /// <summary>Bad game mode.</summary>
    BadGameMode = 104,
    /// <summary>Corrupted data.</summary>
    CorruptedData = 105,
    /// <summary>Please try again later.</summary>
    TryAgainLater = 106,
    /// <summary>Too many players.</summary>
    TooManyPlayers = 107,
    /// <summary>Not connected.</summary>
    NotConnected2 = 108,
    /// <summary>Invalid object type.</summary>
    InvalidObjectType = 109,
    /// <summary>Invalid page.</summary>
    InvalidPage = 110,
    /// <summary>Invalid page count.</summary>
    InvalidPageCount = 111,
    /// <summary>Invalid crash address.</summary>
    InvalidCrashAddress = 112,
    /// <summary>Invalid exe.</summary>
    InvalidExe = 113,
    /// <summary>Cannot open file.</summary>
    CannotOpenFile2 = 114,
    /// <summary>Cannot write file.</summary>
    CannotWriteFile = 115,
    /// <summary>Invalid file.</summary>
    InvalidFile = 116,
    /// <summary>Cannot download.</summary>
    CannotDownload = 117,
    /// <summary>No file match.</summary>
    NoFileMatch = 118,
    /// <summary>Invalid session ID.</summary>
    InvalidSessionId = 119,
    /// <summary>Not connected.</summary>
    NotConnected3 = 120,
    /// <summary>Incorrect password.</summary>
    IncorrectPassword2 = 121,
    /// <summary>Already connected. Only known to TMF.</summary>
    AlreadyConnected2 = 122,
    /// <summary>Computer banned.</summary>
    ComputerBanned = 123,
    /// <summary>Too many connections: you cannot connect to different accounts simultaneously from the same machine.</summary>
    TooManyConnections = 124,
    /// <summary>Permission denied: this account does not have rights to perform this action.</summary>
    PermissionDenied = 125,
    /// <summary>Connection denied: this account is not allowed to connect.</summary>
    ConnectionDenied = 126,
    /// <summary>Server unknown or disconnected.</summary>
    ServerUnknownOrDisconnected = 127,
    /// <summary>Unknown campaign.</summary>
    UnknownCampaign = 128,
    /// <summary>No scores available.</summary>
    NoScoresAvailable = 129,
    /// <summary>Cannot use (create) groups.</summary>
    CannotUseGroups = 130,
    /// <summary>Cannot subscribe to groups. Only known to TMF.</summary>
    CannotSubscribeToGroups = 131,
    /// <summary>Cannot do official records.</summary>
    CannotDoOfficialRecords = 132,
    /// <summary>The account is not allowed to change its zone. Only known to TMF.</summary>
    CannotChangeZone = 133,
    /// <summary>Cannot use messenger.</summary>
    CannotUseMessenger = 134,
    /// <summary>Cannot find players.</summary>
    CannotFindPlayers = 135,
    /// <summary>Cannot find servers.</summary>
    CannotFindServers = 136,
    /// <summary>Cannot manage buddies.</summary>
    CannotManageBuddies = 137,
    /// <summary>Cannot manage favorites.</summary>
    CannotManageFavorites = 138,
    /// <summary>Invalid moderator login.</summary>
    InvalidModeratorLogin = 139,
    /// <summary>Invitation failed, the server could not send the e-mail.</summary>
    InvitationFailed = 140,
    /// <summary>A player with this e-mail is already invited.</summary>
    AlreadyInvited = 141,
    /// <summary>Already requesting to be your buddy.</summary>
    AlreadyRequestingBuddy = 142,
    /// <summary>Retry later (wait a while before adding a removed buddy).</summary>
    RetryLater = 143,
    /// <summary>Too many buddies.</summary>
    TooManyBuddies = 144,
    /// <summary>Too many favorites.</summary>
    TooManyFavorites = 145,
    /// <summary>Account conversion already asked.</summary>
    ConversionAlreadyAsked = 146,
    /// <summary>Account already converted.</summary>
    AlreadyConverted = 147,
    /// <summary>Cannot do account conversions.</summary>
    CannotDoAccountConversions = 148,
    /// <summary>Invalid int.</summary>
    InvalidInt = 149,
    /// <summary>Invalid medium int.</summary>
    InvalidMediumInt = 150,
    /// <summary>This key is already associated to an account.</summary>
    AlreadyUsedKey = 151,
    /// <summary>Player online.</summary>
    PlayerOnline = 152,
    /// <summary>Invalid cost (coppers in TMF).</summary>
    InvalidCost = 153,
    /// <summary>Bad identification.</summary>
    BadIdentification = 154,
    /// <summary>Deletion denied.</summary>
    DeletionDenied = 155,
    /// <summary>Unknown key or account.</summary>
    UnknownKeyOrAccount = 156,
    /// <summary>Empty parameter.</summary>
    EmptyParameter = 157,
    /// <summary>Unknown key.</summary>
    UnknownKey = 158,
    /// <summary>No more attempts left.</summary>
    NoMoreAttemptsLeft = 159,
    /// <summary>Unknown distro.</summary>
    UnknownDistro = 160,
    /// <summary>Invalid index.</summary>
    InvalidIndex = 161,
    /// <summary>Invalid count.</summary>
    InvalidCount = 162,
    /// <summary>Ladder server limit not empty.</summary>
    LadderServerLimitNotEmpty = 163,
    /// <summary>Cannot create directory.</summary>
    CannotCreateDirectory = 164,
    /// <summary>Cannot change file rights.</summary>
    CannotChangeFileRights = 165,
    /// <summary>Unknown protocol.</summary>
    UnknownProtocol = 166,
    /// <summary>Forbidden protocol.</summary>
    ForbiddenProtocol = 167,
    /// <summary>Forgotten parameter.</summary>
    ForgottenParameter = 168,
    /// <summary>Invalid master server.</summary>
    InvalidMasterServer = 169,
    /// <summary>Unknown master server.</summary>
    UnknownMasterServer = 170,
    /// <summary>Invalid IP.</summary>
    InvalidIp = 171,
    /// <summary>Deprecated version.</summary>
    DeprecatedVersion = 172,
    /// <summary>Unknown version.</summary>
    UnknownVersion = 173,
    /// <summary>Invalid dedicated version.</summary>
    InvalidDedicatedVersion = 174,
    /// <summary>Different IPs and ports counts.</summary>
    DifferentIpsAndPortsCounts = 175,
    /// <summary>Unknown manialink.</summary>
    UnknownManialink = 176,
    /// <summary>Unknown environment.</summary>
    UnknownEnvironment = 177,
    /// <summary>Manialink already exists.</summary>
    ManialinkAlreadyExists = 178,
    /// <summary>Cannot manage tags.</summary>
    CannotManageTags = 179,
    /// <summary>Invalid UCC checksum.</summary>
    InvalidUccChecksum = 180,
    /// <summary>Cannot register UCC.</summary>
    CannotRegisterUcc = 181,
    /// <summary>Already used UCC name.</summary>
    AlreadyUsedUccName = 182,
    /// <summary>Already used UCC checksum.</summary>
    AlreadyUsedUccChecksum = 183,
    /// <summary>Cannot get UCC.</summary>
    CannotGetUcc = 184,
    /// <summary>Invalid UCC host.</summary>
    InvalidUccHost = 185,
    /// <summary>Unknown host.</summary>
    UnknownHost = 186,
    /// <summary>No URL.</summary>
    NoUrl = 187,
    /// <summary>No URL for dependencies.</summary>
    NoUrlForDependencies = 188,
    /// <summary>Unknown UCC.</summary>
    UnknownUcc = 189,
    /// <summary>Host already exists.</summary>
    HostAlreadyExists = 190,
    /// <summary>URL already exists.</summary>
    UrlAlreadyExists = 191,
    /// <summary>File already exists.</summary>
    FileAlreadyExists = 192,
    /// <summary>Unknown abuse report.</summary>
    UnknownAbuseReport = 193,
    /// <summary>Cannot add custom challenge (map).</summary>
    CannotAddCustomChallenge = 194,
    /// <summary>Not enough data.</summary>
    NotEnoughData = 195,
    /// <summary>Community advertising error.</summary>
    CommunityAdvertisingError = 196,
    /// <summary>Unknown title.</summary>
    UnknownTitle = 197,
    /// <summary>Invalid client rand.</summary>
    InvalidClientRand = 198,
    /// <summary>Invalid hardware key.</summary>
    InvalidHardwareKey = 199,
    /// <summary>Not match owner.</summary>
    NotMatchOwner = 200,
    /// <summary>Invalid authentication token. Codes 201 to 204 all have this meaning.</summary>
    InvalidAuthenticationToken = 201,
    /// <summary>Invalid authentication token.</summary>
    InvalidAuthenticationToken2 = 202,
    /// <summary>Invalid authentication token.</summary>
    InvalidAuthenticationToken3 = 203,
    /// <summary>Invalid authentication token.</summary>
    InvalidAuthenticationToken4 = 204,
    /// <summary>Unknown credited package.</summary>
    UnknownCreditedPackage = 205,
    /// <summary>Credited package already added.</summary>
    CreditedPackageAlreadyAdded = 206,
    /// <summary>Invalid package.</summary>
    InvalidPackage = 207,
    /// <summary>Outdated package chunk.</summary>
    OutdatedPackageChunk = 208,
    /// <summary>Cannot invite you.</summary>
    CannotInviteYou = 209,
    /// <summary>Retry later.</summary>
    RetryLater2 = 210,
    /// <summary>Already requesting to join the group.</summary>
    AlreadyRequestingToJoinGroup = 211,
    /// <summary>Cannot add campaign.</summary>
    CannotAddCampaign = 212,
    /// <summary>Cannot add challenge (map).</summary>
    CannotAddChallenge = 213,
    /// <summary>Cannot add challenge (map) to campaign.</summary>
    CannotAddChallengeToCampaign = 214,
    /// <summary>Already uploaded.</summary>
    AlreadyUploaded = 215,
    /// <summary>MySQL error. Only known to TMF.</summary>
    MySqlError = 1000,
    /// <summary>MySQL error. Only known to TMF.</summary>
    MySqlError2 = 1001,
    /// <summary>MySQL error. Only known to TMF.</summary>
    MySqlError3 = 1002,
    /// <summary>MySQL error. Only known to TMF.</summary>
    MySqlError4 = 2000,
}
